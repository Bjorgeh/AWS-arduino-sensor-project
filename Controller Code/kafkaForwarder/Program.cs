using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;

class Program
{
    // AWS Lambda endpoint URL
    static readonly string lambdaUrl = "https://a2qi5icu22wrd2cgswudq6d23a0zgsfl.lambda-url.eu-north-1.on.aws/";

    // Shared HTTP client for sending requests
    static readonly HttpClient httpClient = new HttpClient();

    static async Task Main(string[] args)
    {
        // Kafka consumer configuration
        var config = new ConsumerConfig
        {
            BootstrapServers = "127.0.0.1:9092", // Replace with your Kafka server address
            GroupId = "aws-forwarder",           // Consumer group ID
            AutoOffsetReset = AutoOffsetReset.Earliest // Start reading from the earliest message
        };

        // Create and configure Kafka consumer
        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe("sensor-data"); // Subscribe to topic "sensor-data"

        Console.WriteLine("📡 Listening for Kafka messages...");

        // Infinite loop to continuously listen for messages
        while (true)
        {
            try
            {
                // Wait for new message from Kafka
                var cr = consumer.Consume(CancellationToken.None);
                Console.WriteLine($"📥 Received from Kafka: {cr.Message.Value}");

                // Wrap message content in HTTP JSON body
                var content = new StringContent(cr.Message.Value, Encoding.UTF8, "application/json");

                // Send POST request to AWS Lambda
                var response = await httpClient.PostAsync(lambdaUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                // Log the response from Lambda
                Console.WriteLine($"✅ Forwarded to AWS Lambda: {response.StatusCode} - {responseBody}");
            }
            catch (ConsumeException e)
            {
                // Handle Kafka-specific consumption error
                Console.WriteLine($"❌ Kafka error: {e.Error.Reason}");
            }
            catch (Exception ex)
            {
                // Handle general HTTP or runtime errors
                Console.WriteLine($"❌ HTTP error: {ex.Message}");
            }
        }
    }
}

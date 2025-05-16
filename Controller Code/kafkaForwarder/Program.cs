using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;

class Program
{
    static readonly string lambdaUrl = "https://a2qi5icu22wrd2cgswudq6d23a0zgsfl.lambda-url.eu-north-1.on.aws/";
    static readonly HttpClient httpClient = new HttpClient();

    static async Task Main(string[] args)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "127.0.0.1:9092", // or "Your IP:9092"
            GroupId = "aws-forwarder",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe("sensor-data");

        Console.WriteLine("📡 Listening for Kafka messages...");

        while (true)
        {
            try
            {
                var cr = consumer.Consume(CancellationToken.None);
                Console.WriteLine($"📥 Received from Kafka: {cr.Message.Value}");

                var content = new StringContent(cr.Message.Value, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(lambdaUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"✅ Forwarded to AWS Lambda: {response.StatusCode} - {responseBody}");
            }
            catch (ConsumeException e)
            {
                Console.WriteLine($"❌ Kafka error: {e.Error.Reason}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ HTTP error: {ex.Message}");
            }
        }
    }
}

using System;
using System.IO.Ports;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Confluent.Kafka;

namespace WaterLevelUploader
{
    class Program
    {
        // Serial port for communication with the Arduino
        static SerialPort _serialPort;

        // List to store water level readings temporarily
        static List<int> readings = new List<int>();

        // ID of the device sending data
        static int deviceId = 1;

        // Timers for reading data and sending data
        static Timer readTimer;
        static Timer sendTimer;

        // Kafka producer instance
        static IProducer<Null, string> _kafkaProducer;

        static async Task Main(string[] args)
        {
            // List of possible serial ports where the Arduino might be connected
            string[] possiblePorts = { "/dev/ttyUSB0", "/dev/ttyUSB1" };
            bool portOpened = false;

            // Try to open one of the available serial ports
            foreach (var portName in possiblePorts)
            {
                try
                {
                    _serialPort = new SerialPort(portName, 9600); // Set baud rate
                    _serialPort.NewLine = "\n"; // Use newline character as line delimiter
                    _serialPort.Open(); // Attempt to open the port
                    Console.WriteLine($"Serial port opened on {portName}.");
                    portOpened = true;
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not open {portName}: {ex.Message}");
                }
            }

            // Exit if no serial port could be opened
            if (!portOpened)
            {
                Console.WriteLine("No valid serial port found. Exiting...");
                return;
            }

            // Kafka producer configuration
            var kafkaConfig = new ProducerConfig
            {
                BootstrapServers = "localhost:9092",           // Kafka server address
                SecurityProtocol = SecurityProtocol.Plaintext  // No encryption
            };

            // Build Kafka producer
            _kafkaProducer = new ProducerBuilder<Null, string>(kafkaConfig).Build();

            // Timer to read from Arduino every 60 seconds
            readTimer = new Timer(60000); // 1 minute
            readTimer.Elapsed += ReadFromArduino;
            readTimer.AutoReset = true;
            readTimer.Start();

            // Timer to send averaged data to Kafka every 5 minutes
            sendTimer = new Timer(300000); // 5 minutes
            sendTimer.Elapsed += async (sender, e) => await SendToKafka();
            sendTimer.AutoReset = true;
            sendTimer.Start();

            Console.WriteLine("Started reading and sending... Press Ctrl+C to stop.");

            // Keep the application running indefinitely
            await Task.Delay(-1);
        }

        // Reads the latest line from the Arduino and parses the sensor value
        static void ReadFromArduino(object sender, ElapsedEventArgs e)
        {
            try
            {
                string latestLine = null;

                // Read all lines available in the buffer and keep the last one
                while (_serialPort.BytesToRead > 0)
                {
                    latestLine = _serialPort.ReadLine();
                }

                // Check if the line contains the expected data
                if (latestLine != null && latestLine.Contains("Sensor Value:"))
                {
                    // Extract the numeric value from the line
                    var valueStr = latestLine.Split(':')[1].Trim();
                    if (int.TryParse(valueStr, out int value))
                    {
                        readings.Add(value); // Store the value
                        Console.WriteLine($"[{DateTime.Now}] Latest data: {value}");
                    }
                }
                else
                {
                    Console.WriteLine("No valid data found in buffer.");
                }
            }
            catch (TimeoutException)
            {
                Console.WriteLine("Timeout: No data available from Arduino.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Read error: {ex.Message}");
            }
            finally
            {
                // Clear the input buffer regardless of success or failure
                _serialPort.DiscardInBuffer();
            }
        }

        // Sends the average of collected sensor readings to Kafka as JSON
        static async Task SendToKafka()
        {
            // Skip sending if no readings are available
            if (readings.Count == 0)
            {
                Console.WriteLine("No data to send yet.");
                return;
            }

            // Calculate the average reading
            int avg = (int)Math.Round(readings.Average());
            readings.Clear(); // Clear stored readings after calculating average

            // Create a JSON payload
            var payload = new
            {
                device_id = deviceId,
                water_level = avg,
                timestamp = DateTime.UtcNow.ToString("o") // ISO 8601 format
            };

            var json = JsonSerializer.Serialize(payload);
            Console.WriteLine($"[{DateTime.Now}] Sending to Kafka: {json}");

            try
            {
                // Send the JSON message to Kafka topic "sensor-data"
                var result = await _kafkaProducer.ProduceAsync("sensor-data", new Message<Null, string> { Value = json });
                Console.WriteLine($"✅ Sent to Kafka: {result.TopicPartitionOffset}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Kafka error: {ex.Message}");
            }
        }
    }
}

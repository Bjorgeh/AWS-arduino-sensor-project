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
        // Serial port used to read data from Arduino
        static SerialPort _serialPort;

        // List to store sensor readings
        static List<int> readings = new List<int>();

        // Device ID for identifying the source
        static int deviceId = 1;

        // Timers for reading and sending data
        static Timer readTimer;
        static Timer sendTimer;

        // Kafka producer instance
        static IProducer<Null, string> _kafkaProducer;

        static async Task Main(string[] args)
        {
            // Possible serial ports to connect to
            string[] possiblePorts = { "/dev/ttyUSB0", "/dev/ttyUSB1" };
            bool portOpened = false;

            // Try to open one of the available serial ports
            foreach (var portName in possiblePorts)
            {
                try
                {
                    _serialPort = new SerialPort(portName, 9600);
                    _serialPort.NewLine = "\n"; // Set newline character
                    _serialPort.Open(); // Attempt to open port
                    Console.WriteLine($"Serial port opened on {portName}.");
                    portOpened = true;
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not open {portName}: {ex.Message}");
                }
            }

            // Exit if no port could be opened
            if (!portOpened)
            {
                Console.WriteLine("No valid serial port found. Exiting...");
                return;
            }

            // Configure Kafka producer
            var kafkaConfig = new ProducerConfig
            {
                BootstrapServers = "localhost:9092",
                SecurityProtocol = SecurityProtocol.Plaintext
            };

            _kafkaProducer = new ProducerBuilder<Null, string>(kafkaConfig).Build();

            // Set up timer to read from Arduino every 60 seconds
            readTimer = new Timer(60000);
            readTimer.Elapsed += ReadFromArduino;
            readTimer.AutoReset = true;
            readTimer.Start();

            // Set up timer to send data to Kafka every 5 minutes
            sendTimer = new Timer(300000);
            sendTimer.Elapsed += async (sender, e) => await SendToKafka();
            sendTimer.AutoReset = true;
            sendTimer.Start();

            Console.WriteLine("Started reading and sending... Press Ctrl+C to stop.");

            // Prevent the app from exiting
            await Task.Delay(-1);
        }

        // Read the latest data line from the Arduino
        static void ReadFromArduino(object sender, ElapsedEventArgs e)
        {
            try
            {
                string latestLine = null;

                // Read all lines in buffer, but only keep the latest one
                while (_serialPort.BytesToRead > 0)
                {
                    latestLine = _serialPort.ReadLine();
                }

                // Parse the line if it contains sensor data
                if (latestLine != null && latestLine.Contains("Sensor Value:"))
                {
                    var valueStr = latestLine.Split(':')[1].Trim();
                    if (int.TryParse(valueStr, out int value))
                    {
                        readings.Add(value);
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
                // Always clear the buffer to prevent stale data
                _serialPort.DiscardInBuffer();
            }
        }

        // Send average sensor data to Kafka
        static async Task SendToKafka()
        {
            // Don't send anything if we have no readings
            if (readings.Count == 0)
            {
                Console.WriteLine("No data to send yet.");
                return;
            }

            // Calculate average and clear the readings
            int avg = (int)Math.Round(readings.Average());
            readings.Clear();

            // Create the data payload
            var payload = new
            {
                device_id = deviceId,
                water_level = avg,
                timestamp = DateTime.UtcNow.ToString("o") // ISO 8601 format
            };

            // Serialize to JSON
            var json = JsonSerializer.Serialize(payload);
            Console.WriteLine($"[{DateTime.Now}] Sending to Kafka: {json}");

            try
            {
                // Send the message to Kafka topic "sensor-data"
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

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
        static SerialPort _serialPort;
        static List<int> readings = new List<int>();
        static int deviceId = 1;
        static Timer readTimer;
        static Timer sendTimer;
        static IProducer<Null, string> _kafkaProducer;

        static async Task Main(string[] args)
        {
            string[] possiblePorts = { "/dev/ttyUSB0", "/dev/ttyUSB1" };
            bool portOpened = false;

            foreach (var portName in possiblePorts)
            {
                try
                {
                    _serialPort = new SerialPort(portName, 9600);
                    _serialPort.NewLine = "\n";
                    _serialPort.Open();
                    Console.WriteLine($"Serial port opened on {portName}.");
                    portOpened = true;
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not open {portName}: {ex.Message}");
                }
            }

            if (!portOpened)
            {
                Console.WriteLine("No valid serial port found. Exiting...");
                return;
            }

            // Kafka-setup
            var kafkaConfig = new ProducerConfig
            {
                BootstrapServers = "localhost:9092",
                SecurityProtocol = SecurityProtocol.Plaintext
            };

            _kafkaProducer = new ProducerBuilder<Null, string>(kafkaConfig).Build();

            readTimer = new Timer(60000); // every 1 minute
            readTimer.Elapsed += ReadFromArduino;
            readTimer.AutoReset = true;
            readTimer.Start();

            sendTimer = new Timer(300000); // every 5 minutes
            sendTimer.Elapsed += async (sender, e) => await SendToKafka();
            sendTimer.AutoReset = true;
            sendTimer.Start();

            Console.WriteLine("Started reading and sending... Press Ctrl+C to stop.");
            await Task.Delay(-1);
        }

        static void ReadFromArduino(object sender, ElapsedEventArgs e)
        {
            try
            {
                string latestLine = null;

                while (_serialPort.BytesToRead > 0)
                {
                    latestLine = _serialPort.ReadLine();
                }

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
                _serialPort.DiscardInBuffer(); // flush buffer regardless
            }
        }

        static async Task SendToKafka()
        {
            if (readings.Count == 0)
            {
                Console.WriteLine("No data to send yet.");
                return;
            }

            int avg = (int)Math.Round(readings.Average());
            readings.Clear();

            var payload = new
            {
                device_id = deviceId,
                water_level = avg,
                timestamp = DateTime.UtcNow.ToString("o")
            };

            var json = JsonSerializer.Serialize(payload);
            Console.WriteLine($"[{DateTime.Now}] Sending to Kafka: {json}");

            try
            {
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

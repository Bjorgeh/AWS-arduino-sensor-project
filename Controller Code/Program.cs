using System;
using System.IO.Ports;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace WaterLevelUploader
{
    class Program
    {
        static SerialPort _serialPort;
        static HttpClient _httpClient = new HttpClient();
        static List<int> readings = new List<int>();
        static string lambdaUrl = "https://a2qi5icu22wrd2cgswudq6d23a0zgsfl.lambda-url.eu-north-1.on.aws/";
        static int deviceId = 1;
        static System.Timers.Timer readTimer;
        static System.Timers.Timer sendTimer;

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
                    Console.WriteLine($"Serial-port opened on {portName}.");
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

            readTimer = new System.Timers.Timer(60000); // 1 minute
            readTimer.Elapsed += ReadFromArduino;
            readTimer.AutoReset = true;
            readTimer.Start();

            sendTimer = new System.Timers.Timer(300000); // 5 minutes
            sendTimer.Elapsed += async (sender, e) => await SendToAws();
            sendTimer.AutoReset = true;
            sendTimer.Start();

            Console.WriteLine("Started reading and sending... Press Ctrl+C to stop.");
            await Task.Delay(-1); // Keeps the app running
        }

        static void ReadFromArduino(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                string line = _serialPort.ReadLine();
                if (line.Contains("Sensor Value:"))
                {
                    var valueStr = line.Split(':')[1].Trim();
                    if (int.TryParse(valueStr, out int value))
                    {
                        readings.Add(value);
                        Console.WriteLine($"Read data: {value}");
                    }
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
        }

        static async Task SendToAws()
        {
            if (readings.Count == 0)
            {
                Console.WriteLine("No data to send yet.");
                return;
            }

            int avg = (int)Math.Round(readings.Average());
            readings.Clear();

            Console.WriteLine($"Average: {avg}");
            Console.WriteLine($"Uploading: device_id={deviceId}, water_level={avg}");

            var payload = new
            {
                device_id = deviceId,
                water_level = avg
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(lambdaUrl, content);
                var responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Upload complete: {response.StatusCode} - {responseString}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Upload error: {ex.Message}");
            }
        }
    }
}

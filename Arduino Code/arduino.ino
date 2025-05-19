// Define which analog pin is connected to the water level sensor
#define WATER_SENSOR_PIN A0

// Used for timing between readings
unsigned long previousMillis = 0;

// Interval between sensor readings (in milliseconds)
// 60000 ms = 1 minute
const long interval = 60000;

void setup() {
  // Initialize serial communication at 9600 baud
  Serial.begin(9600);

  // Set the water sensor pin as input
  pinMode(WATER_SENSOR_PIN, INPUT);
}

void loop() {
  // Get the current time in milliseconds
  unsigned long currentMillis = millis();

  // Check if the interval has passed
  if (currentMillis - previousMillis >= interval) {
    // Save the last time a reading was taken
    previousMillis = currentMillis;

    int sum = 0;

    // Take 3 sensor readings to smooth out noise
    for (int i = 0; i < 3; i++) {
      int value = analogRead(WATER_SENSOR_PIN); // Read sensor value
      sum += value;
      delay(50); // Short delay between reads to improve accuracy
    }

    // Calculate the average of the 3 readings
    int average = sum / 3;

    // Output the result to the serial port in the expected format
    Serial.print("Sensor Value: ");
    Serial.println(average);

    // Ensure all data is fully sent before proceeding
    Serial.flush();
  }
}

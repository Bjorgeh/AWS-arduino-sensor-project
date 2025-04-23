#define WATER_SENSOR_PIN A0 // Analog pin for water sensor

unsigned long previousMillis = 0;
const long interval = 60000; // 1 minute (60000 ms)

void setup() {
  Serial.begin(9600);
  pinMode(WATER_SENSOR_PIN, INPUT);
}

void loop() {
  unsigned long currentMillis = millis();
  if (currentMillis - previousMillis >= interval) {
    previousMillis = currentMillis;

    int sum = 0;
    for (int i = 0; i < 3; i++) {
      int value = analogRead(WATER_SENSOR_PIN);
      sum += value;
      delay(50); // litt kortere delay gir raskere responstid
    }

    int average = sum / 3;
    Serial.print("Sensor Value: ");
    Serial.println(average);
    Serial.flush(); // Sikrer at alt er sendt før ny runde
  }
}

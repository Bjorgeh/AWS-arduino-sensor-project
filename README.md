## 💧 Water-Sensor Monitoring With Dashboard – Real-Time Water Level Monitoring

This project provides a complete IoT solution for monitoring liquid levels using an Arduino, a Banana Pi, and a graphical web dashboard with real-time charts and historical data.

---

### 📁 Project Structure

```
/Arduino Code/
└── arduino.ino                 # Arduino code for reading water sensor data

/AWS Lambda Code/
├── SaveToDynamoDB.py          # Lambda function for saving sensor data to DynamoDB
└── GetFromDynamoDB.py         # Lambda function for retrieving data based on time ranges

/Controller Code/
└── Program.cs                 # .NET 8 app running on Banana Pi, reads from Arduino and sends data to AWS

/sensor-dashboard/
├── src/pages/SensorDashboard.tsx   # Frontend dashboard with chart and UI
└── public/index.html               # HTML template for React app
```

---

### ⚙️ Requirements

- **Arduino Mega** with **Water Level Detection Sensor Module**
  - Sensor connected to analog pin **A0**
  - Power: **VCC to 5V**, **GND to GND**
  
  Example:
  ![image](https://github.com/user-attachments/assets/0958f8a3-af73-4b40-9f7f-a69934fc5779)

- **Banana Pi** (or Raspberry Pi) with **.NET 8 installed**
  - Communicates with Arduino via **USB/Serial**
  - Runs .NET app and sends data to AWS Lambda
  
  Example:
  ![image](https://github.com/user-attachments/assets/192584b7-d572-49eb-9c57-8f5794b2ce97)

- **AWS Setup:**
  - DynamoDB table (`WaterLevelTable`)
  - Two Lambda Functions (GET/POST) with Function URLs enabled and CORS allowed
  - IAM Policy: Lambda must have **PutItem** and **Query** permissions for DynamoDB

---

### 🧠 DynamoDB Table Setup

- **Table Name**: `WaterLevelTable`
- **Partition Key**: `device_id` (Number)
- **Sort Key (optional)**: `timestamp` (String – ISO8601 format recommended)
- Additional field: `water_level` (Number)

---

### 🔌 Arduino Wiring

- **Water Level Sensor → Arduino Mega**
  - **Signal (S)** → **A0**
  - **VCC** → **5V**
  - **GND** → **GND**

- Arduino sends sensor values over USB to Banana Pi.

---

### 🚀 .NET App for Banana Pi

- File: `/Controller Code/Program.cs`
- Reads from Arduino via serial port (`/dev/ttyUSB0`)
- Takes measurements every **1 minute**, sends average every **5 minutes** to AWS Lambda POST URL.

---

### 💻 Frontend – Sensor Dashboard (React GUI)

#### Setup:

1. Navigate to the frontend folder:
   ```bash
   cd sensor-dashboard
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Start the app:
   ```bash
   npm start
   ```

Frontend runs at **http://localhost:3000**, fetching data from the Lambda GET URL.

![image](https://github.com/user-attachments/assets/c3340e3b-f254-4b64-b00e-9be2aa01a092)

---

### 🌐 Lambda Function URLs

- **POST (Save Data):**
- Code in /AWS Lambda Code/SaveToDynamoDB.py
  - `https://your-lambda-save-url/`
  - Body:  
    ```json
    { "device_id": 1, "water_level": 245 }
    ```

- **GET (Fetch Data):**
- Code in /AWS Lambda Code/GetFromDynamoDB.py
  - `https://your-lambda-get-url/?device_id=1&range=last_day`
  - Range values: `last_hour`, `last_day`, `last_week`, etc.

---

### 🔧 Setup & Requirements Summary

- **Arduino IDE** is required to upload `/Arduino Code/arduino.ino`.
- Banana Pi requires:
  - **.NET 8 SDK**
  - .NET app can be run via `dotnet run` or as a **systemd service** for auto-start.
- AWS Lambda Function URLs must have **CORS** enabled (for GET).

---

### ✅ Coming Soon

- 📷 Photos of physical wiring
- 🗂️ PDF diagrams of the full setup
- Multi-sensor and device_id support
- Mobile-friendly dashboard enhancements

# 💧 Sensor Monitoring System – Arduino + Banana Pi + AWS 🚀

This project allows you to **read water sensor values** from an **Arduino Mega**, send them through a **.NET app** running on a **Banana Pi**, and store them in **AWS DynamoDB** via **Lambda**. You can also retrieve historical data using a **Lambda GET API**.

---

## 📦 Components Used:
- Arduino Mega + Liquid Level Sensor
- Banana Pi running Linux + .NET 8
- AWS Lambda (Python)
- AWS DynamoDB
- Lambda Function URLs (public APIs)

---

## ⚙️ System Overview:

1. **Arduino Mega** reads water level every **minute** and sends data over **serial (USB)**.
2. **Banana Pi** runs a **.NET app**:
   - Reads values every **minute** from Arduino.
   - Calculates an **average** every **5 minutes**.
   - Sends the average to **AWS Lambda POST** → stores in DynamoDB.
3. **AWS Lambda GET** retrieves data by time range (e.g., last hour, day, week).

---

## 🔧 .NET App Setup on Banana Pi:

- Reads from `/dev/ttyUSB0`.
- Posts data to AWS every 5 minutes.
- Automatically starts on boot via **systemd service**.

### .NET App Features:
- Serial read every **minute**.
- AWS Lambda POST every **5 minutes**:
  - `device_id`: static ID (e.g., 1).
  - `water_level`: averaged value.

---

## 🌐 AWS Lambda Endpoints:

### 1. **POST Lambda URL (Send Data):**
- **URL**: `https://a2qi5icu22wrd2cgswudq6d23a0zgsfl.lambda-url.eu-north-1.on.aws/`
- **Payload Example**:
  ```json
  {
    "device_id": 1,
    "water_level": 512
  }
  ```

- **Stores** data in DynamoDB table: `WaterLevelTable`.

---

### 2. **GET Lambda URL (Retrieve Data):**
- **URL**: `https://bxja4letmr3vymmxfbd5rl2d5m0fshvb.lambda-url.eu-north-1.on.aws/`
- **Query Parameters**:
  - `device_id`: Required (e.g., 1)
  - `range`: last_hour, last_day, last_week, last_month

- **Example**:
  ```
  https://bxja4letmr3vymmxfbd5rl2d5m0fshvb.lambda-url.eu-north-1.on.aws/?device_id=1&range=last_day
  ```

- **Response**:
  ```json
  {
    "data": [
      {
        "device_id": 1,
        "timestamp": "2025-04-23T12:30:00Z",
        "water_level": 512
      },
      ...
    ]
  }
  ```

---

## 🗃️ DynamoDB Table Structure:

- **Table Name**: `WaterLevelTable`
- **Attributes**:
  - `device_id`: Number (Partition Key)
  - `timestamp`: ISO8601 String (Sort Key recommended)
  - `water_level`: Number

---

## 🔐 IAM Permissions:

- Lambda roles need:
  - **AmazonDynamoDBReadOnlyAccess** (for GET Lambda)
  - **AmazonDynamoDBFullAccess** or **PutItem** (for POST Lambda)

---

## ⚡ Systemd Service for Auto-Start:

1. **.NET App Published** using:
   ```bash
   dotnet publish -c Release -r linux-arm64 --self-contained true -o /home/sql/WaterLevelUploader/publish
   ```

2. **Service File** `/etc/systemd/system/waterlevel.service`:
   ```ini
   [Unit]
   Description=Water Level Uploader Service
   After=network.target

   [Service]
   Type=simple
   User=sql
   WorkingDirectory=/home/sql/WaterLevelUploader/publish
   ExecStart=/home/sql/WaterLevelUploader/publish/WaterLevelUploader
   Restart=always
   RestartSec=10

   [Install]
   WantedBy=multi-user.target
   ```

3. **Commands**:
   ```bash
   sudo systemctl daemon-reexec
   sudo systemctl enable waterlevel.service
   sudo systemctl start waterlevel.service
   sudo systemctl status waterlevel.service
   ```

---

## 🔥 Features in Progress:
- More advanced querying (e.g., custom ranges).
- Frontend visualization (graphs from GET API).
- Alerts (e.g., if water levels drop too low).

---

## 👨‍💻 Author:
- Developed by **bjorgen92** with love for embedded systems and cloud automation 💧☁️⚡

---

Let me know if you'd like to tweak or add anything else! 😄

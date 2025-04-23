## 💧 Water-Sensor Monitoring With Dashboard – Real-Time Water Level Monitoring

Dette prosjektet gir deg en komplett IoT-løsning for overvåking av væskenivå ved hjelp av en Arduino, en Banana Pi og en grafisk webapp med live graf og historiske data.

---

### 📁 Prosjektstruktur

```
/Arduino Code/
└── arduino.ino                 # Arduino-kode for lesing av vannsensor

/AWS Lambda Code/
├── SaveToDynamoDB.py          # Lambda-funksjon for å lagre sensorverdier i DynamoDB
└── GetFromDynamoDB.py         # Lambda-funksjon for å hente data basert på tidsintervall

/Controller Code/
└── Program.cs                 # .NET 8 app som kjører på Banana Pi, leser fra Arduino og sender til AWS

/sensor-dashboard/
├── src/pages/SensorDashboard.tsx   # Frontend med graf og UI for sensorvisning
└── public/index.html               # HTML-template for React app
```

---

### ⚙️ Krav og Utstyr

- **Arduino Mega** med **Water Level Detection Sensor Module**
  - Sensoren kobles til analog pinne **A0**
  - Strøm: **VCC til 5V**, **GND til GND**
  
  Eksempel:
  ![image](https://github.com/user-attachments/assets/0958f8a3-af73-4b40-9f7f-a69934fc5779)

- **Banana Pi** (eller Raspberry Pi med .NET 8 installert)
  - Kommuniserer med Arduino via **USB/Serial**
  - Kjører .NET appen og sender data til AWS Lambda
  
  Eksempel:
  ![image](https://github.com/user-attachments/assets/192584b7-d572-49eb-9c57-8f5794b2ce97)

- **AWS Setup:**
  - DynamoDB-tabell (`WaterLevelTable`)
  - Lambda Function URLs (1 for GET, 1 for POST)
  - CORS må aktiveres for GET-funksjonen.
  - IAM Policy: Lambda må ha **PutItem** og **Query** tillatelse.

---

### 🧠 DynamoDB-tabelloppsett

- **Table Name**: `WaterLevelTable`
- **Partition Key**: `device_id` (Number)
- **Sort Key (valgfritt)**: `timestamp` (String – ISO8601 anbefalt)
- Andre felt: `water_level` (Number)

---

### 🔌 Arduino – Koblingsdetaljer

- **Water Level Sensor → Arduino Mega**
  - **Signal (S)** → **A0**
  - **VCC** → **5V**
  - **GND** → **GND**

- Arduino sender sensorverdier over USB til Banana Pi.

---

### 🚀 .NET App for Banana Pi

- Fil: `/Controller Code/Program.cs`
- Les fra Arduino via serial port (`/dev/ttyUSB0`)
- Gjør måling hvert **1. minutt**, sender gjennomsnitt hver **5. minutt** til AWS Lambda POST URL.

---

### 💻 Frontend – Sensor Dashboard (React GUI)

#### Oppsett:

1. Gå til frontend-mappen:
   ```bash
   cd sensor-dashboard
   ```

2. Installer avhengigheter:
   ```bash
   npm install
   ```

3. Start appen:
   ```bash
   npm start
   ```

Frontend kjører på **http://localhost:3000**, og henter data fra Lambda GET URL.

![image](https://github.com/user-attachments/assets/c3340e3b-f254-4b64-b00e-9be2aa01a092)

---

### 🌐 Lambda Function URLs

- **POST (Lagre data):**
  - `https://your-lambda-save-url/`
  - Body:  
    ```json
    { "device_id": 1, "water_level": 245 }
    ```

- **GET (Hent data):**
  - `https://your-lambda-get-url/?device_id=1&range=last_day`
  - Range-verdier: `last_hour`, `last_day`, `last_week`, etc.

---

### 🔧 Settings og Forutsetninger

- **Arduino IDE** må brukes for å laste opp `/Arduino Code/arduino.ino`.
- Banana Pi må ha:
  - **.NET 8 SDK**
  - Kjør .NET app med `dotnet run` eller som en **systemd service**.
- AWS Lambda Function URLs må ha CORS aktivert (for GET).

---

### ✅ Kommer snart

- 📷 Bilder av koblinger
- 🗂️ PDF-skjema for komplett oppsett
- Flere sensorer og device_id støtte
- Evt. mobiltilpasset dashboard

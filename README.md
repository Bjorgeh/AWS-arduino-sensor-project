## 💧 Water-Sensor Monitoring With Dashboard – Real-Time Water Level Monitoring

Dette prosjektet gir deg en komplett IoT-løsning for overvåking av væskenivå ved hjelp av en Arduino og en grafisk webapp.

---

### 📁 Prosjektstruktur

```
/Arduino Code/
└── arduino.ino                 # Koden lastes opp på Arduino Mega

/AWS Lambda Code/
├── SaveToDynamoDB.py          # Lambda-funksjon for å lagre sensorverdier
└── GetFromDynamoDB.py         # Lambda-funksjon for å hente historiske data

/Controller Code/
└── Program.cs                 # .NET 8 app som leser fra Arduino og sender til AWS

/sensor-dashboard/
├── src/pages/SensorDashboard.tsx   # Frontend dashboard med graf og UI
└── public/index.html               # Tilpasset HTML-template
```

---

### ⚙️ Krever følgende

- Arduino Mega med Liquid Level Sensor
- Banana Pi eller annen enhet med .NET 8 installert
- AWS-konto med:
  - DynamoDB-tabell (`WaterLevelTable`)
  - To Lambda-funksjoner (GET/POST) med Function URL aktivert og CORS tillatt
- React frontend med Recharts
- Internettilgang på controller-enheten (for å sende data)

---

### 🧠 DynamoDB-oppsett

- Tabellnavn: `WaterLevelTable`
- Primærnøkkel: `device_id` (Number)
- Sorteringsnøkkel (om brukt): `timestamp` (String eller ISO8601)
- Sørg for at IAM-rollen til Lambda har tillatelse til `dynamodb:PutItem` og `dynamodb:Query`.

---

### 💻 Frontend – Sensor Dashboard (React GUI)

#### 1. Gå til frontend-mappen:

```bash
cd sensor-dashboard
```

#### 2. Installer avhengigheter:

```bash
npm install
```

#### 3. Start utviklingsserver:

```bash
npm start
```

Frontend kjører på `http://localhost:3000` og henter data fra Lambda GET-endepunktet automatisk.

---

### 🚀 .NET App for Banana Pi

- Kildekode: `/Controller Code/Program.cs`
- Den leser fra Arduino via serial, sender data til Lambda hvert 5. minutt.

---

### 🔌 Arduino-kobling

Koblingsskjema og bilder kommer snart!
- Sensor koblet til `A0` (analog)
- GND og VCC til GND og 5V

---

### 🌐 Lambda-endepunkter

**POST (Save)**  
`https://your-lambda-save-url/...`

**GET (Fetch by range)**  
`https://your-lambda-get-url/?device_id=1&range=last_day`

---

### ✅ Kommer snart

- Bilder av fysiske koblinger
- PDF-skjema over hele oppsettet
- Mulighet for flere sensorer og enhetsvalg i GUI

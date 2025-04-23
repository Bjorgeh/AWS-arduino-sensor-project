# 💻 Sensor Dashboard (React GUI)

This is the frontend dashboard for the Water Sensor Monitoring system. It visualizes water level data in real-time and supports historical data filtering via AWS Lambda.

---

## 🚀 Features

- Line chart of sensor data using **Recharts**
- Dropdown to select time ranges (last hour, day, week, etc.)
- Status display (Dry / Moist / Wet) with color indicator
- Modern responsive layout using plain CSS

---

## 📦 Getting Started

Make sure you have Node.js and npm installed.

### 1. Install dependencies:

```bash
npm install
```

### 2. Start the development server:

```bash
npm start
```

This runs the app in development mode.  
Visit [http://localhost:3000](http://localhost:3000) in your browser.

- The page reloads automatically on code changes.
- Console will show any lint or runtime errors.

---

## 🔧 Environment Setup

- This app fetches data from an **AWS Lambda Function URL**.
- Make sure CORS is enabled on your Lambda (GET endpoint).
- The expected URL pattern:

```
https://your-lambda-get-url/?device_id=1&range=last_day
```

Available `range` values:
- `last_hour`
- `last_6_hours`
- `last_12_hours`
- `last_day`
- `last_week`
- `last_month`

You can modify the `lambdaUrl` inside:
```tsx
/src/pages/SensorDashboard.tsx
```

---

## 🛠️ Build for Production

To build the project for production:

```bash
npm run build
```

The app will be optimized and bundled into the `build/` folder, ready to be deployed.

---

## 📚 Learn More

- [React Documentation](https://reactjs.org/)
- [Recharts Documentation](https://recharts.org/)
- [DynamoDB Time-Based Queries](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/Query.html)

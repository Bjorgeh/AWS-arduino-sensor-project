import React, { useEffect, useState, useCallback, useRef } from 'react';
import {
  LineChart, Line, XAxis, YAxis, Tooltip,
  ResponsiveContainer, CartesianGrid, Legend
} from 'recharts';
import './SensorDashboard.css';

interface DataPoint {
  timestamp: string;
  water_level: number;
}

const imageDescriptions = [
"Banana Pi's placed in 3D-printed case, with a 5V power supply and a local 1gb connection.",
"Arduino with the water level sensor placed in water for testing.",
"Water sensor in a glass of water.",
"Sensor lights up and is active + best friend in the background.",
"Arduino (Actually a GeekCreit) with the water level sensor connected.",
"Rack top with Arduino connected to a Banana Pi via USB.",
"Full rack, machine on top is for another server site (will remove soon).",
"(Sorry for the cables) Rack info: Mikrotik Router, Managed HP 1810-24G switch, 5 Banana Pi M4s, 2 HP 8300 as NAS and Rack controller with cockpit, GreenCell UPS. ",
];


const timeRanges = [
  { label: 'Last Hour', value: 'last_hour' },
  { label: 'Last 6 Hours', value: 'last_6_hours' },
  { label: 'Last 12 Hours', value: 'last_12_hours' },
  { label: 'Last Day', value: 'last_day' },
  { label: 'Last Week', value: 'last_week' },
  { label: 'Last Month', value: 'last_month' },
];

const lambdaUrl = 'https://bxja4letmr3vymmxfbd5rl2d5m0fshvb.lambda-url.eu-north-1.on.aws/';
const Images = [
  '/images/1.jpg', '/images/2.jpg', '/images/3.jpg', '/images/4.jpg',
  '/images/5.jpg', '/images/6.jpg', '/images/7.jpg', '/images/8.jpg',
];

const SensorDashboard: React.FC = () => {
  const [data, setData] = useState<DataPoint[]>([]);
  const [loading, setLoading] = useState(false);
  const [range, setRange] = useState('last_day');
  const [latestValue, setLatestValue] = useState<number | null>(null);
  const [currentImage, setCurrentImage] = useState(0);
  const scheduledFetchRef = useRef<NodeJS.Timeout | null>(null);

  const fetchData = useCallback(async () => {
    setLoading(true);
    try {
      const response = await fetch(`${lambdaUrl}?device_id=1&range=${range}`);
      const json = await response.json();

      const formatted = json.data
        .map((item: any) => ({
          timestamp: new Date(item.timestamp).toLocaleString(),
          timestampRaw: new Date(item.timestamp),
          water_level: item.water_level,
        }))
        .sort((a: { timestampRaw: Date }, b: { timestampRaw: Date }) =>
          a.timestampRaw.getTime() - b.timestampRaw.getTime()
        )
        .map(({ timestamp, water_level }: { timestamp: string; water_level: number }) => ({
          timestamp,
          water_level,
        }));

      setData(formatted);
      if (formatted.length > 0) {
        setLatestValue(formatted[formatted.length - 1].water_level);
        scheduleNextFetch(formatted[formatted.length - 1].timestampRaw);
      }
    } catch (error) {
      console.error('Error fetching data:', error);
    }
    setLoading(false);
  }, [range]);

  const scheduleNextFetch = (lastTimestamp: Date) => {
    if (scheduledFetchRef.current) {
      clearTimeout(scheduledFetchRef.current);
    }
    const now = new Date().getTime();
    const lastDataTime = lastTimestamp.getTime();
    const nextFetchTime = lastDataTime + (5 * 60 * 1000) + 10000; // 5 min 10 sek
    let delay = nextFetchTime - now;

    if (delay < 0) {
      delay = 5 * 60 * 1000; // Hvis vi er for sent ute, bare vent 5 minutter
    }

    scheduledFetchRef.current = setTimeout(() => {
      fetchData();
    }, delay);
  };

  useEffect(() => {
    fetchData();
    return () => {
      if (scheduledFetchRef.current) {
        clearTimeout(scheduledFetchRef.current);
      }
    };
  }, [fetchData]);



  const getStatus = () => {
    if (latestValue === null) return 'No Data';
    if (latestValue <= 10) return 'Dry';
    if (latestValue <= 300) return 'Moist';
    return 'Wet';
  };

  const getStatusColor = () => {
    if (latestValue === null) return '#888';
    if (latestValue <= 10) return '#ff4d4f';
    if (latestValue <= 300) return '#ffc107';
    return '#4caf50';
  };

  const goPrev = () => setCurrentImage((prev) => (prev - 1 + Images.length) % Images.length);
  const goNext = () => setCurrentImage((prev) => (prev + 1) % Images.length);

  return (
    <div className="dashboard-container">
      <h1 className="dashboard-title">💧 Sensor Dashboard</h1>

      <div className="controls-container">
        <label htmlFor="range-select">Select Time Range:</label>
        <select id="range-select" value={range} onChange={(e) => setRange(e.target.value)}>
          {timeRanges.map((r) => (
            <option key={r.value} value={r.value}>{r.label}</option>
          ))}
        </select>
      </div>

      <div className="status-container">
        <h2>Status: <span style={{ color: getStatusColor() }}>{getStatus()}</span></h2>
        {latestValue !== null && <p>Latest Value: {latestValue}</p>}
      </div>

      {loading ? (
        <p className="loading-text">🔄 Loading data...</p>
      ) : (
        <ResponsiveContainer width="100%" height={400}>
          <LineChart data={data}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="timestamp" />
            <YAxis />
            <Tooltip />
            <Legend />
            <Line type="monotone" dataKey="water_level" stroke="#00bcd4" strokeWidth={2} dot={false} />
          </LineChart>
        </ResponsiveContainer>
      )}
      <h2 className="gallery-title">🔌 Wiring & System Overview</h2>
      <div className="diagrams-container">
        <div className="diagram-card">
          <h3>Arduino Sensor Wiring</h3>
          <img src="/images/arduino_overview.png" alt="Arduino Wiring" className="diagram-image" />
          <p>
            This diagram illustrates the physical wiring between the water level sensor and the Arduino.
            The sensor's signal pin is connected to analog input A0 on the Arduino. Power is supplied via
            the 5V and GND pins. This setup enables the Arduino to continuously read analog values representing
            the current water level and transmit them to the Banana Pi via USB.
          </p>
          <ul>
            <li><strong>VCC</strong> → 5V on Arduino</li>
            <li><strong>GND</strong> → GND on Arduino</li>
            <li><strong>Signal</strong> → A0 on Arduino</li>
          </ul>
        </div>
        <div className="diagram-card">
          <h3>System Overview</h3>
          <img src="/images/system_overview.png" alt="System Overview" className="diagram-image" />
          <p>
            This high-level diagram shows the complete IoT system in action. The water sensor sends readings to
            the Arduino, which transmits them over a serial USB connection to the Banana Pi M4. The Banana Pi
            runs a .NET application that reads sensor data, calculates average levels at regular intervals,
            and sends this data to an AWS Lambda function via HTTP POST. AWS Lambda stores the data in DynamoDB.
            Finally, this dashboard fetches and visualizes the data dynamically, providing live monitoring of
            water levels in an elegant and user-friendly interface.
          </p>
        </div>
      </div>
      <h2 className="gallery-title">📷 Setup</h2>
        <div className="carousel-container">
          <button className="carousel-button" onClick={goPrev}>←</button>
          <img src={Images[currentImage]} alt={`setup ${currentImage + 1}`} className="carousel-image" />
          <button className="carousel-button" onClick={goNext}>→</button>
        </div>
        <p className="carousel-description">{imageDescriptions[currentImage]}</p>

    </div>
  );
};

export default SensorDashboard;

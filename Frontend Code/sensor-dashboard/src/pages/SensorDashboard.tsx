import React, { useEffect, useState, useCallback } from 'react';
import {
  LineChart, Line, XAxis, YAxis, Tooltip,
  ResponsiveContainer, CartesianGrid, Legend
} from 'recharts';
import './SensorDashboard.css';

interface DataPoint {
  timestamp: string;
  water_level: number;
}

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
      }
    } catch (error) {
      console.error('Error fetching data:', error);
    }
    setLoading(false);
  }, [range]);

  // 👇 Sørg for at fetchData faktisk blir brukt
  useEffect(() => {
    fetchData();
  }, [fetchData]);

  // Bildekarusell autoplay
  useEffect(() => {
    const interval = setInterval(() => {
      setCurrentImage((prev) => (prev + 1) % Images.length);
    }, 4000);
    return () => clearInterval(interval);
  }, []);

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

      <h2 className="gallery-title">📷 Setup</h2>
      <div className="carousel-container">
        <button className="carousel-button" onClick={goPrev}>←</button>
        <img src={Images[currentImage]} alt={`setup ${currentImage + 1}`} className="carousel-image" />
        <button className="carousel-button" onClick={goNext}>→</button>
      </div>
    </div>
  );
};

export default SensorDashboard;

import React, { useEffect, useState, useCallback } from 'react';
import { LineChart, Line, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid, Legend } from 'recharts';
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

const SensorDashboard: React.FC = () => {
  const [data, setData] = useState<DataPoint[]>([]);
  const [loading, setLoading] = useState(false);
  const [range, setRange] = useState('last_day');
  const [latestValue, setLatestValue] = useState<number | null>(null);

  const fetchData = useCallback(async () => {
    setLoading(true);
    try {
      const response = await fetch(`${lambdaUrl}?device_id=1&range=${range}`);
      const json = await response.json();
      const formatted = json.data.map((item: any) => ({
        timestamp: new Date(item.timestamp).toLocaleString(),
        water_level: item.water_level,
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

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const getStatus = () => {
    if (latestValue === null) return 'No Data';
    if (latestValue <= 10) return 'Dry';
    if (latestValue <= 300) return 'Moist';
    return 'Wet';
  };

  const getStatusColor = () => {
    if (latestValue === null) return '#888';
    if (latestValue <= 10) return '#ff4d4f'; // Red for Dry
    if (latestValue <= 300) return '#ffc107'; // Amber for Moist
    return '#4caf50'; // Green for Wet
  };

  return (
    <div className="dashboard-container">
      <h1 className="dashboard-title">💧 Sensor Dashboard</h1>

      <div className="controls-container">
        <label htmlFor="range-select">Select Time Range:</label>
        <select id="range-select" value={range} onChange={(e) => setRange(e.target.value)}>
          {timeRanges.map((r) => (
            <option key={r.value} value={r.value}>
              {r.label}
            </option>
          ))}
        </select>
      </div>

      <div className="status-container">
        <h2>
          Status: <span style={{ color: getStatusColor() }}>{getStatus()}</span>
        </h2>
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
    </div>
  );
};

export default SensorDashboard;

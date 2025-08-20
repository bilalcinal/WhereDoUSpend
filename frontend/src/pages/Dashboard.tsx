import { useEffect, useState } from 'react';
import dayjs from 'dayjs';
import { reportsApi } from '../api';
import { PieChart, Pie, Cell, ResponsiveContainer, Legend, Tooltip, LineChart, Line, XAxis, YAxis, CartesianGrid } from 'recharts';

const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884D8', '#82CA9D'];

export default function Dashboard() {
  const [summary, setSummary] = useState<any[]>([]);
  const [cashflow, setCashflow] = useState<any[]>([]);
  const year = dayjs().year();
  const month = dayjs().month() + 1;
  const from = dayjs().startOf('month').format('YYYY-MM-DD');
  const to = dayjs().endOf('month').format('YYYY-MM-DD');

  useEffect(() => {
    const load = async () => {
      try {
        const [sum, cf] = await Promise.all([
          reportsApi.summary(year, month),
          reportsApi.cashflow(from, to, 'day'),
        ]);
        setSummary(sum);
        setCashflow(cf);
      } catch (e) {
        setSummary([]);
        setCashflow([]);
        // Optionally log
        console.warn('Failed to load dashboard data', e);
      }
    };
    load();
  }, []);

  const expenseData = summary.filter(s => s.type === 2).map((s: any) => ({ name: s.category, value: Math.abs(s.total) }));

  return (
    <div className="section">
      <h2>Dashboard - {dayjs().format('MMMM YYYY')}</h2>
      <div className="chart-container">
        <ResponsiveContainer width="100%" height={300}>
          <PieChart>
            <Pie data={expenseData} cx="50%" cy="50%" outerRadius={80} dataKey="value" label={({ name, percent = 0 }) => `${name} ${(percent * 100).toFixed(0)}%`}>
              {expenseData.map((_, index) => (
                <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
              ))}
            </Pie>
            <Tooltip />
            <Legend />
          </PieChart>
        </ResponsiveContainer>
      </div>

      <div className="chart-container">
        <ResponsiveContainer width="100%" height={300}>
          <LineChart data={cashflow}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="period" tickFormatter={(v) => dayjs(v).format('MM-DD')} />
            <YAxis />
            <Tooltip labelFormatter={(v) => dayjs(v).format('YYYY-MM-DD')} />
            <Legend />
            <Line type="monotone" dataKey="net" stroke="#8884d8" name="Net" />
          </LineChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
} 
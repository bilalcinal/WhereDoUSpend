import { useEffect, useState } from 'react';
import dayjs from 'dayjs';
import { budgetsApi, categoriesApi } from '../api';

export default function Budgets() {
  const [items, setItems] = useState<any[]>([]);
  const [categories, setCategories] = useState<any[]>([]);
  const [year, setYear] = useState(dayjs().year());
  const [month, setMonth] = useState(dayjs().month() + 1);
  const [form, setForm] = useState({ categoryId: 0, amount: 0 });

  const load = async () => {
    const [list, cats] = await Promise.all([
      budgetsApi.list(year, month),
      categoriesApi.list(),
    ]);
    setItems(list);
    setCategories(cats);
  };

  useEffect(() => { load(); }, [year, month]);

  const create = async () => {
    if (!form.categoryId) return;
    await budgetsApi.create({ categoryId: form.categoryId, year, month, amount: form.amount });
    setForm({ categoryId: 0, amount: 0 });
    await load();
  };

  return (
    <div className="section">
      <h2>Budgets</h2>
      <div className="form-row">
        <input type="number" value={year} onChange={(e) => setYear(parseInt(e.target.value))} />
        <input type="number" value={month} onChange={(e) => setMonth(parseInt(e.target.value))} />
      </div>
      <div className="transaction-form">
        <div className="form-row">
          <select value={form.categoryId} onChange={(e) => setForm({ ...form, categoryId: parseInt(e.target.value) })}>
            <option value={0}>Select Category</option>
            {categories.map(c => (<option key={c.id} value={c.id}>{c.name}</option>))}
          </select>
          <input type="number" step="0.01" value={form.amount} onChange={(e) => setForm({ ...form, amount: parseFloat(e.target.value) || 0 })} placeholder="Amount" />
          <button onClick={create}>Add</button>
        </div>
      </div>

      <div className="transactions-table">
        <table>
          <thead><tr><th>Category</th><th>Amount</th><th>Actions</th></tr></thead>
          <tbody>
            {items.map(b => (
              <tr key={b.id}>
                <td>{categories.find(c => c.id === b.categoryId)?.name ?? b.categoryId}</td>
                <td>${b.amount.toFixed(2)}</td>
                <td>
                  <button className="delete-btn" onClick={async () => { await budgetsApi.delete(b.id); await load(); }}>Delete</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
} 
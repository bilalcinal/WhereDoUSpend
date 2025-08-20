import { useEffect, useState } from 'react';
import type { Account } from '../api';
import { accountsApi } from '../api';

export default function Accounts() {
  const [items, setItems] = useState<Account[]>([]);
  const [form, setForm] = useState({ name: '', type: 1, currency: 'USD', openingBalance: 0 });

  const load = async () => {
    const list = await accountsApi.list();
    setItems(list);
  };

  useEffect(() => { load(); }, []);

  const create = async () => {
    if (!form.name) return;
    await accountsApi.create(form);
    setForm({ name: '', type: 1, currency: 'USD', openingBalance: 0 });
    await load();
  };

  return (
    <div className="section">
      <h2>Accounts</h2>
      <div className="transaction-form">
        <div className="form-row">
          <input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} placeholder="Name" />
          <select value={form.type} onChange={(e) => setForm({ ...form, type: parseInt(e.target.value) })}>
            <option value={1}>Cash</option>
            <option value={2}>Bank</option>
            <option value={3}>Card</option>
          </select>
          <input value={form.currency} onChange={(e) => setForm({ ...form, currency: e.target.value.toUpperCase().slice(0,3) })} placeholder="Currency" />
          <input type="number" step="0.01" value={form.openingBalance} onChange={(e) => setForm({ ...form, openingBalance: parseFloat(e.target.value) || 0 })} placeholder="Opening Balance" />
          <button onClick={create}>Add</button>
        </div>
      </div>

      <div className="transactions-table">
        <table>
          <thead>
            <tr><th>Name</th><th>Type</th><th>Currency</th><th>Opening</th><th>Actions</th></tr>
          </thead>
          <tbody>
            {items.map(a => (
              <tr key={a.id}>
                <td>{a.name}</td>
                <td>{a.type === 1 ? 'Cash' : a.type === 2 ? 'Bank' : 'Card'}</td>
                <td>{a.currency}</td>
                <td>{a.openingBalance.toFixed(2)}</td>
                <td>
                  <button className="delete-btn" onClick={async () => { await accountsApi.archive(a.id); await load(); }}>Archive</button>
                  <button className="delete-btn" onClick={async () => { await accountsApi.delete(a.id); await load(); }}>Delete</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
} 
import { useEffect, useState } from 'react';
import dayjs from 'dayjs';
import { recurringApi, categoriesApi, accountsApi } from '../api';

export default function Recurring() {
  const [items, setItems] = useState<any[]>([]);
  const [categories, setCategories] = useState<any[]>([]);
  const [accounts, setAccounts] = useState<any[]>([]);
  const [form, setForm] = useState({ accountId: undefined as number | undefined, categoryId: 0, amount: 0, type: 2 as 1 | 2, cadence: 3, nextRunAt: dayjs().format('YYYY-MM-DDTHH:mm'), note: '' });

  const load = async () => {
    const [list, cats, accs] = await Promise.all([
      recurringApi.list(),
      categoriesApi.list(),
      accountsApi.list(),
    ]);
    setItems(list);
    setCategories(cats);
    setAccounts(accs);
  };

  useEffect(() => { load(); }, []);

  const create = async () => {
    if (!form.categoryId || form.amount <= 0) return;
    await recurringApi.create(form as any);
    setForm({ accountId: undefined, categoryId: 0, amount: 0, type: 2, cadence: 3, nextRunAt: dayjs().format('YYYY-MM-DDTHH:mm'), note: '' });
    await load();
  };

  return (
    <div className="section">
      <h2>Recurring</h2>
      <div className="transaction-form">
        <div className="form-row">
          <select value={form.accountId ?? ''} onChange={(e) => setForm({ ...form, accountId: e.target.value ? parseInt(e.target.value) : undefined })}>
            <option value="">No Account</option>
            {accounts.map((a) => (<option key={a.id} value={a.id}>{a.name}</option>))}
          </select>
          <select value={form.categoryId} onChange={(e) => setForm({ ...form, categoryId: parseInt(e.target.value) })}>
            <option value={0}>Select Category</option>
            {categories.map((c) => (<option key={c.id} value={c.id}>{c.name}</option>))}
          </select>
          <input type="number" step="0.01" value={form.amount} onChange={(e) => setForm({ ...form, amount: parseFloat(e.target.value) || 0 })} placeholder="Amount" />
          <select value={form.type} onChange={(e) => setForm({ ...form, type: parseInt(e.target.value) as 1 | 2 })}>
            <option value={2}>Expense</option>
            <option value={1}>Income</option>
          </select>
          <select value={form.cadence} onChange={(e) => setForm({ ...form, cadence: parseInt(e.target.value) })}>
            <option value={1}>Daily</option>
            <option value={2}>Weekly</option>
            <option value={3}>Monthly</option>
          </select>
          <input type="datetime-local" value={form.nextRunAt} onChange={(e) => setForm({ ...form, nextRunAt: e.target.value })} />
          <input value={form.note} onChange={(e) => setForm({ ...form, note: e.target.value })} placeholder="Note" />
          <button onClick={create}>Add</button>
        </div>
      </div>

      <div className="form-row" style={{ marginTop: 10 }}>
        <button onClick={async () => { await recurringApi.runDue(); await load(); }}>Run Due Now</button>
      </div>

      <div className="transactions-table">
        <table>
          <thead><tr><th>Next Run</th><th>Account</th><th>Category</th><th>Type</th><th>Amount</th><th>Cadence</th><th>Note</th><th>Actions</th></tr></thead>
          <tbody>
            {items.map(r => (
              <tr key={r.id}>
                <td>{dayjs(r.nextRunAt).format('YYYY-MM-DD HH:mm')}</td>
                <td>{r.accountId ?? '-'}</td>
                <td>{r.categoryId}</td>
                <td className={r.type === 1 ? 'income' : 'expense'}>{r.type === 1 ? 'Income' : 'Expense'}</td>
                <td className={r.type === 1 ? 'income' : 'expense'}>${r.amount.toFixed(2)}</td>
                <td>{r.cadence === 1 ? 'Daily' : r.cadence === 2 ? 'Weekly' : 'Monthly'}</td>
                <td>{r.note ?? '-'}</td>
                <td><button className="delete-btn" onClick={async () => { await recurringApi.delete(r.id); await load(); }}>Delete</button></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
} 
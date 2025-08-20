import { useEffect, useState } from 'react';
import dayjs from 'dayjs';
import { transactionsApi, categoriesApi, accountsApi } from '../api';

export default function Transactions() {
  const [items, setItems] = useState<any[]>([]);
  const [categories, setCategories] = useState<any[]>([]);
  const [accounts, setAccounts] = useState<any[]>([]);
  const [form, setForm] = useState({ amount: 0, type: 2 as 1 | 2, date: dayjs().format('YYYY-MM-DDTHH:mm'), note: '', accountId: undefined as number | undefined, categoryId: undefined as number | undefined });

  const load = async () => {
    const [tx, cats, accs] = await Promise.all([
      transactionsApi.list({ pageSize: 100 }).then(res => res.items ?? res),
      categoriesApi.list(),
      accountsApi.list(),
    ]);
    setItems(tx);
    setCategories(cats);
    setAccounts(accs);
  };

  useEffect(() => { load(); }, []);

  const create = async () => {
    if (form.amount <= 0) return;
    await transactionsApi.create(form);
    setForm({ amount: 0, type: 2, date: dayjs().format('YYYY-MM-DDTHH:mm'), note: '', accountId: undefined, categoryId: undefined });
    await load();
  };

  return (
    <div className="section">
      <h2>Transactions</h2>
      <div className="transaction-form">
        <div className="form-row">
          <input type="number" step="0.01" value={form.amount} onChange={(e) => setForm({ ...form, amount: parseFloat(e.target.value) || 0 })} placeholder="Amount" />
          <select value={form.type} onChange={(e) => setForm({ ...form, type: parseInt(e.target.value) as 1 | 2 })}>
            <option value={2}>Expense</option>
            <option value={1}>Income</option>
          </select>
          <input type="datetime-local" value={form.date} onChange={(e) => setForm({ ...form, date: e.target.value })} />
        </div>
        <div className="form-row">
          <select value={form.accountId ?? ''} onChange={(e) => setForm({ ...form, accountId: e.target.value ? parseInt(e.target.value) : undefined })}>
            <option value="">No Account</option>
            {accounts.map((a) => (<option key={a.id} value={a.id}>{a.name}</option>))}
          </select>
          <select value={form.categoryId ?? ''} onChange={(e) => setForm({ ...form, categoryId: e.target.value ? parseInt(e.target.value) : undefined })}>
            <option value="">No Category</option>
            {categories.map((c) => (<option key={c.id} value={c.id}>{c.name}</option>))}
          </select>
          <input value={form.note} onChange={(e) => setForm({ ...form, note: e.target.value })} placeholder="Note (optional)" />
          <button onClick={create}>Add</button>
        </div>
      </div>

      <div className="transactions-table">
        <table>
          <thead>
            <tr><th>Date</th><th>Account</th><th>Category</th><th>Type</th><th>Amount</th><th>Note</th><th>Actions</th></tr>
          </thead>
          <tbody>
            {items.map(t => (
              <tr key={t.id}>
                <td>{dayjs(t.date).format('YYYY-MM-DD')}</td>
                <td>{t.accountName ?? '-'}</td>
                <td>{t.categoryName ?? '-'}</td>
                <td className={t.type === 1 ? 'income' : 'expense'}>{t.type === 1 ? 'Income' : 'Expense'}</td>
                <td className={t.type === 1 ? 'income' : 'expense'}>${t.amount.toFixed(2)}</td>
                <td>{t.note ?? '-'}</td>
                <td><button className="delete-btn" onClick={async () => { await transactionsApi.delete(t.id); await load(); }}>Delete</button></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
} 
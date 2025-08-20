import { useEffect, useState } from 'react';
import { tagsApi } from '../api';

export default function Tags() {
  const [items, setItems] = useState<any[]>([]);
  const [name, setName] = useState('');

  const load = async () => {
    const list = await tagsApi.list();
    setItems(list);
  };

  useEffect(() => { load(); }, []);

  const create = async () => {
    if (!name.trim()) return;
    await tagsApi.create({ name });
    setName('');
    await load();
  };

  return (
    <div className="section">
      <h2>Tags</h2>
      <div className="input-group">
        <input value={name} onChange={(e) => setName(e.target.value)} placeholder="Tag name" onKeyDown={(e) => e.key === 'Enter' && create()} />
        <button onClick={create}>Add</button>
      </div>
      <div className="categories-list">
        {items.map(t => (
          <div key={t.id} className="category-item"><span>{t.name}</span></div>
        ))}
      </div>
    </div>
  );
} 
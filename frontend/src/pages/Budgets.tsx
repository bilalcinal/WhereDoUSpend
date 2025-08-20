import { useEffect, useMemo, useState } from 'react';
import dayjs from 'dayjs';
import { budgetsApi, categoriesApi, reportsApi, type Budget } from '../api';
import { toast } from 'sonner';

type BudgetRow = {
  id?: number;
  categoryId: number;
  category: string;
  amount: number;
  actual: number;
};

export default function Budgets() {
  const [cursor, setCursor] = useState(dayjs().startOf('month'));
  const [rows, setRows] = useState<BudgetRow[]>([]);
  const [categories, setCategories] = useState<Array<{ id: number; name: string }>>([]);
  const [loading, setLoading] = useState(false);
  const [creating, setCreating] = useState<{ categoryId: number | ''; amount: number }>({ categoryId: '', amount: 0 });

  const ym = useMemo(() => ({ year: cursor.year(), month: cursor.month() + 1 }), [cursor]);

  const load = async () => {
    setLoading(true);
    try {
      const [list, bva, cats] = await Promise.all([
        budgetsApi.list(ym.year, ym.month),
        reportsApi.budgetVsActual(ym.year, ym.month),
        categoriesApi.list(),
      ]);

      setCategories(cats.map((c: any) => ({ id: c.id, name: c.name })));

      const byCat = new Map<number, Budget>();
      list.forEach((b) => byCat.set(b.categoryId, b));

      const mapped: BudgetRow[] = (bva as any[]).map((x) => {
        const b = [...byCat.values()].find((bb) => categories.find((c) => c.id === bb.categoryId)?.name === x.category);
        // Prefer match by name, fallback by reverse map if possible
        const fromCat = categories.find((c) => c.name === x.category);
        const id = b?.id ?? (fromCat ? byCat.get(fromCat.id)?.id : undefined);
        const categoryId = b?.categoryId ?? fromCat?.id ?? 0;
        return { id, categoryId, category: x.category, amount: x.budget ?? 0, actual: x.actual ?? 0 } as BudgetRow;
      });

      // Also include budgets that have no actuals yet (not returned by bva)
      list.forEach((b) => {
        if (!mapped.some((m) => m.categoryId === b.categoryId)) {
          const name = cats.find((c: any) => c.id === b.categoryId)?.name ?? 'Unknown';
          mapped.push({ id: b.id, categoryId: b.categoryId, category: name, amount: b.amount, actual: 0 });
        }
      });

      mapped.sort((a, b) => a.category.localeCompare(b.category));
      setRows(mapped);
    } catch (e) {
      toast.error('Failed to load budgets');
      setRows([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); /* eslint-disable-next-line react-hooks/exhaustive-deps */ }, [ym.year, ym.month]);

  const pct = (actual: number, budget: number) => {
    if (budget <= 0) return 0;
    return Math.min(100, Math.max(0, Math.round((Math.abs(actual) / budget) * 100)));
  };

  const onCreate = async () => {
    if (!creating.categoryId || creating.amount <= 0) return;
    const cat = categories.find((c) => c.id === creating.categoryId);
    const optimistic: BudgetRow = { categoryId: creating.categoryId as number, category: cat?.name ?? 'Unknown', amount: creating.amount, actual: 0 };
    setRows((r) => [...r, optimistic]);
    setCreating({ categoryId: '', amount: 0 });
    try {
      const saved = await budgetsApi.create({ categoryId: optimistic.categoryId, year: ym.year, month: ym.month, amount: optimistic.amount });
      setRows((r) => r.map((x) => (x === optimistic ? { ...x, id: saved.id } : x)));
      toast.success('Budget created');
    } catch (e) {
      setRows((r) => r.filter((x) => x !== optimistic));
      toast.error('Create failed');
    }
  };

  const onUpdate = async (row: BudgetRow, newAmount: number) => {
    if (!row.id) return;
    const prev = row.amount;
    setRows((r) => r.map((x) => (x.id === row.id ? { ...x, amount: newAmount } : x)));
    try {
      await budgetsApi.update(row.id, { amount: newAmount });
    } catch (e) {
      setRows((r) => r.map((x) => (x.id === row.id ? { ...x, amount: prev } : x)));
      toast.error('Update failed');
    }
  };

  const onDelete = async (row: BudgetRow) => {
    if (!row.id) return;
    const prev = rows;
    setRows((r) => r.filter((x) => x.id !== row.id));
    try {
      await budgetsApi.delete(row.id);
      toast.success('Deleted');
    } catch (e) {
      setRows(prev);
      toast.error('Delete failed');
    }
  };

  const rollover = async () => {
    // Copy from previous month if current is empty
    const prevMonth = cursor.subtract(1, 'month');
    try {
      const prev = await budgetsApi.list(prevMonth.year(), prevMonth.month() + 1);
      if (!prev.length) {
        toast.info('No previous budgets to copy');
        return;
      }
      // optimistic add
      const optimisticRows: BudgetRow[] = prev.map((b) => {
        const name = categories.find((c) => c.id === b.categoryId)?.name ?? 'Unknown';
        return { categoryId: b.categoryId, category: name, amount: b.amount, actual: 0 };
      });
      setRows((r) => [...r, ...optimisticRows]);
      await Promise.all(prev.map((b) => budgetsApi.create({ categoryId: b.categoryId, year: ym.year, month: ym.month, amount: b.amount })));
      toast.success('Rollover completed');
      await load();
    } catch (e) {
      toast.error('Rollover failed');
      await load();
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <button className="px-3 py-2 rounded-lg bg-gray-100 hover:bg-gray-200" onClick={() => setCursor(cursor.subtract(1, 'month'))}>{'<'}</button>
          <div className="font-semibold text-lg">{cursor.format('MMMM YYYY')}</div>
          <button className="px-3 py-2 rounded-lg bg-gray-100 hover:bg-gray-200" onClick={() => setCursor(cursor.add(1, 'month'))}>{'>'}</button>
        </div>
        <div className="flex items-center gap-2">
          <button className="px-3 py-2 rounded-lg bg-blue-600 text-white hover:bg-blue-700" onClick={rollover}>Copy from previous</button>
        </div>
      </div>

      <div className="rounded-2xl bg-white p-4 shadow-sm">
        <div className="grid grid-cols-12 gap-2 font-medium text-sm text-gray-600 border-b pb-2">
          <div className="col-span-5">Category</div>
          <div className="col-span-2 text-right">Budget</div>
          <div className="col-span-2 text-right">Actual</div>
          <div className="col-span-2">Progress</div>
          <div className="col-span-1"></div>
        </div>
        <div className="divide-y">
          {loading && <div className="py-6 text-sm text-gray-500">Loading...</div>}
          {!loading && rows.length === 0 && <div className="py-6 text-sm text-gray-500">No budgets yet. Create one below.</div>}
          {rows.map((r) => (
            <div key={(r.id ?? 'new') + '-' + r.categoryId} className="grid grid-cols-12 gap-2 items-center py-2">
              <div className="col-span-5">{r.category}</div>
              <div className="col-span-2 text-right">
                <input
                  type="number"
                  className="w-28 text-right px-2 py-1 border rounded-md"
                  value={r.amount}
                  onChange={(e) => onUpdate(r, parseFloat(e.target.value) || 0)}
                />
              </div>
              <div className="col-span-2 text-right">{Math.abs(r.actual).toFixed(2)}</div>
              <div className="col-span-2">
                <div className="h-2 bg-gray-200 rounded-full overflow-hidden">
                  <div className={`h-full ${pct(r.actual, r.amount) < 80 ? 'bg-green-500' : pct(r.actual, r.amount) < 100 ? 'bg-yellow-500' : 'bg-red-500'}`} style={{ width: pct(r.actual, r.amount) + '%' }} />
                </div>
                <div className="text-xs text-gray-500 mt-1">{pct(r.actual, r.amount)}%</div>
              </div>
              <div className="col-span-1 text-right">
                <button className="text-red-600 hover:underline" onClick={() => onDelete(r)}>Delete</button>
              </div>
            </div>
          ))}
        </div>
      </div>

      <div className="rounded-2xl bg-white p-4 shadow-sm">
        <div className="font-medium mb-3">Create budget</div>
        <div className="flex flex-wrap items-center gap-2">
          <select
            className="min-w-48 px-3 py-2 border rounded-md"
            value={creating.categoryId}
            onChange={(e) => setCreating((f) => ({ ...f, categoryId: e.target.value ? parseInt(e.target.value) : '' }))}
          >
            <option value="">Select category</option>
            {categories.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>
          <input
            type="number"
            className="w-40 px-3 py-2 border rounded-md"
            placeholder="Amount"
            value={creating.amount}
            onChange={(e) => setCreating((f) => ({ ...f, amount: parseFloat(e.target.value) || 0 }))}
          />
          <button className="px-4 py-2 rounded-md bg-green-600 text-white hover:bg-green-700" onClick={onCreate}>
            Add
          </button>
        </div>
      </div>
    </div>
  );
}


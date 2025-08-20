import { useState, useEffect } from 'react';
import { PieChart, Pie, Cell, ResponsiveContainer, Legend, Tooltip } from 'recharts';
import dayjs from 'dayjs';
import { categoriesApi, transactionsApi, reportsApi } from './api';
import type { Category, Transaction, SummaryReport, UpsertTransactionDto } from './api';
import './App.css';

const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884D8', '#82CA9D'];

function App() {
	const [categories, setCategories] = useState<Category[]>([]);
	const [transactions, setTransactions] = useState<Transaction[]>([]);
	const [summary, setSummary] = useState<SummaryReport[]>([]);
	const [newCategoryName, setNewCategoryName] = useState('');
	const [newTransaction, setNewTransaction] = useState<UpsertTransactionDto>({
		amount: 0,
		type: 2,
		date: dayjs().format('YYYY-MM-DDTHH:mm'),
		note: '',
		categoryId: undefined
	});
	const [loading, setLoading] = useState(false);

	const currentYear = dayjs().year();
	const currentMonth = dayjs().month() + 1;

	useEffect(() => {
		loadData();
	}, []);

	const loadData = async () => {
		setLoading(true);
		try {
			const [categoriesData, transactionsData, summaryData] = await Promise.all([
				categoriesApi.list(),
				transactionsApi.list(),
				reportsApi.monthlySummary(currentYear, currentMonth)
			]);
			
			setCategories(categoriesData);
			setTransactions(transactionsData);
			setSummary(summaryData);
		} catch (error) {
			console.error('Error loading data:', error);
		} finally {
			setLoading(false);
		}
	};

	const handleCreateCategory = async () => {
		if (!newCategoryName.trim()) return;
		
		try {
			const category = await categoriesApi.create({ name: newCategoryName.trim() });
			setCategories(prev => [...prev, category]);
			setNewCategoryName('');
			await loadData(); // Refresh summary
		} catch (error) {
			console.error('Error creating category:', error);
		}
	};

	const handleDeleteCategory = async (id: number) => {
		try {
			await categoriesApi.delete(id);
			setCategories(prev => prev.filter(c => c.id !== id));
			await loadData(); // Refresh data
		} catch (error) {
			console.error('Error deleting category:', error);
		}
	};

	const handleCreateTransaction = async () => {
		if (newTransaction.amount <= 0) return;
		
		try {
			await transactionsApi.create(newTransaction);
			setNewTransaction({
				amount: 0,
				type: 2,
				date: dayjs().format('YYYY-MM-DDTHH:mm'),
				note: '',
				categoryId: undefined
			});
			await loadData(); // Refresh data
		} catch (error) {
			console.error('Error creating transaction:', error);
		}
	};

	const handleDeleteTransaction = async (id: number) => {
		try {
			await transactionsApi.delete(id);
			await loadData(); // Refresh data
		} catch (error) {
			console.error('Error deleting transaction:', error);
		}
	};

	// Prepare chart data for expenses only
	const chartData = summary
		.filter(item => item.type === 2) // Only expenses
		.map(item => ({
			name: item.category,
			value: item.total
		}));

	if (loading) {
		return <div className="app">Loading...</div>;
	}

	return (
		<div className="app">
			<h1>Personal Finance Tracker</h1>
			
			{/* Categories Section */}
			<section className="section">
				<h2>Categories</h2>
				<div className="input-group">
					<input
						type="text"
						value={newCategoryName}
						onChange={(e) => setNewCategoryName(e.target.value)}
						placeholder="Category name"
						onKeyPress={(e) => e.key === 'Enter' && handleCreateCategory()}
					/>
					<button onClick={handleCreateCategory}>Add Category</button>
				</div>
				<div className="categories-list">
					{categories.map(category => (
						<div key={category.id} className="category-item">
							<span>{category.name}</span>
							<button 
								onClick={() => handleDeleteCategory(category.id)}
								className="delete-btn"
							>
								Delete
							</button>
						</div>
					))}
				</div>
			</section>

			{/* New Transaction Section */}
			<section className="section">
				<h2>New Transaction</h2>
				<div className="transaction-form">
					<div className="form-row">
						<input
							type="number"
							step="0.01"
							value={newTransaction.amount}
							onChange={(e) => setNewTransaction(prev => ({ ...prev, amount: parseFloat(e.target.value) || 0 }))}
							placeholder="Amount"
						/>
						<select
							value={newTransaction.type}
							onChange={(e) => setNewTransaction(prev => ({ ...prev, type: parseInt(e.target.value) as 1 | 2 }))}
						>
							<option value={2}>Expense</option>
							<option value={1}>Income</option>
						</select>
					</div>
					<div className="form-row">
						<input
							type="datetime-local"
							value={newTransaction.date}
							onChange={(e) => setNewTransaction(prev => ({ ...prev, date: e.target.value }))}
						/>
						<select
							value={newTransaction.categoryId || ''}
							onChange={(e) => setNewTransaction(prev => ({ ...prev, categoryId: e.target.value ? parseInt(e.target.value) : undefined }))}
						>
							<option value="">No Category</option>
							{categories.map(cat => (
								<option key={cat.id} value={cat.id}>{cat.name}</option>
							))}
						</select>
					</div>
					<div className="form-row">
						<input
							type="text"
							value={newTransaction.note}
							onChange={(e) => setNewTransaction(prev => ({ ...prev, note: e.target.value }))}
							placeholder="Note (optional)"
						/>
						<button onClick={handleCreateTransaction}>Save Transaction</button>
					</div>
				</div>
			</section>

			{/* Transactions Table */}
			<section className="section">
				<h2>Transactions</h2>
				<div className="transactions-table">
					<table>
						<thead>
							<tr>
								<th>Date</th>
								<th>Category</th>
								<th>Type</th>
								<th>Amount</th>
								<th>Note</th>
								<th>Actions</th>
							</tr>
						</thead>
						<tbody>
							{transactions.map(transaction => (
								<tr key={transaction.id}>
									<td>{dayjs(transaction.date).format('MMM DD, YYYY')}</td>
									<td>{transaction.categoryName || 'Uncategorized'}</td>
									<td className={transaction.type === 1 ? 'income' : 'expense'}>
										{transaction.type === 1 ? 'Income' : 'Expense'}
									</td>
									<td className={transaction.type === 1 ? 'income' : 'expense'}>
										${transaction.amount.toFixed(2)}
									</td>
									<td>{transaction.note || '-'}</td>
									<td>
										<button 
											onClick={() => handleDeleteTransaction(transaction.id)}
											className="delete-btn"
										>
											Delete
										</button>
									</td>
								</tr>
							))}
						</tbody>
					</table>
				</div>
			</section>

			{/* Dashboard Chart */}
			<section className="section">
				<h2>Dashboard - {dayjs().format('MMMM YYYY')}</h2>
				<div className="chart-container">
					<ResponsiveContainer width="100%" height={300}>
						<PieChart>
							<Pie
								data={chartData}
								cx="50%"
								cy="50%"
								labelLine={false}
								label={({ name, percent = 0 }) => `${name} ${(percent * 100).toFixed(0)}%`}
								outerRadius={80}
								fill="#8884d8"
								dataKey="value"
							>
								{chartData.map((entry, index) => (
									<Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
								))}
							</Pie>
							<Tooltip formatter={(value) => [`$${Number(value).toFixed(2)}`, 'Amount']} />
							<Legend />
						</PieChart>
					</ResponsiveContainer>
				</div>
				
				{/* Summary Stats */}
				<div className="summary-stats">
					<div className="stat">
						<h3>Total Income</h3>
						<p className="income">
							${summary.filter(s => s.type === 1).reduce((sum, s) => sum + s.total, 0).toFixed(2)}
						</p>
					</div>
					<div className="stat">
						<h3>Total Expenses</h3>
						<p className="expense">
							${summary.filter(s => s.type === 2).reduce((sum, s) => sum + s.total, 0).toFixed(2)}
						</p>
					</div>
					<div className="stat">
						<h3>Net</h3>
						<p className={summary.reduce((sum, s) => sum + (s.type === 1 ? s.total : -s.total), 0) >= 0 ? 'income' : 'expense'}>
							${summary.reduce((sum, s) => sum + (s.type === 1 ? s.total : -s.total), 0).toFixed(2)}
						</p>
					</div>
				</div>
			</section>
		</div>
	);
}

export default App;

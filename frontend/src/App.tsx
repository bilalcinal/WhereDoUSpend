import { BrowserRouter, Routes, Route, Link, Navigate } from 'react-router-dom';
import './App.css';
import { useState } from 'react';
//
import Dashboard from './pages/Dashboard';
import Accounts from './pages/Accounts';
import Transactions from './pages/Transactions';
import Budgets from './pages/Budgets';
import Recurring from './pages/Recurring';
import Tags from './pages/Tags';
import Login from './pages/Login';

function App() {
  const [isAuthed, setIsAuthed] = useState(false);

  return (
    <BrowserRouter>
      <div className="app">
        <nav className="nav">
          <Link to="/">Dashboard</Link>
          <Link to="/accounts">Accounts</Link>
          <Link to="/transactions">Transactions</Link>
          <Link to="/budgets">Budgets</Link>
          <Link to="/recurring">Recurring</Link>
          <Link to="/tags">Tags</Link>
          <Link to="/login" className="right">Login</Link>
        </nav>
        <Routes>
          <Route path="/login" element={<Login onLoggedIn={() => setIsAuthed(true)} />} />
          <Route path="/" element={isAuthed ? <Dashboard /> : <Navigate to="/login" replace />} />
          <Route path="/accounts" element={isAuthed ? <Accounts /> : <Navigate to="/login" replace />} />
          <Route path="/transactions" element={isAuthed ? <Transactions /> : <Navigate to="/login" replace />} />
          <Route path="/budgets" element={isAuthed ? <Budgets /> : <Navigate to="/login" replace />} />
          <Route path="/recurring" element={isAuthed ? <Recurring /> : <Navigate to="/login" replace />} />
          <Route path="/tags" element={isAuthed ? <Tags /> : <Navigate to="/login" replace />} />
        </Routes>
      </div>
    </BrowserRouter>
  );
}

export default App;

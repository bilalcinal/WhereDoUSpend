import { BrowserRouter, Routes, Route, Link, Navigate, useLocation } from 'react-router-dom';
import './App.css';
import { useEffect, useState } from 'react';
import Dashboard from './pages/Dashboard';
import Accounts from './pages/Accounts';
import Transactions from './pages/Transactions';
import Budgets from './pages/Budgets';
import Recurring from './pages/Recurring';
import Tags from './pages/Tags';
import Login from './pages/Login';
import { useAuth } from './store/auth';

function Shell({ children }: { children: React.ReactNode }) {
  return (
    <div className="app">
      <nav className="nav">
        <Link to="/dashboard">Dashboard</Link>
        <Link to="/accounts">Accounts</Link>
        <Link to="/transactions">Transactions</Link>
        <Link to="/budgets">Budgets</Link>
        <Link to="/recurring">Recurring</Link>
        <Link to="/tags">Categories</Link>
        <Link to="/login" className="right">Login</Link>
      </nav>
      {children}
    </div>
  );
}

function AppRoutes() {
  const [isAuthed, setIsAuthed] = useState(false);
  const auth = useAuth();
  const location = useLocation();

  useEffect(() => {
    auth.hydrate();
    if (auth.accessToken) setIsAuthed(true);
  }, []);

  useEffect(() => {
    if (location.pathname !== '/login') auth.setLastRoute(location.pathname);
  }, [location.pathname]);

  return (
    <Shell>
      <Routes>
        <Route path="/login" element={isAuthed ? <Navigate to={auth.lastRoute ?? '/dashboard'} replace /> : <Login onLoggedIn={() => setIsAuthed(true)} />} />
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        <Route path="/dashboard" element={isAuthed ? <Dashboard /> : <Navigate to="/login" replace />} />
        <Route path="/accounts" element={isAuthed ? <Accounts /> : <Navigate to="/login" replace />} />
        <Route path="/transactions" element={isAuthed ? <Transactions /> : <Navigate to="/login" replace />} />
        <Route path="/budgets" element={isAuthed ? <Budgets /> : <Navigate to="/login" replace />} />
        <Route path="/recurring" element={isAuthed ? <Recurring /> : <Navigate to="/login" replace />} />
        <Route path="/tags" element={isAuthed ? <Tags /> : <Navigate to="/login" replace />} />
      </Routes>
    </Shell>
  );
}

function App() {
  return (
    <BrowserRouter>
      <AppRoutes />
    </BrowserRouter>
  );
}

export default App;

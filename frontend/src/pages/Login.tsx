import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { authApi } from '../api';
import { useAuth } from '../store/auth';

export default function Login({ onLoggedIn }: { onLoggedIn: () => void }) {
  const navigate = useNavigate();
  const auth = useAuth();
  const [email, setEmail] = useState('demo@local');
  const [password, setPassword] = useState('Pass123$');
  const [error, setError] = useState<string | null>(null);

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    try {
      const t = await authApi.login(email, password);
      auth.setTokens({ accessToken: t.accessToken, refreshToken: t.refreshToken, email });
      onLoggedIn();
      navigate(auth.lastRoute ?? '/dashboard');
    } catch {
      setError('Login failed');
    }
  };

  return (
    <div className="section">
      <h2>Login</h2>
      <form onSubmit={submit} className="transaction-form">
        <div className="form-row">
          <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} placeholder="Email" />
          <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} placeholder="Password" />
        </div>
        <div className="form-row">
          <button type="submit">Login</button>
        </div>
        {error && <p className="expense">{error}</p>}
      </form>
    </div>
  );
} 
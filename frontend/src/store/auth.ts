import { create } from 'zustand';

type AuthState = {
    accessToken: string | null;
    refreshToken: string | null;
    userEmail: string | null;
    lastRoute: string | null;
    setTokens: (t: { accessToken: string; refreshToken: string; email?: string | null }) => void;
    clear: () => void;
    setLastRoute: (path: string | null) => void;
    hydrate: () => void;
};

export const useAuth = create<AuthState>((set) => ({
    accessToken: null,
    refreshToken: null,
    userEmail: null,
    lastRoute: null,
    setTokens: (t) => {
        set({ accessToken: t.accessToken, refreshToken: t.refreshToken, userEmail: t.email ?? null });
        localStorage.setItem('auth', JSON.stringify({ accessToken: t.accessToken, refreshToken: t.refreshToken, email: t.email ?? null }));
    },
    clear: () => {
        set({ accessToken: null, refreshToken: null, userEmail: null });
        localStorage.removeItem('auth');
    },
    setLastRoute: (path) => set({ lastRoute: path }),
    hydrate: () => {
        const raw = localStorage.getItem('auth');
        if (!raw) return;
        try {
            const data = JSON.parse(raw) as { accessToken: string; refreshToken: string; email?: string | null };
            set({ accessToken: data.accessToken, refreshToken: data.refreshToken, userEmail: data.email ?? null });
        } catch {
            /* noop */
        }
    },
}));



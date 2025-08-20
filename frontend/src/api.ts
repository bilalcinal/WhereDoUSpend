import axios from 'axios';
import { useAuth } from './store/auth';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5002/api';
const ORIGIN_URL = API_URL.replace(/\/api\/?$/, '');

const api = axios.create({
    baseURL: API_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

const authClient = axios.create({
    baseURL: ORIGIN_URL,
    headers: { 'Content-Type': 'application/json' },
});

// helpers
type Paged<T> = { items: T[]; total?: number; page?: number; pageSize?: number };
function asItems<T>(d: unknown): T[] {
    const maybe = d as Partial<Paged<T>>;
    return Array.isArray((maybe as { items?: unknown }).items) ? (maybe.items as T[]) : (d as T[]);
}

// Auth token handling
let accessToken: string | null = null;
let refreshToken: string | null = null;

export function setTokens(tokens: { accessToken: string; refreshToken: string }) {
    accessToken = tokens.accessToken;
    refreshToken = tokens.refreshToken;
    api.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;
    // also mirror to zustand store
    try {
        useAuth.getState().setTokens({ accessToken, refreshToken });
    } catch { }
}

// attach access token from store before requests
api.interceptors.request.use((config) => {
    if (!accessToken) {
        try {
            const { accessToken: at } = useAuth.getState();
            if (at) {
                accessToken = at;
                config.headers = config.headers ?? {};
                (config.headers as any)['Authorization'] = `Bearer ${at}`;
            }
        } catch { }
    }
    return config;
});

api.interceptors.response.use(
    (res) => res,
    async (error) => {
        if (error.response?.status === 401 && refreshToken) {
            try {
                const res = await authClient.post('/auth/refresh', { refreshToken });
                setTokens(res.data);
                error.config.headers['Authorization'] = `Bearer ${accessToken}`;
                return api.request(error.config);
            } catch {
                // reset tokens and fall through
                accessToken = null;
                refreshToken = null;
                delete api.defaults.headers.common['Authorization'];
            }
        }
        return Promise.reject(error);
    }
);

// Types
export interface Category { id: number; name: string }
export interface Account { id: number; name: string; type: number; currency: string; openingBalance: number; isArchived: boolean }
export interface Tag { id: number; name: string }
export interface Budget { id: number; categoryId: number; year: number; month: number; amount: number }
export interface Transaction {
    id: number;
    amount: number;
    type: 1 | 2;
    date: string;
    note?: string;
    accountId?: number;
    categoryId?: number;
    categoryName?: string;
    accountName?: string;
    tags: string[];
}

// Auth
export const authApi = {
    register: (email: string, password: string) => authClient.post('/auth/register', { email, password }),
    login: async (email: string, password: string) => {
        const res = await authClient.post('/auth/login', { email, password });
        setTokens(res.data);
        return res.data as { accessToken: string; refreshToken: string; expiresAt: string };
    },
    logout: () => authClient.post('/auth/logout', { refreshToken }),
};

// Categories
export const categoriesApi = {
    list: () => api.get<Paged<Category> | Category[]>('/categories').then(res => asItems<Category>(res.data)),
    create: (data: { name: string }) => api.post<Category>('/categories', data).then(res => res.data),
    update: (id: number, data: { name: string }) => api.put<Category>(`/categories/${id}`, data).then(res => res.data),
    delete: (id: number) => api.delete(`/categories/${id}`),
};

// Accounts
export const accountsApi = {
    list: () => api.get<Paged<Account> | Account[]>('/accounts').then(res => asItems<Account>(res.data)),
    create: (data: { name: string; type: number; currency: string; openingBalance: number }) => api.post<Account>('/accounts', data).then(res => res.data),
    archive: (id: number) => api.patch(`/accounts/${id}/archive`, {}),
    delete: (id: number) => api.delete(`/accounts/${id}`),
};

// Tags
export const tagsApi = {
    list: () => api.get<Paged<Tag> | Tag[]>('/tags').then(res => asItems<Tag>(res.data)),
    create: (data: { name: string }) => api.post<Tag>('/tags', data).then(res => res.data),
};

// Transactions
type TransactionList = { items: Transaction[]; total: number; page: number; pageSize: number };
export const transactionsApi = {
    list: (params?: { from?: string; to?: string; accountId?: number; categoryId?: number; sort?: string; page?: number; pageSize?: number }) =>
        api.get<TransactionList>('/transactions', { params }).then(res => res.data),
    create: (data: { amount: number; type: 1 | 2; date: string; note?: string; accountId?: number; categoryId?: number; tagIds?: number[] }) =>
        api.post<Transaction>('/transactions', data).then(res => res.data),
    update: (id: number, data: Partial<{ amount: number; type: 1 | 2; date: string; note?: string; accountId?: number; categoryId?: number; tagIds?: number[] }>) =>
        api.put<Transaction>(`/transactions/${id}`, data).then(res => res.data),
    delete: (id: number) => api.delete(`/transactions/${id}`),
};

// Budgets
export const budgetsApi = {
    list: (year: number, month: number) => api.get<Budget[]>(`/budgets`, { params: { year, month } }).then(res => res.data),
    create: (data: { categoryId: number; year: number; month: number; amount: number }) => api.post<Budget>('/budgets', data).then(res => res.data),
    update: (id: number, data: { amount: number }) => api.put<Budget>(`/budgets/${id}`, data).then(res => res.data),
    delete: (id: number) => api.delete(`/budgets/${id}`),
};

// Recurring
export const recurringApi = {
    list: () => api.get('/recurring').then(res => res.data as Array<unknown>),
    create: (data: { accountId?: number; categoryId: number; amount: number; type: 1 | 2; cadence: number; nextRunAt: string; note?: string }) => api.post('/recurring', data).then(res => res.data),
    runDue: () => api.post('/recurring/run-due', {}).then(res => res.data),
    delete: (id: number) => api.delete(`/recurring/${id}`),
};

// Reports
export const reportsApi = {
    summary: (year: number, month: number) => api.get('/reports/summary', { params: { year, month } }).then(res => res.data as Array<{ category: string; type: number; total: number }>),
    cashflow: (from: string, to: string, groupBy: 'day' | 'month') => api.get('/reports/cashflow', { params: { from, to, groupBy } }).then(res => res.data as Array<{ period: string; net: number }>),
    byAccount: (from: string, to: string) => api.get('/reports/by-account', { params: { from, to } }).then(res => res.data as Array<{ account: string; total: number }>),
    budgetVsActual: (year: number, month: number) => api.get('/reports/budget-vs-actual', { params: { year, month } }).then(res => res.data as Array<{ category: string; budget: number; actual: number }>),
};

export default api; 
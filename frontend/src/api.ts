import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5002/api';

const api = axios.create({
    baseURL: API_URL,
    headers: {
        'x-user-id': 'demo',
        'Content-Type': 'application/json',
    },
});

export interface Category {
    id: number;
    name: string;
}

export interface Transaction {
    id: number;
    amount: number;
    type: 1 | 2; // 1 = Income, 2 = Expense
    date: string;
    note?: string;
    categoryId?: number;
    categoryName?: string;
}

export interface SummaryReport {
    category: string;
    type: 1 | 2;
    total: number;
}

export interface UpsertCategoryDto {
    name: string;
}

export interface UpsertTransactionDto {
    amount: number;
    type: 1 | 2;
    date: string;
    note?: string;
    categoryId?: number;
}

// Categories API
export const categoriesApi = {
    list: () => api.get<Category[]>('/categories').then(res => res.data),
    create: (data: UpsertCategoryDto) => api.post<Category>('/categories', data).then(res => res.data),
    delete: (id: number) => api.delete(`/categories/${id}`),
};

// Transactions API
export const transactionsApi = {
    list: (from?: string, to?: string) => {
        const params = new URLSearchParams();
        if (from) params.append('from', from);
        if (to) params.append('to', to);
        return api.get<Transaction[]>(`/transactions?${params}`).then(res => res.data);
    },
    create: (data: UpsertTransactionDto) => api.post<Transaction>('/transactions', data).then(res => res.data),
    delete: (id: number) => api.delete(`/transactions/${id}`),
};

// Reports API
export const reportsApi = {
    monthlySummary: (year: number, month: number) =>
        api.get<SummaryReport[]>(`/reports/summary?year=${year}&month=${month}`).then(res => res.data),
};

export default api; 
import axios from 'axios';
import { toast } from 'sonner';
import { useAuthStore } from '../store/useAuthStore';

const API_BASE_URL = (() => {
  const envUrl = import.meta.env.VITE_API_URL;
  if (envUrl) return `${envUrl.replace(/\/$/, '')}/api/v1`;
  if (typeof window !== 'undefined') {
    const proto = window.location.protocol;
    const host = window.location.hostname;
    const port = window.location.port;
    if (port && port !== '80' && port !== '443') {
      return `${proto}//${host}:5000/api/v1`;
    }
    return `${proto}//${host}/api/v1`;
  }
  return '/api/v1';
})();

export const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

let isRefreshing = false;
let failedQueue: Array<{
  resolve: (token: string) => void;
  reject: (error: unknown) => void;
}> = [];

function processQueue(error: unknown, token: string | null = null) {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token!);
    }
  });
  failedQueue = [];
}

api.interceptors.request.use(
  (config) => {
    const token = useAuthStore.getState().token;
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

function getSuccessMessage(method: string, url: string): string | null {
  if (url.includes('/auth/')) return null;
  const resource = url.split('/').pop()?.split('?')[0] ?? '';
  const map: Record<string, Record<string, string>> = {
    post: { default: `${resource} criado com sucesso` },
    put: { default: `${resource} atualizado com sucesso` },
    patch: { default: `${resource} atualizado com sucesso` },
    delete: { default: `${resource} removido com sucesso` },
  };
  return map[method]?.default ?? 'Operação realizada com sucesso';
}

api.interceptors.response.use(
  (response) => {
    const method = (response.config.method ?? '').toLowerCase();
    if (method !== 'get' && response.status >= 200 && response.status < 300) {
      const msg = getSuccessMessage(method, response.config.url ?? '');
      if (msg) toast.success(msg);
    }
    return response;
  },
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status !== 401 || originalRequest._retry) {
      if (error.response?.status !== 401) {
        const msg = error.response?.data?.title || error.response?.data?.description || error.message || 'Erro inesperado';
        toast.error(msg);
      }
      return Promise.reject(error);
    }

    if (isRefreshing) {
      return new Promise((resolve, reject) => {
        failedQueue.push({ resolve, reject });
      }).then((token) => {
        originalRequest.headers.Authorization = `Bearer ${token}`;
        return api(originalRequest);
      });
    }

    originalRequest._retry = true;
    isRefreshing = true;

    try {
      const refreshToken = useAuthStore.getState().refreshToken;
      if (!refreshToken) throw new Error('No refresh token');

      const res = await axios.post(`${API_BASE_URL}/auth/refresh`, { token: refreshToken });
      const { accessToken: newToken, refreshToken: newRefreshToken } = res.data;

      useAuthStore.getState().setTokens(newToken, newRefreshToken);
      processQueue(null, newToken);

      originalRequest.headers.Authorization = `Bearer ${newToken}`;
      return api(originalRequest);
    } catch (refreshError) {
      processQueue(refreshError, null);
      useAuthStore.getState().logout();
      window.location.href = '/login';
      return Promise.reject(refreshError);
    } finally {
      isRefreshing = false;
    }
  }
);

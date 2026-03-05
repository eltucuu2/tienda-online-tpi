import axios from 'axios';

const instance = axios.create({
  baseURL: import.meta.env.VITE_BACKEND_URL,
  withCredentials: true, 
  headers: {
    'Content-Type': 'application/json',
  },
});

instance.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');

    if (token) {
      // El backend .NET espera "Bearer <token>"
      config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
  },
  (error) => Promise.reject(error),
);

instance.interceptors.response.use(
  (config) => { return config; },
  (error) => {
    // El status suele venir en error.response
    if (error.response && error.response.status === 401) {
      if (window.location.pathname.includes('/admin/')) {
        localStorage.clear();
        window.location.href = '/login';
      } else {
        localStorage.removeItem('token');
      }
    }

    return Promise.reject(error);
  },
);

// Export nombrado (significa que al importar debes usar llaves { instance })
export { instance };
import { instance } from '../../shared/api/axiosInstance';

export const login = async (username, password) => {
  
  const response = await instance.post('/auth/login', { username, password });

  // Retornamos la respuesta completa para que el AuthProvider pueda leer el token
  return { data: response.data, error: null };
};
import { instance } from '../../shared/api/axiosInstance';

export const getPublicProducts = async () => {
  try {
    // GET /api/products (Endpoint público)
    const response = await instance.get('/products');
    return { data: response.data, error: null };
  } catch (error) {
    return { data: [], error: error };
  }
};
import { instance } from '../../shared/api/axiosInstance';

// Endpoint 4: Actualizar Producto
export const updateProduct = async (id, productData) => {
  try {
    const response = await instance.put(`/products/${id}`, productData);
    return { data: response.data, error: null };
  } catch (error) {
    return { data: null, error: error };
  }
};
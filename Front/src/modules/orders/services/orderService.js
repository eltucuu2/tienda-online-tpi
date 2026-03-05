import { instance } from '../../shared/api/axiosInstance';

// 1. Traer todas las órdenes (Para el Listado)
export const getOrders = async (search = null, status = null, pageNumber = 1, pageSize = 10) => {
  const queryString = new URLSearchParams({
    pageNumber,
    pageSize,
    ...(status && status !== 'all' && { status }),
    ...(search && { search }),
  });

  try {
    const response = await instance.get(`/orders?${queryString}`);
    return { data: response.data, error: null };
  } catch (error) {
    return { data: null, error: error };
  }
};

// 2. Traer una orden por ID (Para el Formulario de Edición) <--- ESTA TE FALTABA
export const getOrderById = async (id) => {
  try {
    const response = await instance.get(`/orders/${id}`);
    return { data: response.data, error: null };
  } catch (error) {
    console.error("Error al obtener orden:", error);
    return { data: null, error: error };
  }
};

// 3. Actualizar estado (Para guardar cambios) <--- ESTA TAMBIÉN
export const updateOrderStatus = async (id, newStatus) => {
  try {
    const response = await instance.put(`/orders/${id}/status`, { newStatus });
    return { data: response.data, error: null };
  } catch (error) {
    return { data: null, error: error };
  }
};
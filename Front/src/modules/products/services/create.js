import { instance } from '../../shared/api/axiosInstance';

export const createProduct = async (productData) => {
  try {
     // URL: Ponemos solo '/products' (Axios agrega el base /api)
    //  Enviamos productData directo (El formulario ya tiene los nombres correctos
    const response = await instance.post('/products', productData);

    //  Devolvemos el objeto que espera el componente
    return { data: response.data, error: null };
    
  } catch (error) {
    console.error("Error creating product:", error);
    // Retornamos el error para que el formulario muestre el mensaje rojo
    return { data: null, error: error };
  }
};
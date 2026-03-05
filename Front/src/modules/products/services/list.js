import { instance } from '../../shared/api/axiosInstance';

//Endpoint 2: Mostrar todos los productos
export const getProducts = async (search = null, status = null, pageNumber = 1, pageSize = 10 ) => {
  const queryString = new URLSearchParams({
    pageNumber,
    pageSize,
    // Enviamos los filtros solo si existen
    ...(search && { name: search }), // OJO: Tu backend en ProductModel.FilterProduct seguro espera 'name' o 'search'? Asumo 'name' o lo que pida el modelo.
    ...(status && status !== 'all' && { status }),
  });

  
  // Usamos '/products/admin' porque es el endpoint que acepta filtros y devuelve el total para paginar
  const response = await instance.get(`/products/admin?${queryString}`);

  return { data: response.data, error: null };
};

// Endpoint 3: Obtener producto por ID 
export const getProductById = async (id) => {
  try {
    // Llamada al backend: GET /api/products/{id}
    const response = await instance.get(`/products/${id}`);
    
    // Retornamos los datos del producto
    return { data: response.data, error: null };
  } catch (error) {
    console.error("Error al buscar producto por ID:", error);
    return { data: null, error: error };
  }
};
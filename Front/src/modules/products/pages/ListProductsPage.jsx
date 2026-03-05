import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Button from '../../shared/components/Button';
import { getProducts } from '../services/list';
import { frontendErrorMessage } from '../helpers/backendError';

const productStatus = {
  ALL: 'all',
  ENABLED: 'enabled',
  DISABLED: 'disabled',
};

function ListProductsPage() {
  const navigate = useNavigate();

  const [searchTerm, setSearchTerm] = useState('');
  const [status, setStatus] = useState(productStatus.ALL);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  
  const [total, setTotal] = useState(0);
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');

  const fetchProducts = async () => {
    try {
      setLoading(true);
      setErrorMessage('');

      const { data, error } = await getProducts(searchTerm, status, pageNumber, pageSize);

      if (error) throw error;

      if (data.productItems) {
          setTotal(data.total);
          setProducts(data.productItems);
      } else if (Array.isArray(data)) {
          setTotal(data.length);
          setProducts(data);
      }
    } catch (error) {
      const code = error.response?.status;
      setErrorMessage(frontendErrorMessage[code] || frontendErrorMessage.default);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchProducts();
  }, [status, pageSize, pageNumber]);

  const totalPages = Math.ceil(total / pageSize) || 1;

  const handleSearch = async () => {
    setPageNumber(1);
    await fetchProducts();
  };

  return (
    <div className="p-4 bg-gray-50 min-h-full">
      
      {/* 1. CABECERA RESPONSIVA */}
      <div className="flex justify-between items-center mb-6 bg-white p-4 rounded-lg shadow-sm">
        <h2 className="text-2xl font-bold text-gray-800">Productos</h2>
        
        {/* --- OPCIÓN A: EN MÓVIL (SOLO EL +) --- */}
        <button 
            onClick={() => navigate('/admin/products/create')}
            className="
                sm:hidden 
                w-10 h-10 
                bg-gray-100 hover:bg-gray-200 
                text-gray-800 font-bold text-2xl 
                rounded-full 
                flex items-center justify-center 
                shadow-sm transition-colors
            "
        >
            +
        </button>

        {/* --- OPCIÓN B: EN PC (BOTÓN CON TEXTO) --- */}
        <div className="hidden sm:block">
            <Button onClick={() => navigate('/admin/products/create')}>
              Crear Producto
            </Button>
        </div>
      </div>

      {/* 2. FILTROS */}
      <div className="flex flex-col md:flex-row gap-4 mb-6">
        <div className="flex flex-1 gap-2">
            <input 
                type="text"
                placeholder="Buscar"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full border border-gray-300 rounded-md px-4 py-2 focus:outline-none focus:ring-2 focus:ring-purple-500"
            />
            <button 
                onClick={handleSearch}
                className="bg-purple-100 text-purple-600 p-2 rounded-md hover:bg-purple-200 transition"
            >
                <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                </svg>
            </button>
        </div>

        <div className="w-full md:w-64">
            <select 
                value={status}
                onChange={(e) => setStatus(e.target.value)}
                className="w-full border border-gray-300 rounded-md px-4 py-2 bg-white focus:outline-none focus:ring-2 focus:ring-purple-500"
            >
                <option value={productStatus.ALL}>Estado de Producto</option>
                <option value={productStatus.ENABLED}>Activos</option>
                <option value={productStatus.DISABLED}>Inactivos</option>
            </select>
        </div>
      </div>

      {errorMessage && (
        <div className="mb-4 p-3 bg-red-100 text-red-700 rounded border border-red-200">
            {errorMessage}
        </div>
      )}

      {/* 3. LISTADO DE TARJETAS */}
      <div className="space-y-4">
        {loading ? (
            <p className="text-center text-gray-500">Cargando...</p>
        ) : products.length > 0 ? (
            products.map((product) => (
                <div key={product.id || product.productId} className="bg-white p-6 rounded-lg shadow-sm border border-gray-200 flex justify-between items-center">
                    <div>
                        <h3 className="text-lg font-bold text-gray-900 mb-1">
                            {product.sku} - {product.name}
                        </h3>
                        <p className="text-gray-500 text-sm">
                            Stock: {product.stockQuantity} - {product.isActive ? 'Activo' : 'Inactivo'}
                        </p>
                    </div>
                    <button 
                        className="bg-purple-100 text-purple-700 px-6 py-2 rounded-md font-medium hover:bg-purple-200 transition"
                        onClick={() => navigate(`/admin/products/edit/${product.id || product.productId}`)} 
                    >
                        Editar
                    </button>
                </div>
            ))
        ) : (
            !errorMessage && <div className="text-center p-8 bg-white rounded-lg">No se encontraron productos.</div>
        )}
      </div>

      {/* 4. PAGINACIÓN */}
      <div className="flex justify-center items-center mt-8 gap-4 text-gray-600">
        <button 
            disabled={pageNumber === 1}
            onClick={() => setPageNumber(pageNumber - 1)}
            className="hover:text-purple-600 disabled:opacity-50"
        >
            &larr; Previous
        </button>
        <span className="bg-gray-800 text-white px-3 py-1 rounded-md text-sm">
            {pageNumber}
        </span>
        <button 
            disabled={pageNumber >= totalPages}
            onClick={() => setPageNumber(pageNumber + 1)}
            className="hover:text-purple-600 disabled:opacity-50"
        >
            Next &rarr;
        </button>
      </div>
    </div>
  );
};

export default ListProductsPage;
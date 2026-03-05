import { useState, useEffect } from 'react';
import Navbar from '../Components/Navbar'; 
import { useNavigate } from 'react-router-dom';

const ImageIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 24 24" fill="none" stroke="#d1d5db" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
    <rect x="3" y="3" width="18" height="18" rx="2" ry="2"></rect>
    <circle cx="8.5" cy="8.5" r="1.5"></circle>
    <polyline points="21 15 16 10 5 21"></polyline>
  </svg>
);

function CatalogPage() {
  const [products, setProducts] = useState([]);
  const [quantities, setQuantities] = useState({});
  const [searchTerm, setSearchTerm] = useState("");
  const [loading, setLoading] = useState(true);

  // Usuario logueado
  const [user, setUser] = useState(null);

  // Paginación
  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 8;

  const navigate = useNavigate();

  // --- EFECTO: CARGAR PRODUCTOS Y USUARIO ---
  useEffect(() => {
    const userStored = localStorage.getItem('user');
    if (userStored) {
      try {
        setUser(JSON.parse(userStored));
      } catch (error) {
        console.error("Error leyendo usuario", error);
      }
    }

    const fetchAndNormalizeData = async () => {
      setLoading(true);
      let apiData = [];

      try {
        const fetchOptions = {
          method: 'GET',
          headers: { 'Cache-Control': 'no-cache' },
          cache: 'no-store'
        };

        let response = await fetch("http://localhost:3000/api/products", fetchOptions);
        if (!response.ok) response = await fetch("http://localhost:3000/api/Product", fetchOptions);

        if (response.ok) {
          apiData = await response.json();
        } else {
          throw new Error("Backend no disponible");
        }
      } catch (error) {
        // Datos demo
        apiData = Array.from({ length: 20 }, (_, i) => ({
          id: i + 1,
          name: `Producto Demo ${i + 1}`,
          currentPrice: (Math.random() * 1000).toFixed(0),
          stockQuantity: Math.floor(Math.random() * 20)
        }));
      }

      const cart = JSON.parse(localStorage.getItem('cart')) || [];

      const normalizedProducts = apiData.map(p => {
        const id = p.id || p.Id || p.productId;
        const name = p.name || p.Name || p.ProductName || "Producto sin nombre";
        const price = p.currentUnitPrice ?? p.currentPrice ?? p.price ?? 0;
        const realStock = p.stockQuantity ?? p.stock ?? p.Stock ?? 0;

        const inCartItem = cart.find(item => item.id === id);
        const quantityInCart = inCartItem ? inCartItem.quantity : 0;

        return {
          id: id,
          name: name,
          currentPrice: price,
          stockQuantity: Math.max(0, realStock - quantityInCart)
        };
      });

      setProducts(normalizedProducts);
      setLoading(false);
    };

    fetchAndNormalizeData();

    window.addEventListener('focus', fetchAndNormalizeData);
    return () => window.removeEventListener('focus', fetchAndNormalizeData);

  }, []);

  useEffect(() => {
    setCurrentPage(1);
  }, [searchTerm]);


  // Cerrar sesión
  const handleLogout = () => {
    localStorage.removeItem('user');
    setUser(null);
    navigate('/login');
  };

  const handleQty = (id, delta, currentStock) => {
    setQuantities(prev => {
      const currentQty = prev[id] || 0;
      const newQty = Math.max(0, currentQty + delta);
      if (newQty > currentStock) return prev;
      return { ...prev, [id]: newQty };
    });
  };

  const addToCart = (product) => {
    if (user && (user.role === 'Administrador' || user.role === 'Admin' || user.rol === 'Administrador')) {
      alert("ACCESO DENEGADO\nLos administradores no pueden realizar compras.");
      return;
    }

    const qty = quantities[product.id] || 0;
    if (qty === 0) return;

    try {
      const cart = JSON.parse(localStorage.getItem('cart')) || [];
      const existingIndex = cart.findIndex(item => item.id === product.id);

      if (existingIndex >= 0) {
        cart[existingIndex].quantity += qty;
      } else {
        cart.push({ ...product, quantity: qty, currentUnitPrice: product.currentPrice });
      }

      localStorage.setItem('cart', JSON.stringify(cart));

      setProducts(prevProducts =>
        prevProducts.map(p =>
          p.id === product.id
            ? { ...p, stockQuantity: Math.max(0, p.stockQuantity - qty) }
            : p
        )
      );

      alert(`Agregado al carrito: ${product.name}`);
      setQuantities(prev => ({ ...prev, [product.id]: 0 }));
    } catch (error) {
      console.error("Error al guardar en carrito:", error);
    }
  };

  // Filtro de productos
  const filteredProducts = products.filter(product =>
    product.name.toLowerCase().includes(searchTerm.toLowerCase())
  );

  // Paginación
  const indexOfLastItem = currentPage * itemsPerPage;
  const indexOfFirstItem = indexOfLastItem - itemsPerPage;
  const currentProducts = filteredProducts.slice(indexOfFirstItem, indexOfLastItem);
  const totalPages = Math.ceil(filteredProducts.length / itemsPerPage);

  const paginate = (pageNumber) => setCurrentPage(pageNumber);


  // RENDER
  return (
    <div className="min-h-screen bg-white font-sans text-gray-800">
      
      <Navbar 
        onSearch={setSearchTerm}
        user={user}
        onLogout={handleLogout}
      />

      <main className="max-w-7xl mx-auto px-4 py-8">

        <h1 className="text-2xl font-bold mb-6 text-purple-700">Catálogo de Productos</h1>

        {loading ? (
          <p className="text-center text-gray-500 py-10">Cargando productos...</p>
        ) : filteredProducts.length === 0 ? (
          <p className="text-gray-500 text-center py-10">No se encontraron productos.</p>
        ) : (
          <>
            <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">

              {currentProducts.map((product, index) => {
                const isWide = index >= 4;
                const currentQty = quantities[product.id] || 0;

                return (
                  <div
                    key={product.id}
                    className={`bg-white rounded-lg border border-gray-100 p-3 shadow-sm flex flex-col
                    ${isWide ? 'md:col-span-2' : 'md:col-span-1'}`}
                  >
                    <div
                      className={`bg-gray-100 rounded-md flex items-center justify-center mb-3
                      ${isWide ? 'h-48 md:h-56' : 'h-48'}`}
                    >
                      <ImageIcon />
                    </div>

                    <div className="mb-4">
                      <p className="text-xs text-gray-500 font-medium">Tecnología</p>
                      <h3 className="text-gray-900 font-bold text-base">{product.name}</h3>
                      <p className="font-bold text-sm text-gray-600">$ {product.currentPrice}</p>

                      <p className={`text-xs mt-1 font-bold 
                        ${product.stockQuantity > 0 ? 'text-green-600' : 'text-red-600'}`}>
                        Stock: {product.stockQuantity}
                      </p>
                    </div>

                    <div className="mt-auto flex justify-between items-center">
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => handleQty(product.id, -1, product.stockQuantity)}
                          className="text-gray-400 hover:text-black font-bold px-2"
                        >
                          –
                        </button>

                        <span className="text-sm font-medium w-4 text-center">
                          {currentQty}
                        </span>

                        <button
                          onClick={() => handleQty(product.id, 1, product.stockQuantity)}
                          className="text-gray-400 hover:text-black font-bold px-2"
                        >
                          +
                        </button>
                      </div>

                      <button
                        onClick={() => addToCart(product)}
                        disabled={product.stockQuantity === 0 || currentQty === 0}
                        className="bg-purple-100 text-purple-700 text-xs font-bold px-4 py-2 rounded-md hover:bg-purple-200 disabled:opacity-50 disabled:cursor-not-allowed"
                      >
                        {product.stockQuantity === 0 ? 'Agotado' : 'Agregar'}
                      </button>
                    </div>
                  </div>
                );
              })}
            </div>

            {filteredProducts.length > itemsPerPage && (
              <div className="flex justify-center items-center space-x-2 mt-8 pb-8">
                
                <button
                  onClick={() => paginate(currentPage - 1)}
                  disabled={currentPage === 1}
                  className={`px-4 py-2 rounded-md text-sm font-bold border 
                    ${currentPage === 1 ? 'border-gray-200 text-gray-300' : 'border-purple-200 text-purple-700'}`}
                >
                  Anterior
                </button>

                <div className="hidden sm:flex space-x-1">
                  {Array.from({ length: totalPages }, (_, i) => i + 1).map(number => (
                    <button
                      key={number}
                      onClick={() => paginate(number)}
                      className={`w-8 h-8 flex items-center justify-center rounded-md text-sm font-bold 
                        ${currentPage === number ? 'bg-purple-600 text-white' : 'text-gray-600'}`}
                    >
                      {number}
                    </button>
                  ))}
                </div>

                <button
                  onClick={() => paginate(currentPage + 1)}
                  disabled={currentPage === totalPages}
                  className={`px-4 py-2 rounded-md text-sm font-bold border 
                    ${currentPage === totalPages ? 'border-gray-200 text-gray-300' : 'border-purple-200 text-purple-700'}`}
                >
                  Siguiente
                </button>

              </div>
            )}

          </>
        )}

      </main>
    </div>
  );
}

export default CatalogPage;

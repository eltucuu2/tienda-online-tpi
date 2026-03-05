import { useEffect, useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import Navbar from '../Components/Navbar'; 
import useAuth from "../../auth/hook/useAuth"; 

// Importamos tus páginas de Auth para el modal
import LoginPage from "../../auth/pages/LoginPage";
import RegisterPage from "../../auth/pages/RegisterPage";

// --- MODAL WRAPPER ---
const AuthModal = ({ isOpen, onClose }) => {
  const [showRegister, setShowRegister] = useState(false);
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50 backdrop-blur-sm p-4">
      <div className="bg-white rounded-xl shadow-2xl w-full max-w-md relative p-6 animate-fade-in">
        <button onClick={onClose} className="absolute top-4 right-4 text-gray-400 hover:text-black font-bold z-10">✕</button>
        {showRegister ? (
            <div className="animate-fade-in">
                <RegisterPage />
                <div className="mt-4 text-center border-t pt-2">
                    <p className="text-sm text-gray-600">¿Ya tienes cuenta?</p>
                    <button onClick={() => setShowRegister(false)} className="text-purple-600 font-bold hover:underline">Iniciar Sesión</button>
                </div>
            </div>
        ) : (
            <div className="animate-fade-in">
                <LoginPage />
                <div className="mt-4 text-center border-t pt-2">
                    <p className="text-sm text-gray-600">¿No tienes cuenta?</p>
                    <button onClick={() => setShowRegister(true)} className="text-purple-600 font-bold hover:underline">Regístrate</button>
                </div>
            </div>
        )}
      </div>
    </div>
  );
};

function CartPage() {
  const [cartItems, setCartItems] = useState([]);
  const [showLoginModal, setShowLoginModal] = useState(false);

  const { isAuthenticated } = useAuth(); 
  const navigate = useNavigate();

  // 1. CARGA SEGURA DEL LOCALSTORAGE
  useEffect(() => {
    const storedCart = JSON.parse(localStorage.getItem('cart')) || [];
    const validCart = storedCart.map(item => ({
        ...item,
        currentUnitPrice: Number(item.currentUnitPrice || item.currentPrice || item.price || 0),
        quantity: Number(item.quantity || 1)
    }));
    setCartItems(validCart);
  }, []);

  const updateCart = (newCart) => {
    setCartItems(newCart);
    localStorage.setItem('cart', JSON.stringify(newCart));
  };

  const handleQuantity = (id, delta) => {
    const newCart = cartItems.map(item => {
      if (item.id === id) {
        return { ...item, quantity: Math.max(1, item.quantity + delta) };
      }
      return item;
    });
    updateCart(newCart);
  };

  const removeItem = (id) => {
    const newCart = cartItems.filter(item => item.id !== id);
    updateCart(newCart);
  };

  const totalPrice = cartItems.reduce((acc, item) => {
      return acc + (item.currentUnitPrice * item.quantity);
  }, 0);

  const totalQuantity = cartItems.reduce((acc, item) => acc + item.quantity, 0);

  // --- CHECKOUT SIMULADO ---
  const handleCheckout = () => {
    if (cartItems.length === 0) return;

    if (!isAuthenticated) {
        setShowLoginModal(true);
        return;
    }

    alert("¡Compra realizada con éxito! (Simulación Local)");
    localStorage.removeItem('cart');
    setCartItems([]);
    navigate('/');
  };

  useEffect(() => {
    if (isAuthenticated && showLoginModal) {
        setShowLoginModal(false);
    }
  }, [isAuthenticated, showLoginModal]);

  return (
    <div className="min-h-screen bg-gray-50 font-sans text-gray-800 pb-10">
      <Navbar />
      
      <AuthModal isOpen={showLoginModal} onClose={() => setShowLoginModal(false)} />

      <main className="max-w-7xl mx-auto px-4 py-8">
        
        {/* Cabecera */}
        {/* En móvil se oculta el título según tu diseño, pero lo dejaré visible para navegación */}
        <div className="flex items-center justify-between mb-6">
            <h1 className="text-2xl font-bold text-gray-800 hidden md:block">Carrito de compras</h1>
            <Link to="/" className="text-purple-600 font-semibold hover:underline">← Seguir comprando</Link>
        </div>
        
        {/* LAYOUT PRINCIPAL: Columna en Mobile, Fila en Desktop */}
        <div className="flex flex-col lg:flex-row gap-8 items-start">
          
          {/* --- LISTA DE PRODUCTOS (IZQUIERDA) --- */}
          <div className="w-full lg:flex-1 flex flex-col gap-4">
            {cartItems.length === 0 ? (
                <div className="text-center py-16 bg-white rounded-xl border border-gray-100 shadow-sm">
                    <p className="text-gray-500 text-lg">Tu carrito está vacío.</p>
                </div>
            ) : (
                cartItems.map(item => (
                    // TARJETA DE PRODUCTO (Diseño exacto a la imagen)
                    <div key={item.id} className="bg-white border border-gray-200 rounded-lg p-5 shadow-sm">
                        
                        {/* 1. Nombre del Producto */}
                        <h3 className="font-bold text-gray-900 text-lg mb-2">{item.name}</h3>
                        
                        {/* 2. Info (Cantidad y Subtotal) */}
                        <div className="text-gray-500 text-sm space-y-1 mb-4">
                            <p>Cantidad de productos: <span className="font-medium text-gray-700">{item.quantity}</span></p>
                            <p>Sub Total: <span className="font-medium text-gray-700">${(item.currentUnitPrice * item.quantity).toFixed(2)}</span></p>
                        </div>

                        {/* 3. Controles (Alineados a la derecha) */}
                        <div className="flex justify-end items-center gap-3">
                             {/* Selector de Cantidad (+ -) */}
                             <div className="flex items-center border border-gray-300 rounded bg-white">
                                <button 
                                    onClick={() => handleQuantity(item.id, -1)} 
                                    className="px-3 py-1 text-gray-600 hover:bg-gray-100 font-bold"
                                >–</button>
                                <span className="px-2 text-sm font-medium w-6 text-center">{item.quantity}</span>
                                <button 
                                    onClick={() => handleQuantity(item.id, 1)} 
                                    className="px-3 py-1 text-gray-600 hover:bg-gray-100 font-bold"
                                >+</button>
                             </div>
                             
                             {/* Botón Borrar (Estilo Violeta Claro) */}
                             <button 
                                onClick={() => removeItem(item.id)} 
                                className="bg-purple-100 text-purple-700 px-4 py-1.5 rounded text-sm font-bold hover:bg-purple-200 transition-colors"
                             >
                                Borrar
                             </button>
                        </div>
                    </div>
                ))
            )}
          </div>

          {/* --- DETALLE DE PEDIDO (DERECHA) --- */}
          {cartItems.length > 0 && (
            <div className="w-full lg:w-1/3 lg:sticky lg:top-24">
                <div className="bg-white border border-gray-200 rounded-lg p-6 shadow-sm">
                    <h2 className="font-bold text-xl mb-6 text-gray-900">Detalle de pedido</h2>
                    
                    <div className="space-y-4 mb-8 text-gray-600 text-sm">
                        <div className="flex justify-between">
                            <span>Cantidad de en total:</span>
                            <span className="font-medium text-gray-800">{totalQuantity}</span>
                        </div>
                        <div className="flex justify-between">
                            <span>Total a pagar:</span>
                            <span className="font-bold text-gray-800">${totalPrice.toFixed(2)}</span>
                        </div>
                    </div>
                    
                    <button 
                        onClick={handleCheckout} 
                        className="w-full bg-purple-200 text-purple-900 font-bold py-3 rounded-lg hover:bg-purple-300 transition-colors"
                    >
                        {isAuthenticated ? "Finalizar Compra" : "Iniciar Sesión"}
                    </button>
                </div>
            </div>
          )}
        </div>
      </main>
    </div>
  );
}

export default CartPage;
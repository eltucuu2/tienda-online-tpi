import { useState } from 'react';
import { Link } from 'react-router-dom';

// --- ICONOS SVG (Sin cambios) ---
const SearchIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="text-gray-400">
    <circle cx="11" cy="11" r="8"></circle>
    <line x1="21" y1="21" x2="16.65" y2="16.65"></line>
  </svg>
);

const MenuIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="text-black">
    <line x1="3" y1="12" x2="21" y2="12"></line>
    <line x1="3" y1="6" x2="21" y2="6"></line>
    <line x1="3" y1="18" x2="21" y2="18"></line>
  </svg>
);

const CloseIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="text-gray-500">
    <line x1="18" y1="6" x2="6" y2="18"></line>
    <line x1="6" y1="6" x2="18" y2="18"></line>
  </svg>
);

// AHORA RECIBIMOS 'user' y 'onLogout' COMO PROPS
export default function Navbar({ onSearch, user, onLogout }) {
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  // Función para manejar el cambio en el input de forma segura
  const handleSearchChange = (e) => {
    if (onSearch) {
      onSearch(e.target.value);
    }
  };

  return (
    <>
      {/* --- MENU LATERAL MOBILE --- */}
      {isMobileMenuOpen && (
        <div className="fixed inset-0 z-[60] flex">
          <div 
            className="fixed inset-0 bg-black bg-opacity-50" 
            onClick={() => setIsMobileMenuOpen(false)}
          ></div>
          
          <div className="relative bg-white w-64 h-full shadow-2xl p-6 flex flex-col gap-6">
            <div className="flex justify-end">
              <button onClick={() => setIsMobileMenuOpen(false)}>
                <CloseIcon />
              </button>
            </div>
            
            <nav className="flex flex-col gap-4 text-lg font-semibold text-gray-700">
              <Link to="/" onClick={() => setIsMobileMenuOpen(false)} className="hover:text-purple-600">
                Productos
              </Link>
              <Link to="/cart" onClick={() => setIsMobileMenuOpen(false)} className="hover:text-purple-600">
                Carrito de compras
              </Link>
            </nav>
            
            <hr className="border-gray-200" />
            
            {/* LÓGICA DE USUARIO PARA MOBILE */}
            <div className="flex flex-col gap-3">
              {user ? (
                // Si hay usuario logueado
                <>
                  <div className="text-center text-sm font-bold text-purple-700 mb-2">
                     Hola, {user.name || 'Usuario'}
                  </div>
                  <button 
                    onClick={() => {
                        onLogout();
                        setIsMobileMenuOpen(false);
                    }} 
                    className="w-full py-3 bg-red-100 text-red-700 font-bold rounded-xl text-center hover:bg-red-200"
                  >
                    Cerrar Sesión
                  </button>
                </>
              ) : (
                // Si NO hay usuario (Invitado)
                <>
                  <Link to="/login" onClick={() => setIsMobileMenuOpen(false)} className="w-full py-3 bg-purple-100 text-purple-700 font-bold rounded-xl text-center">
                    Iniciar Sesión
                  </Link>
                  <Link to="/signup" onClick={() => setIsMobileMenuOpen(false)} className="w-full py-3 bg-gray-100 text-gray-700 font-bold rounded-xl text-center">
                    Registrarse
                  </Link>
                </>
              )}
            </div>
          </div>
        </div>
      )}

      {/* --- HEADER PRINCIPAL --- */}
      <header className="sticky top-0 z-50 bg-white border-b border-gray-100 py-3 shadow-sm">
        <div className="max-w-7xl mx-auto px-4 flex items-center justify-between gap-4">
          
          <div className="flex items-center gap-8">
             <Link to="/" className="w-8 h-8 border-2 border-black rounded flex items-center justify-center font-bold text-lg text-black no-underline">
               B
             </Link>

             <nav className="hidden md:flex gap-6 text-sm font-semibold text-gray-600">
                <Link to="/" className="hover:text-black">Productos</Link>
                <Link to="/cart" className="hover:text-black">Carrito de compras</Link>
             </nav>
          </div>

          {/* BUSCADOR CONECTADO */}
          <div className="flex-1 max-w-lg relative">
            <div className="absolute left-3 top-1/2 -translate-y-1/2">
                <SearchIcon />
            </div>
            <input 
              type="text" 
              placeholder="Search" 
              className="w-full bg-gray-50 border border-gray-200 rounded-full py-2 pl-10 pr-4 text-sm focus:outline-none focus:ring-1 focus:ring-purple-300"
              onChange={handleSearchChange} 
            />
          </div>

          <div className="flex items-center">
            {/* LÓGICA DE USUARIO PARA DESKTOP */}
            <div className="hidden md:flex items-center gap-3">
               {user ? (
                 // Si hay usuario: Mostramos Saludo y Botón Cerrar Sesión
                 <>
                   <span className="text-sm font-bold text-gray-600 mr-2">
                      Hola, <span className="text-purple-700">{user.name || 'Usuario'}</span>
                   </span>
                   <button 
                     onClick={onLogout}
                     className="px-4 py-2 bg-gray-200 text-gray-700 font-bold rounded-lg text-xs hover:bg-gray-300 transition-colors"
                   >
                     Cerrar Sesión
                   </button>
                 </>
               ) : (
                 // Si NO hay usuario: Mostramos botones originales
                 <>
                   <Link to="/login" className="px-4 py-2 bg-purple-100 text-purple-700 font-bold rounded-lg text-xs hover:bg-purple-200">
                     Iniciar Sesión
                   </Link>
                   <Link to="/signup" className="px-4 py-2 bg-gray-200 text-gray-700 font-bold rounded-lg text-xs hover:bg-gray-300">
                     Registrarse
                   </Link>
                 </>
               )}
            </div>

            <button className="md:hidden ml-2 p-1" onClick={() => setIsMobileMenuOpen(true)}>
               <MenuIcon />
            </button>
          </div>
        </div>
      </header>
    </>
  );
}
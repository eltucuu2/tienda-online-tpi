import { createBrowserRouter, Outlet, RouterProvider } from 'react-router-dom';
import { AuthProvider } from './modules/auth/context/AuthProvider';

import LoginPage from './modules/auth/pages/LoginPage';
import RegisterPage from './modules/auth/pages/RegisterPage';
import Dashboard from './modules/templates/components/Dashboard';
import ProtectedRoute from './modules/auth/components/ProtectedRoute';

import ListOrdersPage from './modules/orders/pages/ListOrdersPage';
import EditOrderPage from './modules/orders/pages/EditOrderPage';
import Home from './modules/home/pages/Home';
import ListProductsPage from './modules/products/pages/ListProductsPage';
import CreateProductPage from './modules/products/pages/CreateProductPage';
import EditProductPage from './modules/products/pages/EditProductPage';

import CatalogPage from './modules/store/pages/CatalogPage';
import CartPage from './modules/store/pages/CartPage';

// --- CONFIGURACIÓN DEL ROUTER (IMPORTANTE: FUERA DE LA FUNCIÓN APP) ---
const router = createBrowserRouter([
  {
    path: '/',
    element: <div><Outlet /></div>, // envolver Outlet en un div seguro
    children: [
      {
        index: true, 
        element: <CatalogPage />, 
      },
      {
        path: 'cart', 
        element: <CartPage />, 
      },
    ],
  },
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    path: '/signup',
    element: <RegisterPage />,
  },
  // Rutas Admin
  {
    path: '/admin',
    element: (
      <ProtectedRoute>
        <Dashboard />
      </ProtectedRoute>
    ),
    children: [
      // ⚠️ Aquí eliminamos el prefijo '/admin/' para que sean relativas al padre
      { path: 'home', element: <Home /> },
      { path: 'products', element: <ListProductsPage /> },
      { path: 'products/create', element: <CreateProductPage /> },
      { path: 'products/edit/:id', element: <EditProductPage /> },
      { path: 'orders', element: <ListOrdersPage /> },
      { path: 'orders/edit/:id', element: <EditOrderPage /> }
    ],
  },
]);

function App() {
  return (
    <AuthProvider>
      <RouterProvider router={router} />
    </AuthProvider>
  );
}

export default App;

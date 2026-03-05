import { createContext, useState, useEffect } from 'react';
import { jwtDecode } from 'jwt-decode'; 
import { login } from '../services/login';

const AuthContext = createContext();

function AuthProvider({ children }) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [user, setUser] = useState(null); 
  const [loading, setLoading] = useState(true);

  const decodeTokenAndSetUser = (token) => {
    try {
      const decoded = jwtDecode(token);

      const role =
        decoded.role ||
        decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];

      setUser({
        username: decoded.sub || decoded.unique_name,
        role: role
      });

      setIsAuthenticated(true);
    } catch (error) {
      console.error("Token inválido", error);
      singout();
    }
  };

  useEffect(() => {
    const token = localStorage.getItem("token");
    if (token) {
      decodeTokenAndSetUser(token);
    }
    setLoading(false);
  }, []);

  const singout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("cart");
    setIsAuthenticated(false);
    setUser(null);
  };

  // 🔥 FIX DEFINITIVO — AHORA DEVUELVE { data, error }
  const singin = async (username, password) => {
    try {
      const { data, error } = await login(username, password);

      if (error) {
        return { data: null, error };
      }

      const token = data.token || data;

      localStorage.setItem("token", token);

      decodeTokenAndSetUser(token);

      // 💥 DEVOLVEMOS DATA como espera el LoginForm
      return { data, error: null };

    } catch (err) {
      return { data: null, error: err };
    }
  };

  return (
    <AuthContext.Provider
      value={{
        isAuthenticated,
        user,
        singin,
        singout,
        loading
      }}
    >
      {!loading && children}
    </AuthContext.Provider>
  );
}

export { AuthProvider, AuthContext };

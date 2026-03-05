import { useContext } from 'react';
import { AuthContext } from '../context/AuthProvider';

const useAuth = () => {
  const context = useContext(AuthContext);

  if (!context) {
    // Solo corregimos esto para que detecte errores reales, no afecta la lógica
    throw new Error('useAuth no debe ser usado por fuera de AuthProvider');
  }

  return {
    // Mantenemos lo que ya tenías funcionando:
    isAuthenticated: context.isAuthenticated,
    singin: context.singin,   // NO cambiamos el nombre
    singout: context.singout, // NO cambiamos el nombre

    // Agregamos SOLO lo nuevo que pide el TPI:
    user: context.user,         // Necesario para ver si es Admin o User
    register: context.register  // Necesario para el botón "Registrar Usuario" del modal
  };
};

export default useAuth;
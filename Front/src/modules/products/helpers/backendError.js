export const frontendErrorMessage = {
  // --- Errores de Negocio (Específicos del Backend) ---
  3000: "Por favor verifique SKU. El formato soportado es 'SKU-XXXX'",
  // Puedes agregar otros códigos específicos aquí si aparecen (ej: 3001, 3002...)

  // --- Errores HTTP Estándar (Requeridos por el PDF) ---
  400: "Datos incorrectos. Por favor revisa la información enviada.",
  401: "Sesión expirada o credenciales inválidas. Inicie sesión nuevamente.",
  403: "Acceso denegado. No tienes permisos de Administrador.",
  404: "No se encontró el recurso solicitado (Producto o servicio no disponible).",
  409: "Conflicto: Ya existe un producto con ese código o nombre.",
  
  // Requisito PDF Pág 11: Error del servidor
  500: "Error interno del servidor. Por favor, intente más tarde.",
  
  // --- Fallback (Por defecto) ---
  default: "Ocurrió un error inesperado. Contacte a soporte."
};
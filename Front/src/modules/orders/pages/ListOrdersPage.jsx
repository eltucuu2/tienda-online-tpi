import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { getOrders } from "../services/orderService";
import { frontendErrorMessage } from "../../products/helpers/backendError";

const orderStatus = {
  ALL: "all",
  PENDING: "Pending",
  PROCESSING: "Processing",
  SHIPPED: "Shipped",
  DELIVERED: "Delivered",
  CANCELLED: "Cancelled"
};

export default function ListOrdersPage() {
  const navigate = useNavigate();

  // Estados
  const [searchTerm, setSearchTerm] = useState("");
  const [status, setStatus] = useState(orderStatus.ALL);
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  // Helper para traducir estados
  const formatStatus = (status) => {
    const map = {
      PENDING: "Pendiente",
      PROCESSING: "Procesando",
      SHIPPED: "Enviado",
      DELIVERED: "Entregado",
      CANCELLED: "Cancelado"
    };
    return map[status?.toUpperCase()] || status;
  };

  // --- FUNCIÓN DE CARGA ---
  const fetchOrders = async () => {
    setLoading(true);
    setErrorMessage("");

    try {
        // 1. Limpieza para que busque bien
        // Si está vacío, manda null (trae todo). Si escribiste "Sofía", manda "Sofía".
        const termToSend = searchTerm.trim() === "" ? null : searchTerm;
        const statusToSend = status === orderStatus.ALL ? null : status;

        // 2. Pedimos las órdenes (50 para llenar la lista)
        const { data, error } = await getOrders(termToSend, statusToSend, 1, 50);

        if (error) {
            const code = error.response?.status;
            setErrorMessage(frontendErrorMessage[code] || "Error al cargar órdenes.");
        } else {
            setOrders(data?.orderItems || []);
        }
    } catch (err) {
        console.error(err);
        setErrorMessage("Error de conexión.");
    } finally {
        setLoading(false);
    }
  };

  // --- EFECTO: BÚSQUEDA EN VIVO ---
  // Esto hace la magia: Cada vez que tocas una tecla en 'searchTerm', espera y busca.
  useEffect(() => {
    const delay = setTimeout(() => {
        fetchOrders();
    }, 500); // Espera 0.5 seg mientras escribes "Sofía"

    return () => clearTimeout(delay);
  }, [searchTerm, status]); // Se ejecuta al escribir o cambiar filtro

  return (
    <div className="p-6 bg-gray-50 min-h-screen">
      <h1 className="text-2xl font-bold mb-6 text-gray-800">Listado de Órdenes</h1>

      {/* --- BARRA DE BÚSQUEDA Y FILTRO --- */}
      <div className="flex flex-col md:flex-row gap-4 mb-8">
        <div className="relative flex-1">
            <input
              type="text"
              placeholder="Buscar por nombre de cliente..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full border border-gray-300 rounded-md px-4 py-2 pl-10 focus:ring-2 focus:ring-purple-500 outline-none shadow-sm"
            />
            {/* Lupa decorativa */}
            <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 text-gray-400 absolute left-3 top-2.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
        </div>

        <select
            value={status}
            onChange={(e) => setStatus(e.target.value)}
            className="w-full md:w-48 border border-gray-300 rounded-md px-4 py-2 bg-white shadow-sm focus:ring-2 focus:ring-purple-500 outline-none"
        >
            <option value={orderStatus.ALL}>Todos los estados</option>
            <option value={orderStatus.PENDING}>Pendiente</option>
            <option value={orderStatus.PROCESSING}>Procesando</option>
            <option value={orderStatus.SHIPPED}>Enviado</option>
            <option value={orderStatus.DELIVERED}>Entregado</option>
            <option value={orderStatus.CANCELLED}>Cancelado</option>
        </select>
      </div>

      {/* ERROR */}
      {errorMessage && (
        <div className="bg-red-100 text-red-700 p-3 rounded mb-6 border border-red-200">
            {errorMessage}
        </div>
      )}

      {/* --- LISTADO (TARJETAS) --- */}
      <div className="space-y-4">
        {loading ? (
          <p className="text-gray-500 text-center py-10">Cargando...</p>
        ) : orders.length === 0 ? (
          <div className="text-center py-12 bg-white rounded-lg border border-gray-200 shadow-sm">
            <p className="text-gray-500 text-lg">No se encontraron órdenes.</p>
          </div>
        ) : (
          orders.map((order) => (
            <div
              key={order.id}
              className="bg-white p-5 rounded-lg border border-gray-200 shadow-sm flex flex-col sm:flex-row justify-between items-center gap-4 hover:shadow-md transition-all"
            >
              <div className="flex flex-col">
                {/* AQUI ESTÁ EL CAMBIO: Quité el ID, dejé solo estado y Nombre */}
                <div className="mb-1">
                    <span className={`text-xs font-bold px-2 py-0.5 rounded ${
                        order.status === 'Delivered' ? 'bg-green-100 text-green-700' : 
                        order.status === 'Cancelled' ? 'bg-red-100 text-red-700' : 'bg-yellow-100 text-yellow-800'
                    }`}>
                        {formatStatus(order.status)}
                    </span>
                </div>
                
                {/* NOMBRE DEL CLIENTE (Bien visible como pediste) */}
                <h3 className="font-bold text-lg text-gray-800">{order.customerName || "Cliente Desconocido"}</h3>
                
                <p className="text-sm text-gray-500">
                    Total: <span className="font-bold text-gray-900">${order.totalAmount}</span>
                </p>
              </div>

              <button
                onClick={() => navigate(`/admin/orders/edit/${order.id}`)}
                className="bg-purple-50 text-purple-700 border border-purple-200 px-6 py-2 rounded-md font-medium hover:bg-purple-100 transition-colors w-full sm:w-auto"
              >
                Ver Detalle
              </button>
            </div>
          ))
        )}
      </div>
    </div>
  );
}
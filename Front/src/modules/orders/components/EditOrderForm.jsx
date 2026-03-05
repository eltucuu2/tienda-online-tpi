import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate, useParams } from 'react-router-dom';
import Card from '../../shared/components/Card';
import { getOrderById, updateOrderStatus } from '../services/orderService';
import { frontendErrorMessage } from '../../products/helpers/backendError';

const ORDER_STATUSES = [
  { value: 'Pending', label: 'Pendiente' },
  { value: 'Processing', label: 'Procesando' },
  { value: 'Shipped', label: 'Enviado' },
  { value: 'Delivered', label: 'Entregado' },
  { value: 'Cancelled', label: 'Cancelado' }
];

function EditOrderForm() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [order, setOrder] = useState(null);
  const [loading, setLoading] = useState(true);
  const [errorMsg, setErrorMsg] = useState('');

  const { register, handleSubmit } = useForm();

  // 1. Cargar la orden al entrar
  useEffect(() => {
    const loadData = async () => {
      const { data, error } = await getOrderById(id);
      if (error) {
        setErrorMsg("Error al cargar la orden.");
      } else {
        setOrder(data);
      }
      setLoading(false);
    };
    loadData();
  }, [id]);

  // 2. Guardar el nuevo estado
  const onValid = async (formData) => {
    try {
      const { error } = await updateOrderStatus(id, formData.status);
      
      if (error) throw error;

      navigate('/admin/orders'); // Volver al listado
    } catch (error) {
      const code = error.response?.status;
      setErrorMsg(frontendErrorMessage[code] || 'No se pudo actualizar el estado.');
    }
  };

  if (loading) return <p className="p-6 text-center">Cargando orden...</p>;
  if (!order) return <p className="p-6 text-center text-red-500">Orden no encontrada.</p>;

  return (
    <Card className="max-w-2xl mx-auto">
      <div className="p-6">
        <h2 className="text-xl font-bold text-gray-800 mb-4">
            Gestionar Orden #{order.id.substring(0, 8)}
        </h2>

        {/* Resumen de la Orden (Solo lectura) */}
        <div className="bg-gray-50 p-4 rounded-lg mb-6 text-sm space-y-2 border border-gray-200">
            <p><span className="font-bold">Cliente:</span> {order.customerName}</p>
            <p><span className="font-bold">Fecha:</span> {new Date(order.date).toLocaleDateString()}</p>
            <p><span className="font-bold">Total:</span> <span className="text-green-600 font-bold">${order.totalAmount}</span></p>
            <div className="mt-2 pt-2 border-t border-gray-200">
                <p className="font-bold mb-1">Items:</p>
                <ul className="list-disc list-inside text-gray-600">
                    {order.orderItems?.map((item, index) => (
                        <li key={index}>
                            {item.quantity}x {item.productName} (${item.unitPrice})
                        </li>
                    ))}
                </ul>
            </div>
        </div>

        {/* Formulario de Cambio de Estado */}
        <form onSubmit={handleSubmit(onValid)} className="flex flex-col gap-4">
            <div className="flex flex-col gap-1">
                <label className="text-gray-700 font-medium">Estado Actual</label>
                <select 
                    defaultValue={order.status} // Valor inicial
                    {...register('status', { required: true })}
                    className="w-full border border-gray-300 rounded-lg px-4 py-2 focus:outline-none focus:ring-2 focus:ring-purple-200 bg-white"
                >
                    {ORDER_STATUSES.map(s => (
                        <option key={s.value} value={s.value}>{s.label}</option>
                    ))}
                </select>
            </div>

            {errorMsg && <div className="text-red-500 text-sm text-center">{errorMsg}</div>}

            <div className="flex justify-end gap-3 mt-4">
                <button 
                    type="button" 
                    onClick={() => navigate('/admin/orders')}
                    className="bg-gray-100 text-gray-600 font-bold px-4 py-2 rounded-lg hover:bg-gray-200 transition"
                >
                    Cancelar
                </button>
                <button 
                    type="submit" 
                    className="bg-purple-200 text-purple-800 font-bold px-6 py-2 rounded-lg hover:bg-purple-300 transition shadow-sm"
                >
                    Actualizar Estado
                </button>
            </div>
        </form>
      </div>
    </Card>
  );
}

export default EditOrderForm;
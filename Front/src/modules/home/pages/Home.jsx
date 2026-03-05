import { useEffect, useState } from 'react';
import Card from '../../shared/components/Card';
import { getProducts } from '../../products/services/list';
import { getOrders } from '../../orders/services/orderService';

function Home() {
  const [stats, setStats] = useState({ products: 0, orders: 0 });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadStats = async () => {
      try {
        // Pedimos 1 ítem solo para leer el total
        const prodRes = await getProducts('', 'all', 1, 1);
        const ordRes = await getOrders(1, 1, 'all');

        setStats({
          products: prodRes.data?.total || prodRes.data?.length || 0,
          orders: ordRes.data?.total || ordRes.data?.length || 0
        });
      } catch (error) {
        console.error("Error cargando estadísticas", error);
      } finally {
        setLoading(false);
      }
    };
    loadStats();
  }, []);

  if (loading) return <p className="p-4">Cargando resumen...</p>;

  return (
    <div className='flex flex-col gap-6 sm:grid sm:grid-cols-2'>
      
      {/* Tarjeta Productos */}
      <Card className="p-6 border-l-4 border-purple-500">
        <h3 className="text-gray-500 font-medium text-lg">Productos</h3>
        <p className="text-4xl font-bold mt-2 text-gray-800">
            Cantidad: {stats.products}
        </p>
      </Card>

      {/* Tarjeta Órdenes */}
      <Card className="p-6 border-l-4 border-blue-500">
        <h3 className="text-gray-500 font-medium text-lg">Ordenes</h3>
        <p className="text-4xl font-bold mt-2 text-gray-800">
            Cantidad: {stats.orders}
        </p>
      </Card>

    </div>
  );
};

export default Home;
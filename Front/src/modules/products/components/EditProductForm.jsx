import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate, useParams } from 'react-router-dom';
import Card from '../../shared/components/Card';
// --- IMPORTS CORREGIDOS ---
import { getProductById } from '../services/list'; 
import { updateProduct } from '../services/update';
// --------------------------
import { frontendErrorMessage } from '../helpers/backendError';

function EditProductForm() {
  const { id } = useParams(); 
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(true);
  const [errorBackendMessage, setErrorBackendMessage] = useState('');

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset, 
  } = useForm();

  // Cargar datos
  useEffect(() => {
    const loadData = async () => {
      try {
        const { data, error } = await getProductById(id);
        if (error) throw error;
        if (data) {
          reset({
            sku: data.sku,
            internalCode: data.internalCode,
            name: data.name,
            description: data.description,
            currentPrice: data.currentPrice,
            stockQuantity: data.stockQuantity,
          });
        }
      } catch (error) {
        setErrorBackendMessage("No se pudo cargar el producto.");
      } finally {
        setIsLoading(false);
      }
    };
    loadData();
  }, [id, reset]);

  const onValid = async (formData) => {
    try {
        const { error } = await updateProduct(id, formData);
        if (error) throw error;
        navigate('/admin/products');
    } catch (error) {
        const code = error.response?.status;
        setErrorBackendMessage(frontendErrorMessage[code] || frontendErrorMessage.default);
    }
  };

  // Componente interno para input limpio
  const InputField = ({ label, name, type = "text", validation, error }) => (
    <div className="flex flex-col gap-1">
      <label className="text-sm font-semibold text-gray-600 ml-1">{label}</label>
      <input
        type={type}
        {...register(name, validation)}
        className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-purple-200 transition-all"
      />
      {error && <span className="text-red-400 text-xs ml-1">{error}</span>}
    </div>
  );

  if (isLoading) return <p className="p-6 text-center text-gray-500">Cargando datos...</p>;

  return (
    <Card className="max-w-md mx-auto">
      <form className='flex flex-col gap-3 p-5' onSubmit={handleSubmit(onValid)}>
        
        <h2 className="text-lg font-bold text-gray-800 mb-2">Editar Producto</h2>
        
        <InputField 
            label="SKU" 
            name="sku" 
            validation={{ required: 'Requerido' }} 
            error={errors.sku?.message} 
        />
        
        <InputField 
            label="Código Único" 
            name="internalCode" 
            validation={{ required: 'Requerido' }} 
            error={errors.internalCode?.message} 
        />
        
        <InputField 
            label="Nombre" 
            name="name" 
            validation={{ required: 'Requerido' }} 
            error={errors.name?.message} 
        />
        
        <div className="flex flex-col gap-1">
            <label className="text-sm font-semibold text-gray-600 ml-1">Descripción</label>
            <input
                {...register('description')}
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-purple-200 transition-all"
            />
        </div>
        
        <div className="grid grid-cols-2 gap-3">
            <InputField 
                label="Precio" 
                name="currentPrice" 
                type="number"
                validation={{ required: 'Requerido', min: { value: 0.01, message: '> 0' }, valueAsNumber: true }} 
                error={errors.currentPrice?.message} 
            />
            
            <InputField 
                label="Stock" 
                name="stockQuantity" 
                type="number"
                validation={{ required: 'Requerido', min: { value: 0, message: 'No negativo' }, valueAsNumber: true }} 
                error={errors.stockQuantity?.message} 
            />
        </div>

        <div className='flex justify-end gap-2 mt-4'>
            <button 
                type="button" 
                onClick={() => navigate('/admin/products')} 
                className="bg-gray-100 text-gray-600 font-bold px-4 py-2 rounded-lg text-sm hover:bg-gray-200 transition-colors"
            >
                Cancelar
            </button>
            <button 
                type='submit' 
                className="bg-purple-200 text-purple-800 font-bold px-4 py-2 rounded-lg text-sm hover:bg-purple-300 transition-colors shadow-sm"
            >
                Guardar Cambios
            </button>
        </div>
        
        {errorBackendMessage && <span className='text-red-500 block mt-2 text-center text-sm'>{errorBackendMessage}</span>}
      </form>
    </Card>
  );
};

export default EditProductForm;
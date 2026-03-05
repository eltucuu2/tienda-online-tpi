import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router-dom';
import Button from '../../shared/components/Button';
import Card from '../../shared/components/Card';
import { createProduct } from '../services/create';
import { useState } from 'react';
import { frontendErrorMessage } from '../helpers/backendError';

function CreateProductForm() {
  const {
    register,
    formState: { errors },
    handleSubmit,
  } = useForm({
    defaultValues: {
      sku: '',
      internalCode: '',
      name: '',
      description: '',
      // IMPORTANTE: Si quieres que aparezcan vacíos al inicio para obligar a escribir,
      // puedes quitar el 0 inicial o dejarlo como string vacío ''.
      // Si dejas 0, el campo se considera "lleno" con el valor 0.
      currentPrice: '', 
      stockQuantity: '', 
    },
  });

  const [errorBackendMessage, setErrorBackendMessage] = useState('');
  const navigate = useNavigate();

  const onValid = async (formData) => {
    try {
      // Convertimos a número aquí si usaste string vacío en defaultValues
      const payload = {
          ...formData,
          currentPrice: Number(formData.currentPrice),
          stockQuantity: Number(formData.stockQuantity)
      };

      const { error } = await createProduct(payload);
      
      if (error) {
        const code = error.response?.status;
        const serverMsg = error.response?.data?.message || error.response?.data;
        
        if (code === 409 || (serverMsg && serverMsg.includes("existe"))) {
             alert(` Error: ${serverMsg || "El producto ya existe (SKU o Código duplicado)."}`);
        } else if (error.response?.data?.code) {
             const errorMessage = frontendErrorMessage[error.response.data.code];
             setErrorBackendMessage(errorMessage);
        } else {
             setErrorBackendMessage('Error al crear el producto.');
        }
        return;
      }

      navigate('/admin/products');
    } catch (error) {
      setErrorBackendMessage('Error inesperado de conexión.');
    }
  };

  // Componente de Input reutilizable
  const InputField = ({ label, name, type = "text", validation, error }) => (
    <div className="flex flex-col gap-1">
      <label className="text-sm font-semibold text-gray-600 ml-1">{label}</label>
      <input
        type={type}
        {...register(name, validation)}
        className={`w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 transition-all 
            ${error ? 'border-red-500 focus:ring-red-200' : 'border-gray-300 focus:ring-purple-200'}`}
      />
      {error && <span className="text-red-500 text-xs ml-1 font-bold">{error}</span>}
    </div>
  );

  return (
    <Card className="max-w-md mx-auto">
      <form className='flex flex-col gap-3 p-5' onSubmit={handleSubmit(onValid)}>
        <h2 className="text-lg font-bold text-gray-800 mb-2 sm:hidden">Nuevo Producto</h2>

        <InputField 
            label="SKU" 
            name="sku" 
            validation={{ required: 'El SKU es requerido' }} 
            error={errors.sku?.message} 
        />
        
        <InputField 
            label="Código Único" 
            name="internalCode" 
            validation={{ required: 'El código es requerido' }} 
            error={errors.internalCode?.message} 
        />

        <InputField 
            label="Nombre" 
            name="name" 
            validation={{ required: 'El nombre es requerido' }} 
            error={errors.name?.message} 
        />

        {/* CAMBIO: He reemplazado el input manual por tu componente InputField.
            Ahora tiene la validación 'required'.
        */}
        <InputField 
            label="Descripción" 
            name="description" 
            validation={{ required: 'La descripción es requerida' }} 
            error={errors.description?.message} 
        />

        <div className="grid grid-cols-2 gap-3">
            <InputField 
                label="Precio" 
                name="currentPrice" 
                type="number"
                validation={{ 
                    required: 'El precio es requerido', // Esto evita que esté vacío
                    min: { value: 0, message: 'No puede ser negativo' }, 
                    valueAsNumber: true 
                }} 
                error={errors.currentPrice?.message} 
            />
            
            <InputField 
                label="Stock" 
                name="stockQuantity" 
                type="number"
                validation={{ 
                    required: 'El stock es requerido', // Esto evita que esté vacío
                    min: { value: 0, message: 'No puede ser negativo' }, 
                    valueAsNumber: true 
                }} 
                error={errors.stockQuantity?.message} 
            />
        </div>

        <div className='flex justify-end mt-2'>
          <button 
            type='submit' 
            className='bg-purple-200 text-purple-800 font-bold px-6 py-2 rounded-lg text-sm hover:bg-purple-300 transition-colors shadow-sm'
          >
            Crear Producto
          </button>
        </div>
        
        {errorBackendMessage && <span className='text-red-500 text-sm text-center mt-2 font-medium bg-red-50 p-2 rounded'>{errorBackendMessage}</span>}
      </form>
    </Card>
  );
};

export default CreateProductForm;
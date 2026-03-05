import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router-dom';
import { registerUser } from '../services/register';

function RegisterPage() {
  const [successMessage, setSuccessMessage] = useState('');
  const navigate = useNavigate();

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm();

  const onValid = async (formData) => {
    setSuccessMessage('');
    
    try {
      const { error } = await registerUser(formData);

      if (error) {
        console.log("Error completo del backend:", error.response);

        let msg = 'Error al registrarse.';
        const data = error.response?.data;

        if (data) {
          if (typeof data === 'string') {
            msg = data;
          } else if (data.errors) {
            const firstKey = Object.keys(data.errors)[0];
            msg = data.errors[firstKey][0];
          } else if (data.message) {
            msg = data.message;
          } else if (data.detail) {
            msg = data.detail;
          }
        }

        alert(msg);
        return;
      }

      // ⭐ REDIRECCIÓN SEGÚN ROL
      const role = formData.role;

      setSuccessMessage('¡Usuario creado con éxito! Redirigiendo...');

      setTimeout(() => {
        if (role === 'Admin') {
          navigate('/admin');      // ⬅️ ruta del panel Admin
        } else {
          navigate('/');           // ⬅️ ruta del catálogo / productos
        }
      }, 1500);

    } catch (error) {
      alert('Error de conexión con el servidor.');
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 p-4">
      <form 
        className='w-[90%] md:w-[450px] bg-white p-8 rounded-2xl shadow-lg border border-gray-100 flex flex-col gap-4'
        onSubmit={handleSubmit(onValid)}
      >
        <h2 className="text-2xl font-bold text-gray-700 text-center mb-2">Crear Cuenta</h2>

        {/* INPUTS */}
        <div className="flex flex-col gap-1">
            <label className="text-gray-700 font-medium ml-1">Usuario</label>
            <input { ...register('username', { required: 'Requerido' }) } className="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-purple-200" />
            {errors.username && <span className="text-red-400 text-xs ml-1">Requerido</span>}
        </div>

        <div className="flex flex-col gap-1">
            <label className="text-gray-700 font-medium ml-1">Email</label>
            <input type="email" { ...register('email', { required: 'Requerido', pattern: { value: /^\S+@\S+$/i, message: "Email inválido" } }) } className="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-purple-200" />
            {errors.email && <span className="text-red-400 text-xs ml-1">{errors.email.message}</span>}
        </div>

        <div className="flex flex-col gap-1">
            <label className="text-gray-700 font-medium ml-1">Teléfono</label>
            <input type="tel" { ...register('phoneNumber', { required: 'Requerido' }) } className="w-full border border-gray-300 rounded-lg px-4 px-2 py-2 focus:ring-2 focus:ring-purple-200" />
            {errors.phoneNumber && <span className="text-red-400 text-xs ml-1">Requerido</span>}
        </div>

        <div className="flex flex-col gap-1">
            <label className="text-gray-700 font-medium ml-1">Rol</label>
            <select { ...register('role', { required: 'Requerido' }) } className="w-full border border-gray-300 rounded-lg px-4 py-2 bg-white focus:ring-2 focus:ring-purple-200">
                <option value="">Seleccione una opción</option>
                <option value="Customer">Cliente</option>
                <option value="Admin">Administrador</option>
            </select>
            {errors.role && <span className="text-red-400 text-xs ml-1">Requerido</span>}
        </div>

        <div className="flex flex-col gap-1">
            <label className="text-gray-700 font-medium ml-1">Contraseña</label>
            <input type="password" { ...register('password', { required: 'Requerido', minLength: { value: 6, message: "Mínimo 6" } }) } className="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-purple-200" />
            {errors.password && <span className="text-red-400 text-xs ml-1">{errors.password.message}</span>}
        </div>

        <div className="flex flex-col gap-1">
            <label className="text-gray-700 font-medium ml-1">Confirmar Contraseña</label>
            <input 
              type="password"
              { 
                ...register('confirmPassword', { 
                  required: 'Requerido', 
                  validate: (val) => watch('password') != val ? "No coinciden" : true 
                }) 
              } 
              className="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-purple-200" 
            />
            {errors.confirmPassword && <span className="text-red-400 text-xs ml-1">{errors.confirmPassword.message}</span>}
        </div>

        {successMessage && (
            <div className='bg-green-100 border-l-4 border-green-500 text-green-700 p-4 rounded text-center font-bold'>
                {successMessage}
            </div>
        )}

        <div className="flex flex-col gap-3 mt-4">
            <button type='submit' className="w-full bg-purple-100 text-purple-800 hover:bg-purple-200 font-bold py-3 rounded-xl transition-colors">
                Registrar Usuario
            </button>
            <button type="button" onClick={() => navigate('/login')} className="w-full bg-gray-200 text-gray-700 hover:bg-gray-300 font-bold py-3 rounded-xl transition-colors">
                Volver al Login
            </button>
        </div>
      </form>
    </div>
  );
}

export default RegisterPage;

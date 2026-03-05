import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router-dom';
import Input from '../../shared/components/Input';
import Button from '../../shared/components/Button';
import useAuth from '../hook/useAuth';
import { frontendErrorMessage } from '../helpers/backendError';

function LoginForm() {
    const [errorMessage, setErrorMessage] = useState('');
    const {
        register,
        handleSubmit,
        formState: { errors },
    } = useForm({ defaultValues: { username: '', password: '' } });

    const navigate = useNavigate();
    const { singin } = useAuth();

    const onValid = async (formData) => {
        try {
            const { data, error } = await singin(formData.username, formData.password);

            if (error) {
                const msg =
                    error.frontendErrorMessage ||
                    frontendErrorMessage[error.response?.data?.code] ||
                    'Error de credenciales';

                setErrorMessage(msg);
                return;
            }

            console.log("Respuesta Completa del Backend:", data);

            const role = data?.role?.toLowerCase();

            if (role === "admin") {
                navigate("/admin/home");
            } else {
                navigate("/");
            }

        } catch (error) {
            setErrorMessage('Ocurrió un error inesperado');
        }
    };

    return (
        <div className="min-h-screen flex items-center justify-center bg-gray-50 p-4">
            <form 
                className='
                    w-[90%] md:w-[450px]
                    bg-white 
                    p-8 md:p-10
                    rounded-2xl
                    shadow-lg
                    border border-gray-100
                    flex flex-col gap-6
                '
                onSubmit={handleSubmit(onValid)}
            >
                <div className="flex flex-col gap-5">
                    <div className="flex flex-col gap-1">
                        <label className="text-gray-700 font-medium ml-1">Usuario</label>
                        <Input
                            {...register('username', { required: 'Requerido' })}
                            className="w-full border border-gray-300 rounded-lg px-4 py-3 focus:outline-none focus:ring-2 focus:ring-purple-200 transition-all"
                        />
                        {errors.username && <span className="text-red-400 text-xs ml-1 font-medium">Error</span>}
                    </div>
                    
                    <div className="flex flex-col gap-1">
                        <label className="text-gray-700 font-medium ml-1">Password</label>
                        <Input
                            type="password"
                            {...register('password', { required: 'Requerido' })}
                            className="w-full border border-gray-300 rounded-lg px-4 py-3 focus:outline-none focus:ring-2 focus:ring-purple-200 transition-all"
                        />
                        {errors.password && <span className="text-red-400 text-xs ml-1 font-medium">Error</span>}
                    </div>
                </div>

                {errorMessage && (
                    <div className='bg-red-50 text-red-500 text-sm p-3 rounded-lg text-center'>
                        {errorMessage}
                    </div>
                )}

                <div className="flex flex-col gap-3 mt-2">
                    <Button 
                        type='submit' 
                        className="w-full bg-purple-100 text-purple-800 hover:bg-purple-200 font-bold py-3 rounded-xl transition-colors shadow-sm"
                    >
                        Iniciar Sesión
                    </Button>
                    
                    <Button 
                        type="button" 
                        onClick={() => navigate('/signup')} 
                        className="w-full bg-gray-200 text-gray-700 hover:bg-gray-300 font-bold py-3 rounded-xl transition-colors shadow-sm"
                    >
                        Registrar Usuario
                    </Button>
                </div>
            </form>
        </div>
    );
};

export default LoginForm;

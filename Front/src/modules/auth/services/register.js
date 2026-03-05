import { instance } from '../../shared/api/axiosInstance';

export const registerUser = async (formData) => {
  try {
    const endpoint = formData.role === 'Admin' 
        ? '/auth/registerAdmin' 
        : '/auth/registerCustomer';

    const response = await instance.post(endpoint, {
        username: formData.username,
        email: formData.email,
        password: formData.password,
        phoneNumber: formData.phoneNumber 
    });

    return { data: response.data, error: null };
  } catch (error) {
    return { data: null, error: error };
  }
};
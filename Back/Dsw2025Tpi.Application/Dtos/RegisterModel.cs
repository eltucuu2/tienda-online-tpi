using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos
{
    // Modelo DTO para el registro de un nuevo usuario
    public record RegisterModel(
        string Username, // Nombre de usuario que se registrará
        string Password, // Contraseña del usuario
        string Email,     // Correo electrónico del usuario
        string PhoneNumber // Número de teléfono del usuario
    );
}

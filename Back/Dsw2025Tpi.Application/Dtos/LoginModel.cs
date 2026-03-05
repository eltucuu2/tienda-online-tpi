using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos
{
    // Este registro representa el modelo de datos que se espera recibir al hacer login.
    // Se utiliza generalmente como DTO (Data Transfer Object) en el endpoint de autenticación.
    public record LoginModel(string Username, string Password);

    // El tipo "record" es inmutable por defecto y útil para modelos de datos simples.
    // Proporciona automáticamente constructor, igualdad estructural y desestructuración.
}


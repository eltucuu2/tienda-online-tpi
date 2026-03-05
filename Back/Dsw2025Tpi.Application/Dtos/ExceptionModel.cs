using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos
{
    public record ExceptionModel
    {
       public record Response(
            int StatusCode,         // Código de estado HTTP (ej. 404, 500)
            string Message        // Mensaje de error descriptivo
       ); 
    }
}

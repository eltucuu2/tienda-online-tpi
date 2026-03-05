using System.Text.Json;
using Dsw2025Tpi.Data;

namespace Dsw2025Tpi.Data.Helpers
{
    public static class DbContextExtensions
    {
        // Método de extensión para poblar (seed) la base de datos con datos de un archivo JSON
        public static void Seedwork<T>(this Dsw2025TpiContext context, string dataSource) where T : class
        {
            // Si la tabla ya tiene datos, no hace nada para evitar duplicados
            if (context.Set<T>().Any()) return;

            // Lee el contenido del archivo JSON con los datos a insertar, considerando la ruta base de la aplicación
            var json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, dataSource));

            // Deserializa el JSON a una lista de entidades del tipo T, ignorando mayúsculas/minúsculas en propiedades
            var entities = JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });

            // Si no hay datos en el JSON, sale sin hacer nada
            if (entities == null || entities.Count == 0) return;

            // Agrega los registros deserializados al contexto para insertarlos en la base de datos
            context.Set<T>().AddRange(entities);

            // Guarda los cambios para persistir los datos en la base
            context.SaveChanges();
        }
    }
}

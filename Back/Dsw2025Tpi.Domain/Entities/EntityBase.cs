namespace Dsw2025Tpi.Domain.Entities
{
    // Clase base abstracta para todas las entidades del dominio
    public abstract class EntityBase
    {
        // Identificador único global (GUID) para cada entidad
        public Guid Id { get; set; }

        // Constructor protegido para evitar instanciar directamente y generar un nuevo Id
        protected EntityBase()
        {
            // Al crear la entidad, asigna automáticamente un nuevo GUID único
            Id = Guid.NewGuid();
        }
    }
}

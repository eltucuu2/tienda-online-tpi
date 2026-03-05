using Dsw2025Tpi.Domain.Entities;
using System.Linq.Expressions;

namespace Dsw2025Tpi.Domain.Interfaces
{
    // Interfaz genérica para el repositorio de entidades que heredan de EntityBase
    public interface IRepository
    {
        /// <summary>
        /// Obtiene una entidad por su Id, incluyendo las relaciones indicadas.
        /// </summary>
        /// <typeparam name="T">Tipo de entidad.</typeparam>
        /// <param name="id">Identificador único de la entidad.</param>
        /// <param name="include">Relaciones a incluir en la consulta.</param>
        /// <returns>Entidad encontrada o null si no existe.</returns>
        Task<T?> GetById<T>(Guid id, params string[] include) where T : EntityBase;

        /// <summary>
        /// Obtiene todas las entidades del tipo especificado, con las relaciones indicadas.
        /// </summary>
        /// <typeparam name="T">Tipo de entidad.</typeparam>
        /// <param name="include">Relaciones a incluir.</param>
        /// <returns>Lista de entidades o null si no hay.</returns>
        Task<IEnumerable<T>?> GetAll<T>(params string[] include) where T : EntityBase;

        /// <summary>
        /// Obtiene la primera entidad que cumple con un predicado dado, incluyendo relaciones.
        /// </summary>
        /// <typeparam name="T">Tipo de entidad.</typeparam>
        /// <param name="predicate">Condición para filtrar.</param>
        /// <param name="include">Relaciones a incluir.</param>
        /// <returns>Primera entidad que cumple o null.</returns>
        Task<T?> First<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : EntityBase;

        /// <summary>
        /// Obtiene todas las entidades que cumplen con un filtro, incluyendo relaciones.
        /// </summary>
        /// <typeparam name="T">Tipo de entidad.</typeparam>
        /// <param name="predicate">Condición para filtrar.</param>
        /// <param name="include">Relaciones a incluir.</param>
        /// <returns>Lista filtrada de entidades o null.</returns>
        Task<IEnumerable<T>?> GetFiltered<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : EntityBase;

        /// <summary>
        /// Agrega una nueva entidad al repositorio y guarda los cambios.
        /// </summary>
        /// <typeparam name="T">Tipo de entidad.</typeparam>
        /// <param name="entity">Entidad a agregar.</param>
        /// <returns>Entidad agregada con sus propiedades actualizadas.</returns>
        Task<T> Add<T>(T entity) where T : EntityBase;

        /// <summary>
        /// Actualiza una entidad existente y guarda los cambios.
        /// </summary>
        /// <typeparam name="T">Tipo de entidad.</typeparam>
        /// <param name="entity">Entidad a actualizar.</param>
        /// <returns>Entidad actualizada.</returns>
        Task<T> Update<T>(T entity) where T : EntityBase;

        /// <summary>
        /// Elimina una entidad del repositorio y guarda los cambios.
        /// </summary>
        /// <typeparam name="T">Tipo de entidad.</typeparam>
        /// <param name="entity">Entidad a eliminar.</param>
        /// <returns>Entidad eliminada.</returns>
        Task<T> Delete<T>(T entity) where T : EntityBase;
    }
}


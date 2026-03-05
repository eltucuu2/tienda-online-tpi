using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Dsw2025Tpi.Data.Repositories;

// Implementación genérica del repositorio usando Entity Framework Core
public class EfRepository : IRepository
{
    private readonly Dsw2025TpiContext _context;  // Contexto de EF para la base de datos

    // Inyección del contexto a través del constructor
    public EfRepository(Dsw2025TpiContext context)
    {
        _context = context;
    }

    // Agrega una entidad y guarda los cambios de forma asíncrona
    public async Task<T> Add<T>(T entity) where T : EntityBase
    {
        await _context.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    // Elimina una entidad y guarda los cambios de forma asíncrona
    public async Task<T> Delete<T>(T entity) where T : EntityBase
    {
        _context.Remove(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    // Obtiene el primer elemento que cumpla con un filtro, incluyendo relaciones si se especifican
    public async Task<T?> First<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : EntityBase
    {
        return await Include(_context.Set<T>(), include).FirstOrDefaultAsync(predicate);
    }

    // Obtiene todos los elementos de un tipo, incluyendo relaciones si se especifican
    public async Task<IEnumerable<T>?> GetAll<T>(params string[] include) where T : EntityBase
    {
        return await Include(_context.Set<T>(), include).ToListAsync();
    }

    // Obtiene una entidad por su Id, incluyendo relaciones si se especifican
    public async Task<T?> GetById<T>(Guid id, params string[] include) where T : EntityBase
    {
        return await Include(_context.Set<T>(), include).FirstOrDefaultAsync(e => e.Id == id);
    }

    // Obtiene una lista filtrada por un predicado, incluyendo relaciones si se especifican
    public async Task<IEnumerable<T>?> GetFiltered<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : EntityBase
    {
        return await Include(_context.Set<T>(), include).Where(predicate).ToListAsync();
    }

    // Actualiza una entidad y guarda los cambios de forma asíncrona
    public async Task<T> Update<T>(T entity) where T : EntityBase
    {
        _context.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    // Método privado para agregar los Include necesarios a la consulta, para cargar relaciones
    private static IQueryable<T> Include<T>(IQueryable<T> query, string[] includes) where T : EntityBase
    {
        var includedQuery = query;

        foreach (var include in includes)
        {
            includedQuery = includedQuery.Include(include); // Se van agregando los Include uno a uno
        }
        return includedQuery;
    }
}

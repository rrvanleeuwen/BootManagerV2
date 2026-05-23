using BootManager.Core.Interfaces;
using BootManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BootManager.Infrastructure.Repositories;

public class EfRepository<T> : IRepository<T> where T : class
{
    private readonly BootManagerDbContext _db;
    private readonly DbSet<T> _set;

    public EfRepository(BootManagerDbContext db)
    {
        _db = db;
        _set = db.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _set.FindAsync([id], ct);

    public async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
    {
        IQueryable<T> q = _set;
        if (predicate != null) q = q.Where(predicate);
        return await q.AsNoTracking().SingleOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
    {
        IQueryable<T> q = _set;
        if (predicate != null) q = q.Where(predicate);
        return await q.AsNoTracking().ToListAsync(ct);
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
    {
        if (predicate == null) return await _set.AnyAsync(ct);
        return await _set.AnyAsync(predicate, ct);
    }

    public async Task AddAsync(T entity, CancellationToken ct = default)
    {
        await _set.AddAsync(entity, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        // Detach een eventueel al getrackte instantie met dezelfde primary key
        // om EF tracking conflicts te voorkomen (bijv. na AddAsync in dezelfde scoped DbContext).
        var keyValues = _db.Model.FindEntityType(typeof(T))!
            .FindPrimaryKey()!
            .Properties
            .Select(p => p.PropertyInfo!.GetValue(entity))
            .ToArray();

        var tracked = _set.Local.FirstOrDefault(e =>
        {
            var trackedKeys = _db.Model.FindEntityType(typeof(T))!
                .FindPrimaryKey()!
                .Properties
                .Select(p => p.PropertyInfo!.GetValue(e))
                .ToArray();
            return keyValues.SequenceEqual(trackedKeys);
        });

        if (tracked != null && !ReferenceEquals(tracked, entity))
            _db.Entry(tracked).State = EntityState.Detached;

        _set.Update(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        _set.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }
}
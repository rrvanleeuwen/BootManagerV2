using System.Linq.Expressions;

namespace BootManager.Core.Interfaces;

/// <summary>
/// Generieke repository interface voor basis CRUD-operaties op entiteiten.
/// </summary>
/// <typeparam name="T">Het entiteitstype beheerd door deze repository.</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Haalt een entiteit op bij primaire sleutel.
    /// </summary>
    /// <param name="id">De primaire sleutel van de entiteit.</param>
    /// <param name="ct">Annuleringstoken.</param>
    /// <returns>De entiteit of null als niet gevonden.</returns>
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Haalt een enkel object op dat aan het predicaat voldoet.
    /// </summary>
    /// <param name="predicate">Filterconditie; null haalt eerste record op.</param>
    /// <param name="ct">Annuleringstoken.</param>
    /// <returns>Het object of null als niet gevonden.</returns>
    Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);

    /// <summary>
    /// Haalt een lijst van objecten op die aan het predicaat voldoen.
    /// </summary>
    /// <param name="predicate">Filterconditie; null haalt alle records op.</param>
    /// <param name="ct">Annuleringstoken.</param>
    /// <returns>Lijst van objecten.</returns>
    Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);

    /// <summary>
    /// Controleert of minstens één object aan het predicaat voldoet.
    /// </summary>
    /// <param name="predicate">Filterconditie; null controleert of minstens één record bestaat.</param>
    /// <param name="ct">Annuleringstoken.</param>
    /// <returns>True als minstens één match gevonden, anders false.</returns>
    Task<bool> AnyAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);

    /// <summary>
    /// Voegt een nieuwe entiteit toe aan de repository.
    /// </summary>
    /// <param name="entity">De toe te voegen entiteit.</param>
    /// <param name="ct">Annuleringstoken.</param>
    Task AddAsync(T entity, CancellationToken ct = default);

    /// <summary>
    /// Werkt een bestaande entiteit bij in de repository.
    /// </summary>
    /// <param name="entity">De entiteit met bijgewerkte waarden.</param>
    /// <param name="ct">Annuleringstoken.</param>
    Task UpdateAsync(T entity, CancellationToken ct = default);

    /// <summary>
    /// Verwijdert een entiteit uit de repository.
    /// </summary>
    /// <param name="entity">De te verwijderen entiteit.</param>
    /// <param name="ct">Annuleringstoken.</param>
    Task DeleteAsync(T entity, CancellationToken ct = default);
}
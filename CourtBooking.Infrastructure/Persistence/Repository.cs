using CourtBooking.Application.Common.Interfaces;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace CourtBooking.Infrastructure.Persistence;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
        => await _context.Set<T>().FindAsync(new[] { id }, cancellationToken);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Set<T>().ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await _context.Set<T>().Where(predicate).ToListAsync(cancellationToken);

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await _context.Set<T>().FirstOrDefaultAsync(predicate, cancellationToken);

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await _context.Set<T>().AnyAsync(predicate, cancellationToken);

    public void Add(T entity) => _context.Set<T>().Add(entity);
    public void Update(T entity) => _context.Set<T>().Update(entity);
    public void Remove(T entity) => _context.Set<T>().Remove(entity);
}

using Microsoft.EntityFrameworkCore;

namespace DatabaseService.DataAccess.Abstractions;

public interface IDbContextWithSets
{
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

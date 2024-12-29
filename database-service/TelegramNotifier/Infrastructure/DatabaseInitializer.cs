using DatabaseService.DataAccess;
using Extensions.Hosting.AsyncInitialization;
using Microsoft.EntityFrameworkCore;

namespace DatabaseService.Infrastructure;

internal sealed class DatabaseInitializer : IAsyncInitializer
{
    private readonly AppDbContext appDbContext;

    /// <summary>
    /// Database initializer. Performs migration and data seed.
    /// </summary>
    /// <param name="appDbContext">Data context.</param>
    public DatabaseInitializer(AppDbContext appDbContext)
    {
        this.appDbContext = appDbContext;
    }

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await appDbContext.Database.MigrateAsync(cancellationToken);
    }
}
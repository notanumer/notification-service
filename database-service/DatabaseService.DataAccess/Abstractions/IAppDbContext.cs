using DatabaseService.Models.Postgres;
using Microsoft.EntityFrameworkCore;

namespace DatabaseService.DataAccess.Abstractions;

public interface IAppDbContext : IDbContextWithSets, IDisposable
{
    DbSet<Notification> Notifications { get; }
    
    DbSet<Channel> Channels { get; }
}
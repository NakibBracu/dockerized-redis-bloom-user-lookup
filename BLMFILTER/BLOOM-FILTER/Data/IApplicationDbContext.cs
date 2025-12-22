using BLOOM_FILTER.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace BLOOM_FILTER.Data
{
    public interface IApplicationDbContext
    {
        DbSet<User> Users { get; set; }
        ChangeTracker ChangeTracker { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}

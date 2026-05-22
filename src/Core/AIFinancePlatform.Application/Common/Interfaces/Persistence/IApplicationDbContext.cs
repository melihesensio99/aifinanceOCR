using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AIFinancePlatform.Domain.Entities;

namespace AIFinancePlatform.Application.Common.Interfaces.Persistence;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<Category> Categories { get; }
    DbSet<Budget> Budgets { get; }
    DbSet<Subscription> Subscriptions { get; }
    DbSet<Receipt> Receipts { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

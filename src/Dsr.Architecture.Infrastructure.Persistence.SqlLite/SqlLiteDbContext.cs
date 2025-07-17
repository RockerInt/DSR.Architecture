
using Microsoft.EntityFrameworkCore;

namespace Dsr.Architecture.Infrastructure.Persistence.SqlLite;

/// <summary>
/// Represents the database context for SQLite, responsible for managing entity configurations and database sessions.
/// </summary>
public class SqlLiteDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlLiteDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
    public SqlLiteDbContext(DbContextOptions<SqlLiteDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Applies entity configurations from the containing assembly.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SqlLiteDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

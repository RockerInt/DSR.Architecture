
using Microsoft.EntityFrameworkCore;

namespace Dsr.Architecture.Infrastructure.Persistence.SqlServer;

/// <summary>
/// Represents the database context for SQL Server, responsible for managing entity configurations and database sessions.
/// </summary>
public class SqlServerDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
    public SqlServerDbContext(DbContextOptions<SqlServerDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Applies entity configurations from the containing assembly.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SqlServerDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}


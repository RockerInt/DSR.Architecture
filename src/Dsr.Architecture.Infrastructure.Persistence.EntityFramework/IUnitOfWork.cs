using Dsr.Architecture.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework;

/// <summary>
/// Defines a unit of work interface for managing database transactions and operations.
/// This interface provides methods for completing transactions and accessing the underlying DbContext.
/// </summary>
/// <typeparam name="TContext"></typeparam>
public interface IUnitOfWork<TContext> : IUnitOfWork
    where TContext : DbContext
{
    /// <summary>
    /// Gets the DbContext associated with this Unit of Work.
    /// This property provides access to the underlying database context for performing operations.
    /// </summary>
    TContext Context { get; }
}
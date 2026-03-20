using Dsr.Architecture.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework;

/// <summary>
/// Defines a unit of work interface for managing database transactions and operations across multiple Entity Framework contexts.
/// This interface provides methods for completing transactions and accessing the underlying DbContext accessor.
/// </summary>
public interface ITransactionalEFUnitOfWork : ITransactionalUnitOfWork
{
    /// <summary>
    /// Gets the DbContext accessor associated with this Unit of Work.
    /// This property provides access to the underlying database contexts for performing operations.
    /// </summary>
    IDbContextAccessor Accessor { get; }
}
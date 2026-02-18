using Dsr.Architecture.Domain.Validation;
using Dsr.Architecture.Domain.Entities;
using Dsr.Architecture.Domain.Events;

namespace Dsr.Architecture.Domain.Aggregates;

/// <summary>
/// Represents the root of an aggregate in the domain model. 
/// An aggregate is a cluster of related entities that are treated as a single unit for data changes.
/// The aggregate root is responsible for maintaining the integrity of the aggregate and enforcing business rules when changes are made. 
/// It also tracks domain events that occur within the aggregate, allowing for event sourcing and integration with other parts of the system.
/// The aggregate root includes a version property for optimistic concurrency control and a validation mechanism to ensure that 
/// the state of the aggregate is consistent and valid according to the business rules.
/// </summary>
/// <param name="id"></param>
public abstract class AggregateRoot<TId>(TId id) : Entity<TId>(id), IAggregateRoot<TId>
    where TId : IEquatable<TId>, IComparable<TId>
{
    /// <summary>
    /// A list to store domain events that have been raised by the aggregate. 
    /// These events represent significant changes to the state of the aggregate and can be used for event sourcing, 
    /// auditing, and integration with other parts of the system. 
    /// The collection is read-only from the outside, and events can only be added through the AddDomainEvent method, 
    /// ensuring that the integrity of the event collection is maintained.
    /// </summary>
    private readonly List<IDomainEvent> _domainEvents = [];
    /// <summary>
    /// Gets the version of the aggregate, which is used for optimistic concurrency control to ensure that updates to 
    /// the aggregate are based on the most recent state. 
    /// The version is typically incremented each time a change is made to the aggregate, allowing the system to detect and handle concurrent modifications appropriately. 
    /// This helps to prevent conflicts and maintain the integrity of the aggregate when multiple users or processes are making changes simultaneously.
    /// </summary>
    public int Version { get; protected set; }
    /// <summary>
    /// Gets a read-only collection of domain events that have been raised by the aggregate. 
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    /// <summary>
    /// Provides a validation mechanism for the aggregate, allowing it to collect validation errors in a fluent manner. 
    /// The ValidationCollector can be used to perform various validations on the state of the aggregate, such as null checks, empty GUIDs, numeric ranges, and business rules. 
    /// The collected errors can then be converted into a Result object for standardized error handling across the application, ensuring that the aggregate maintains a consistent and valid state according to the defined business rules. 
    /// This helps to ensure that any changes made to the aggregate are valid and that any issues are properly reported and handled. 
    /// The Validator property is initialized with a new instance of ValidationCollector, allowing it to be used immediately for validation purposes within the aggregate's methods. 
    /// </summary>
    public ValidationCollector Validator { get; private set; } = new();
    /// <summary>
    /// Performs validation on the aggregate using the ValidationCollector and returns a Result object representing the outcome of the validation. 
    /// If there are any validation errors collected in the Validator, the Result will indicate failure and include the details of the errors. 
    /// If there are no validation errors, the Result will indicate success, allowing the calling code to proceed with confidence that the aggregate is in a valid state according to the defined business rules. 
    /// This method can be called before making changes to the aggregate or before saving it to ensure that any issues are caught and handled appropriately, maintaining the integrity of the aggregate and the overall system. 
    /// The use of a Result object allows for standardized error handling across the application, making it easier to manage and respond to validation issues in a consistent manner.
    /// </summary>
    /// <returns></returns>
    public Result.Result Validate() => Validator.ToResult();
    /// <summary>
    /// Adds a domain event to the aggregate's collection of domain events. 
    /// This method is used to record significant changes to the state of the aggregate, allowing for event sourcing, auditing, and integration with other parts of the system. 
    /// When a domain event is added, it is stored in the _domainEvents list, which can be accessed through the DomainEvents property. 
    /// After the events have been processed (e.g., published to an event bus or stored in an event store), the ClearDomainEvents method can be called to clear the collection of events, ensuring that the aggregate does not retain events that have already been handled. 
    /// This method helps to maintain the integrity of the event collection and ensures that all significant changes to the aggregate are properly recorded and can be acted upon by other parts of the system as needed.
    /// </summary>
    /// <param name="domainEvent"></param>
    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    /// <summary>
    /// Clears the collection of domain events after they have been processed. 
    /// This is typically called after the events have been published to an event bus or stored in an event store, to ensure that the aggregate does not retain events that have already been handled.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
    /// <summary>
    /// Increments the version of the aggregate, which is used for optimistic concurrency control to ensure that updates to 
    /// the aggregate are based on the most recent state. 
    /// This method should be called whenever a change is made to the aggregate that should be tracked for concurrency purposes, allowing the system to detect and handle concurrent modifications appropriately. 
    /// By incrementing the version each time a change is made, the aggregate can help to prevent conflicts and maintain the integrity of the aggregate when multiple users or processes are making changes simultaneously. 
    /// This method is protected, allowing it to be called from within the aggregate's methods when changes are made, while preventing external code from modifying the version directly, ensuring that the integrity of the versioning mechanism is maintained.
    /// </summary>
    protected void IncrementVersion() => Version++;
}
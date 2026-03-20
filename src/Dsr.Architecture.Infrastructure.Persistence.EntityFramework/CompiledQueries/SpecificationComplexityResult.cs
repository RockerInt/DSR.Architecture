namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.CompiledQueries;

/// <summary>
/// Represents the result of analyzing the complexity of a specification. 
/// It contains a score that indicates the complexity level, a boolean that indicates whether the specification should be executed using a compiled query, and a reason that explains the decision.
/// </summary>
public sealed class SpecificationComplexityResult
{
    /// <summary>
    /// Gets the complexity score of the specification. 
    /// The score is calculated based on various factors such as the presence of criteria, includes, ordering, pagination, and whether it uses split queries.
    /// </summary>
    public int Score { get; init; }
    /// <summary>
    /// Gets a boolean that indicates whether the specification should be executed using a compiled query. 
    /// This is determined based on the complexity score, where specifications with a score below or equal to a certain threshold are considered suitable for compiled queries, while those with a higher score are not.
    /// </summary>
    public bool ShouldUseCompiledQuery { get; init; }
    /// <summary>
    /// Gets a reason that explains the decision on whether to use a compiled query or not. 
    /// This reason provides insight into the complexity of the specification, such as whether it is a simple specification or a highly complex one with multiple includes, ordering, pagination, or split query.
    /// </summary>
    public string Reason { get; init; } = "";
}
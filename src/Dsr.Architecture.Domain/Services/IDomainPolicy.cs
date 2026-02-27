namespace Dsr.Architecture.Domain.Services;

/// <summary>
/// Defines a domain policy, which is a specific type of domain service that encapsulates business rules or policies that can be evaluated based on input data.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TResult"></typeparam>
public interface IDomainPolicy<in TInput, TResult>
{
    /// <summary>
    /// Evaluates the policy based on the provided input and returns a result indicating whether the policy is satisfied or not, along with any relevant information.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    TResult Evaluate(TInput input);
}
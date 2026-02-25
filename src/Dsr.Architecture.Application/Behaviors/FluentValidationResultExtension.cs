using Dsr.Architecture.Domain.Result;
using Dsr.Architecture.Domain.Validation;
using FluentValidation;
using FluentValidation.Results;

namespace Dsr.Architecture.Application.Behaviors
{
    public static class FluentValidationResultExtensions
    {
        public static List<Error> AsErrors(this ValidationResult valResult)
        {
            var resultErrors = new List<Error>();

            foreach (var valFailure in valResult.Errors)
            {
                resultErrors.Add(new()
                {
                    Severity = FromSeverity(valFailure.Severity),
                    Message = valFailure.ErrorMessage,
                    Code = valFailure.ErrorCode,
                    Identifier = valFailure.PropertyName
                });
            }

            return resultErrors;
        }

        public static ErrorSeverity FromSeverity(Severity severity)
            => severity switch
                {
                    Severity.Error => ErrorSeverity.Error,
                    Severity.Warning => ErrorSeverity.Warning,
                    Severity.Info => ErrorSeverity.Info,
                    _ => throw new ArgumentOutOfRangeException(nameof(severity), "Unexpected Severity"),
                };
    }
}
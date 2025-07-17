using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsr.Architecture.Application.Exceptions;

/// <summary>
/// Exception class representing a validation error.
/// </summary>
/// <remarks>
/// Constructor for the ValidationException class.
/// </remarks>
/// <param name="errors">The list of validation errors.</param>
internal class ValidationException(List<ValidationError> errors) : Exception
{
    /// <summary>
    /// List of validation errors.
    /// </summary>
    public List<ValidationError> Errors { get; } = errors;
}

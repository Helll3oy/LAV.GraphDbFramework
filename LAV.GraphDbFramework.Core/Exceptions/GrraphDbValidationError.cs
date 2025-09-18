using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Exceptions;

public class GraphDbValidationError
{
	public string PropertyName { get; }
	public string ErrorMessage { get; }
	public object? AttemptedValue { get; }
	public string? ErrorCode { get; }

	public GraphDbValidationError(string propertyName, string errorMessage, object? attemptedValue = null, string? errorCode = null)
	{
		PropertyName = propertyName;
		ErrorMessage = errorMessage;
		AttemptedValue = attemptedValue;
		ErrorCode = errorCode;
	}
}
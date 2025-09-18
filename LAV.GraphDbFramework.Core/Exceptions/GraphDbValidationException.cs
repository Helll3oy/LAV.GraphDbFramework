using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Exceptions;

public class GraphDbValidationException : GraphDbException
{
	public IReadOnlyList<GraphDbValidationError> ValidationErrors { get; }

	public GraphDbValidationException(IReadOnlyList<GraphDbValidationError> validationErrors)
		: base("Validation failed", "VALIDATION_ERROR")
	{
		ValidationErrors = validationErrors;
	}
}

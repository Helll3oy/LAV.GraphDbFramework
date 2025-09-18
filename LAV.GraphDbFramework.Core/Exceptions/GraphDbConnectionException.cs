using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Exceptions;

public class GraphDbConnectionException : GraphDbException
{
	public GraphDbConnectionException(string operation, Exception innerException)
		: base("Database connection failed", "CONNECTION_ERROR", operation, innerException: innerException)
	{
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Exceptions;

public class GraphDbQueryException : GraphDbException
{
	public GraphDbQueryException(string query, object parameters, Exception innerException)
		: base("Query execution failed", "QUERY_EXECUTION_ERROR", "ExecuteQuery", query, parameters, innerException)
	{
	}
}
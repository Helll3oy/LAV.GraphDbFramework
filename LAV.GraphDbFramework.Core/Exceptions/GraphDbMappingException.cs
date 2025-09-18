using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Exceptions;

public class GraphDbMappingException : GraphDbException
{
	public GraphDbMappingException(string message, Type sourceType, Type targetType, Exception innerException = null)
		: base(message, "MAPPING_ERROR", "MapData", innerException: innerException)
	{
		Data["SourceType"] = sourceType;
		Data["TargetType"] = targetType;
	}
}
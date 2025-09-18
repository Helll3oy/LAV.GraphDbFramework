using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Exceptions;

public sealed class GraphDbMappingException : GraphDbException
{
    public GraphDbMappingException(string message, Type sourceType, Type targetType, Exception? innerException = null)
        : base(message, "MAPPING_ERROR", "MapData", innerException: innerException)
    {
        Data["SourceType"] = sourceType;
        Data["TargetType"] = targetType;
    }

    private GraphDbMappingException(string message, string errorCode, string? operation = null, string? query = null,
            object? parameters = null, Exception? innerException = null)
        : base(message, errorCode, operation, query, parameters, innerException)
    {
    }
}
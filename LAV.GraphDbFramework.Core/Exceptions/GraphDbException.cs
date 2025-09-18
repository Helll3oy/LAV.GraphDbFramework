using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Exceptions;

public class GraphDbException : Exception
{
	public string ErrorCode { get; }
	public string DetailedMessage { get; }
	public DateTimeOffset Timestamp { get; }
	public string Operation { get; }
	public string Query { get; }
	public object Parameters { get; }

	public GraphDbException(string message, string errorCode, string operation = null,
		string query = null, object parameters = null, Exception innerException = null)
		: base(message, innerException)
	{
		ErrorCode = errorCode;
		DetailedMessage = message;
		Timestamp = DateTimeOffset.UtcNow;
		Operation = operation;
		Query = query;
		Parameters = parameters;
	}

	public override string ToString()
	{
		var sb = new StringBuilder();
		sb.AppendLine($"GraphDbException: {Message}");
		sb.AppendLine($"ErrorCode: {ErrorCode}");
		sb.AppendLine($"Timestamp: {Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}");
		if (!string.IsNullOrEmpty(Operation))
			sb.AppendLine($"Operation: {Operation}");
		if (!string.IsNullOrEmpty(Query))
			sb.AppendLine($"Query: {Query}");
		if (Parameters != null)
			sb.AppendLine($"Parameters: {JsonSerializer.Serialize(Parameters)}");
		sb.AppendLine($"StackTrace: {StackTrace}");

		if (InnerException != null)
		{
			sb.AppendLine("--- Inner Exception ---");
			sb.AppendLine(InnerException.ToString());
		}

		return sb.ToString();
	}
}
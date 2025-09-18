using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Configuration;

public class GraphDbErrorHandlingOptions
{
	public bool EnableRetry { get; set; } = true;
	public int MaxRetryAttempts { get; set; } = 5;
	public int RetryInitialDelayMs { get; set; } = 100;
	public double RetryBackoffFactor { get; set; } = 2.0;
	public bool LogSensitiveData { get; set; } = false;
	public bool EnableGlobalExceptionHandler { get; set; } = true;
	public string DefaultErrorCode { get; set; } = "UNKNOWN_ERROR";
}

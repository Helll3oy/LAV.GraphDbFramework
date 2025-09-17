
namespace LAV.GraphDbFramework.Core;

public class BaseGraphDbOptions<T> : IGraphDbOptions where T : class
{
	public required string Host { get; set; }	
	public required string Username { get; set; }
	public required string Password { get; set; }

	public bool UseEncryption { get; set; } = false;
	public int MaxConnectionPoolSize { get; set; } = Environment.ProcessorCount * 2;
	public TimeSpan ConnectionAcquisitionTimeout { get; set; } = TimeSpan.FromSeconds(60);
	public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(5);

	public T? Extra { get; set; }
}
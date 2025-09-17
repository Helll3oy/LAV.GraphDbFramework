using LAV.GraphDbFramework.Core;

namespace LAV.GraphDbFramework.Client;

public class GraphDbOptions : IGraphDbOptions
{
	public GraphDbType DbType { get; set; }
	public string? Uri { get; set; }
	public string? Host { get; set; }
	public required string Username { get; set; }
	public required string Password { get; set; }
	public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(5);
	public int MaxConnectionPoolSize { get; set; } = Environment.ProcessorCount * 2;
	public int ConnectionAcquisitionTimeout { get; set; } = 60; // seconds
	public bool UseEncryption { get; set; } = false;
}
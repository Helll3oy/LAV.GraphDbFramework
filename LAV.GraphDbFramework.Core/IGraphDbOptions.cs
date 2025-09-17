
namespace LAV.GraphDbFramework.Core;

public interface IGraphDbOptions
{
	TimeSpan ConnectionAcquisitionTimeout { get; set; }
	TimeSpan ConnectionTimeout { get; set; }
	string Host { get; set; }
	int MaxConnectionPoolSize { get; set; }
	string Password { get; set; }
	bool UseEncryption { get; set; }
	string Username { get; set; }
}
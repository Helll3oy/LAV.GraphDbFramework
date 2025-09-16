using LAV.GraphDbFramework.Core.Attributes;

namespace LAV.GraphDbFramework.Examples.Models;

[GraphMap]
public class User
{
	public string? Id { get; set; }
	public string? Name { get; set; }
	public string? Email { get; set; }
	public int? Age { get; set; }
	public DateTime? CreatedAt { get; set; }
}

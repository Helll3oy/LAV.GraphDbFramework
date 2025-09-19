using LAV.GraphDbFramework.Core.Attributes;

namespace LAV.GraphDbFramework.Examples.Models;

[GraphMap]
public class User
{
	public string Id { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string? Email { get; set; }
	public int? Age { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.MinValue;
	public bool? IsActive { get; set; } = false;
}

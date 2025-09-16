using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Specifications;

public abstract class BaseSpecification : ISpecification
{
	protected readonly Dictionary<string, object> _parameters = new();

	public abstract string BuildQuery();

	public IReadOnlyDictionary<string, object> Parameters => _parameters;

	protected void AddParameter(string key, object value)
	{
		_parameters[key] = value;
	}

	protected string GenerateParameterName(string baseName)
	{
		return $"{baseName}_{_parameters.Count}";
	}
}
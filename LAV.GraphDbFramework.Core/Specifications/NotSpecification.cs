using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Specifications;

public class NotSpecification : BaseSpecification
{
	private readonly ISpecification _specification;

	public NotSpecification(ISpecification specification)
	{
		_specification = specification;

		// Копируем параметры
		foreach (var param in _specification.Parameters)
		{
			AddParameter(param.Key, param.Value);
		}
	}

	public override string BuildQuery()
	{
		return $"NOT ({_specification.BuildQuery()})";
	}
}

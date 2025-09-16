using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Specifications;

public class OrSpecification : BaseSpecification
{
	private readonly ISpecification _left;
	private readonly ISpecification _right;

	public OrSpecification(ISpecification left, ISpecification right)
	{
		_left = left;
		_right = right;

		// Объединяем параметры
		foreach (var param in _left.Parameters)
		{
			AddParameter(param.Key, param.Value);
		}

		foreach (var param in _right.Parameters)
		{
			// Убедимся, что нет конфликта имен параметров
			var key = param.Key;
			var counter = 1;
			while (_parameters.ContainsKey(key))
			{
				key = $"{param.Key}_{counter++}";
			}
			AddParameter(key, param.Value);
		}
	}

	public override string BuildQuery()
	{
		return $"({_left.BuildQuery()} OR {_right.BuildQuery()})";
	}
}
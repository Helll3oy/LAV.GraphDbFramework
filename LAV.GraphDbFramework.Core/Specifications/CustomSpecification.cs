using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Specifications;

public class CustomSpecification : BaseSpecification
{
	private readonly string _queryPattern;

	public CustomSpecification(string queryPattern, IReadOnlyDictionary<string, object>? parameters = null)
	{
		_queryPattern = queryPattern;

		if (parameters != null)
		{
			foreach (var param in parameters)
			{
				AddParameter(param.Key, param.Value);
			}
		}
	}

	public override string BuildQuery()
	{
		return _queryPattern;
	}
}

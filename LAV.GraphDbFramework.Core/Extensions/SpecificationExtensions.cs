using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LAV.GraphDbFramework.Core.Specifications;

namespace LAV.GraphDbFramework.Core.Extensions;

public static class SpecificationExtensions
{
	public static ISpecification And(this ISpecification left, ISpecification right)
	{
		return new AndSpecification(left, right);
	}

	public static ISpecification Or(this ISpecification left, ISpecification right)
	{
		return new OrSpecification(left, right);
	}

	public static ISpecification Not(this ISpecification specification)
	{
		return new NotSpecification(specification);
	}

	public static QuerySpecification<T> ToQuery<T>(this ISpecification specification)
	{
		return new QuerySpecification<T>().Match(specification);
	}
}
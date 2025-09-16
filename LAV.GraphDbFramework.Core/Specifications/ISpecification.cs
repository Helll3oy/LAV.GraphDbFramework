using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Specifications;

public interface ISpecification
{
	string BuildQuery();
	IReadOnlyDictionary<string, object> Parameters { get; }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LAV.GraphDbFramework.Core.Specifications;

namespace LAV.GraphDbFramework.Core.Repositories;

public interface ISpecificationRepository<T>
{
	ValueTask<IReadOnlyList<T>> FindAsync(ISpecification specification);
	ValueTask<IReadOnlyList<T>> FindAsync(QuerySpecification<T> querySpecification);
	ValueTask<T> FindOneAsync(ISpecification specification);
	ValueTask<T> FindOneAsync(QuerySpecification<T> querySpecification);
	ValueTask<int> CountAsync(ISpecification specification);
	ValueTask<bool> ExistsAsync(ISpecification specification);
}
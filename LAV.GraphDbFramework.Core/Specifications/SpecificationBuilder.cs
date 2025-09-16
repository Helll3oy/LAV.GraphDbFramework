using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using LAV.GraphDbFramework.Core.Pooling;

namespace LAV.GraphDbFramework.Core.Specifications;

public static partial class SpecificationBuilder
{
	private static readonly ObjectPool<Dictionary<string, object>> ParameterPool;
	private static readonly ObjectPool<StringBuilder> StringBuilderPool;

	static SpecificationBuilder()
	{
		var poolProvider = new DefaultObjectPoolProvider();
		ParameterPool = poolProvider.Create<Dictionary<string, object>>(new ParameterPoolPolicy());
		StringBuilderPool = poolProvider.Create<StringBuilder>(new StringBuilderPoolPolicy());
	}

	public static PooledSpecificationBuilder<T> Create<T>() where T : class
	{
		return new PooledSpecificationBuilder<T>();
	}

	public class PooledSpecificationBuilder<T> : IDisposable where T : class
	{
		private readonly NodeSpecification _nodeSpecification;
		private readonly Dictionary<string, object> _parameters;
		private bool _disposed;

		public PooledSpecificationBuilder()
		{
			_parameters = ParameterPool.Get();
			_nodeSpecification = new NodeSpecification();
		}

		public PooledSpecificationBuilder<T> WithLabel(string label)
		{
			_nodeSpecification.WithLabel(label);
			return this;
		}

		public PooledSpecificationBuilder<T> WithProperty(string property, object value,
			ComparisonType comparison = ComparisonType.Equals)
		{
			var paramName = $"p_{_parameters.Count}";
			_parameters[paramName] = value;
			_nodeSpecification.WithProperty(property, paramName, comparison);
			return this;
		}

		public (string Query, IReadOnlyDictionary<string, object> Parameters) Build()
		{
			var sb = StringBuilderPool.Get();
			try
			{
				sb.Append("MATCH ");
				sb.Append(_nodeSpecification.BuildQuery());
				sb.Append(" RETURN n");

				return (sb.ToString(), _parameters);
			}
			finally
			{
				StringBuilderPool.Return(sb);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) return;

			if (disposing)
			{
				// TODO: освободить управляемое состояние (управляемые объекты)
				_parameters.Clear();
				ParameterPool.Return(_parameters);
			}
	
			_disposed = true;
		}

		// // TODO: переопределить метод завершения, только если "Dispose(bool disposing)" содержит код для освобождения неуправляемых ресурсов
		// ~PooledSpecificationBuilder()
		// {
		//     // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
		//     Dispose(disposing: false);
		// }

		public void Dispose()
		{
			// Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
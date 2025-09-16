using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;

namespace LAV.GraphDbFramework.Core.Extensions;

public static class PoolingExtensions
{
	public static PooledDictionary RentDictionary(this ObjectPool<Dictionary<string, object>> pool)
	{
		return new PooledDictionary(pool.Get());
	}

	public static PooledList<T> RentList<T>(this ObjectPool<List<T>> pool)
	{
		return new PooledList<T>(pool.Get());
	}
}

public ref struct PooledDictionary
{
	private readonly ObjectPool<Dictionary<string, object>>? _pool;
	private Dictionary<string, object>? _dictionary;

	public PooledDictionary(ObjectPool<Dictionary<string, object>> pool)
	{
		_pool = pool;
		_dictionary = pool.Get();
	}

	internal PooledDictionary(Dictionary<string, object> dictionary)
	{
		_pool = null;
		_dictionary = dictionary;
	}

	public readonly Dictionary<string, object> Dictionary => _dictionary!;

	public void Dispose()
	{
		if (_pool != null && _dictionary != null)
		{
			_pool.Return(_dictionary);
			_dictionary = null;
		}
	}
}

public ref struct PooledList<T>
{
	private readonly ObjectPool<List<T>>? _pool;
	private List<T>? _list;

	public PooledList(ObjectPool<List<T>> pool)
	{
		_pool = pool;
		_list = pool.Get();
	}

	internal PooledList(List<T> list)
	{
		_pool = null;
		_list = list;
	}

	public readonly List<T> List => _list!;

	public void Dispose()
	{
		if (_pool != null && _list != null)
		{
			_pool.Return(_list);
			_list = null;
		}
	}
}
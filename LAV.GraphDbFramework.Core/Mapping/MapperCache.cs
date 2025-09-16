using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Microsoft.Extensions.ObjectPool;
using LAV.GraphDbFramework.Core.Extensions;
//using Microsoft.VisualStudio.Utilities;

namespace LAV.GraphDbFramework.Core.Mapping;

public static class MapperCache<T>
{
    private static readonly FrozenDictionary<string, Action<T, IRecord>> PropertySetters;
    private static readonly Func<IRecord, T> MapperFunc;
    //private static readonly ObjectPool<T> ObjectPool;// = new DefaultObjectPool<T>(new DefaultPooledObjectPolicy(), 1024);

	static MapperCache()
    {
		var type = typeof(T);

		// Пытаемся найти сгенерированный маппер
		var generatedMapperType = type.Assembly.GetType($"{type.Namespace}.{type.Name}Mapper");
		if (generatedMapperType != null)
		{
			var mapMethod = generatedMapperType.GetMethod("MapFromRecord",
				BindingFlags.Public | BindingFlags.Static,
				null,
				new[] { typeof(IRecord) },
				null);

			if (mapMethod != null)
			{
				// Используем сгенерированный маппер
				MapperFunc = (Func<IRecord, T>)Delegate.CreateDelegate(typeof(Func<IRecord, T>), mapMethod);

				// Для обратного маппинга также пытаемся использовать сгенерированный метод
				var mapToPropertiesMethod = generatedMapperType.GetMethod("MapToProperties",
					BindingFlags.Public | BindingFlags.Static,
					null,
					new[] { type },
					null);

				if (mapToPropertiesMethod != null)
				{
					// Настройка PropertySetters для обратной совместимости
					var genSetters = new Dictionary<string, Action<T, IRecord>>();
					var genProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

					foreach (var property in genProperties)
					{
						if (property.CanWrite)
						{
							genSetters[property.Name] = (obj, record) =>
							{
								var propertyType = property.PropertyType;
								var tryGetMethod = typeof(IRecord).GetMethod("TryGet")?
									.MakeGenericMethod(propertyType);

								if (tryGetMethod != null)
								{
									var parameters = new object[] { property.Name, null };
									var success = (bool)tryGetMethod.Invoke(record, parameters);
									if (success)
									{
										property.SetValue(obj, parameters[1]);
									}
								}
							};
						}
					}

					PropertySetters = genSetters.ToFrozenDictionary();

					//// Создаем пул объектов
					//var genObjectPolicy = new DefaultPooledObjectPolicy<T>();
					//ObjectPool = new DefaultObjectPool<T>(genObjectPolicy, 1024);

					return;
				}
			}
		}

		// Если сгенерированный маппер не найден, используем скомпилированное выражение
		var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

		// Создаем скомпилированное выражение для быстрого маппинга
		var recordParam = Expression.Parameter(typeof(IRecord), "record");
		var variable = Expression.Variable(type, "obj");
		var expressions = new List<Expression>();

		// Создаем новый объект
		expressions.Add(Expression.Assign(variable, Expression.New(type)));

		// Для каждого свойства добавляем условие установки значения
		foreach (var property in properties)
		{
			if (property.CanWrite)
			{
				var propertyType = property.PropertyType;
				var tryGetMethod = typeof(IRecord).GetMethod("TryGet")?
					.MakeGenericMethod(propertyType);

				if (tryGetMethod != null)
				{
					var outParam = Expression.Parameter(propertyType.MakeByRefType(), "outValue");
					var tryGetCall = Expression.Call(
						recordParam,
						tryGetMethod,
						Expression.Constant(property.Name),
						outParam);

					var assignProperty = Expression.Assign(
						Expression.Property(variable, property),
						outParam);

					var ifThen = Expression.IfThen(
						tryGetCall,
						assignProperty);

					expressions.Add(ifThen);
				}
			}
		}

		// Возвращаем объект
		expressions.Add(variable);

		var block = Expression.Block(new[] { variable }, expressions);
		MapperFunc = Expression.Lambda<Func<IRecord, T>>(block, recordParam).Compile();

		// Настройка PropertySetters для обратной совместимости
		var setters = new Dictionary<string, Action<T, IRecord>>();
		foreach (var property in properties)
		{
			if (property.CanWrite)
			{
				setters[property.Name] = (obj, record) =>
				{
					var propertyType = property.PropertyType;
					var tryGetMethod = typeof(IRecord).GetMethod("TryGet")?
						.MakeGenericMethod(propertyType);

					if (tryGetMethod != null)
					{
						var parameters = new object[] { property.Name, null };
						var success = (bool)tryGetMethod.Invoke(record, parameters)!;
						if (success)
						{
							property.SetValue(obj, parameters[1]);
						}
					}
				};
			}
		}

		PropertySetters = setters.ToFrozenDictionary();

		//// Создаем пул объектов
		//var objectPolicy = new DefaultPooledObjectPolicy<T>();
		//ObjectPool = new DefaultObjectPool<T>(objectPolicy, 1024);
	}

    public static T MapFromRecord(IRecord record) => MapperFunc(record);

	//public static T GetObject() => ObjectPool.Get();

	//public static void ReturnObject(T obj)
	//{
	//	if (obj is IResettable resettable)
	//	{
	//		resettable.TryReset();
	//	}

	//	ObjectPool.Return(obj);
	//}
	
	public static FrozenDictionary<string, object> MapToProperties(T obj)
    {
		// Пытаемся использовать сгенерированный метод, если доступен
		var type = typeof(T);
		var generatedMapperType = type.Assembly.GetType($"{type.Namespace}.{type.Name}Mapper");
		if (generatedMapperType != null)
		{
			var mapToPropertiesMethod = generatedMapperType.GetMethod("MapToProperties",
				BindingFlags.Public | BindingFlags.Static,
				null,
				[type],
				null);

			if (mapToPropertiesMethod != null)
			{
				return (FrozenDictionary<string, object>)mapToPropertiesMethod.Invoke(null, [obj])!;
			}
		}

		// Резервная реализация
		using var pooledDict = new PooledDictionary();
		var properties = pooledDict.Dictionary!;

		foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
		{
			if (property.CanRead)
			{
				var value = property.GetValue(obj);
				if (value is not null)
					properties[property.Name] = value;
			}
		}

		return properties.ToFrozenDictionary();
	}
}
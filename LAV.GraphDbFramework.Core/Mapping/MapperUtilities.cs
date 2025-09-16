using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Mapping;

public static class MapperUtilities
{
	public static bool HasGeneratedMapper(Type type)
	{
		return type.Assembly.GetType($"{type.Namespace}.{type.Name}Mapper") != null;
	}

	public static bool TryGetGeneratedMapper(Type type, out MethodInfo? mapFromRecordMethod)
	{
		var mapperType = type.Assembly.GetType($"{type.Namespace}.{type.Name}Mapper");
		if (mapperType != null)
		{
			mapFromRecordMethod = mapperType!.GetMethod("MapFromRecord", BindingFlags.Public | BindingFlags.Static,
				null,
				[typeof(IRecord)],
				null);

			return mapFromRecordMethod != null;
		}

		mapFromRecordMethod = null;
		return false;
	}

	public static Func<IRecord, T> CreateMapperDelegate<T>() where T : class, new()
	{
		var type = typeof(T);

		// Пытаемся использовать сгенерированный маппер
		if (TryGetGeneratedMapper(type, out var mapFromRecordMethod) && mapFromRecordMethod != null)
		{
			return (Func<IRecord, T>)Delegate.CreateDelegate(typeof(Func<IRecord, T>), mapFromRecordMethod);
		}

		// Используем MapperCache как резервный вариант
		return MapperCache<T>.MapFromRecord;
	}
}
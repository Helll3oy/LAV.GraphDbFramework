using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Mapping;

public static class MapperCache<T>
{
    private static readonly FrozenDictionary<string, Action<T, IRecord>> PropertySetters;
    private static readonly Func<IRecord, T> MapperFunc;

    static MapperCache()
    {
        var type = typeof(T);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var setters = new Dictionary<string, Action<T, IRecord>>();

        foreach (var property in properties)
        {
            if (property.CanWrite)
            {
                setters[property.Name] = (obj, record) =>
                {
                    if (record.TryGet(property.PropertyType, property.Name, out var value))
                        property.SetValue(obj, value);
                };
            }
        }

        PropertySetters = setters.ToFrozenDictionary();

        // Компилируем выражение для быстрого создания объекта
        var recordParam = Expression.Parameter(typeof(IRecord), "record");
        var newObj = Expression.New(type);
        var bindings = new List<MemberBinding>();

        foreach (var property in properties)
        {
            if (property.CanWrite)
            {
                var recordAccess = Expression.Call(
                    recordParam,
                    typeof(IRecord).GetMethod("TryGet")!.MakeGenericMethod(property.PropertyType),
                    Expression.Constant(property.Name),
                    Expression.Parameter(typeof(object).MakeByRefType()));

                var propertySet = Expression.Bind(
                    property,
                    Expression.Condition(
                        recordAccess,
                        Expression.Convert(recordAccess, property.PropertyType),
                        Expression.Default(property.PropertyType)));

                bindings.Add(propertySet);
            }
        }

        var init = Expression.MemberInit(newObj, bindings);
        MapperFunc = Expression.Lambda<Func<IRecord, T>>(init, recordParam).Compile();
    }

    public static T MapFromRecord(IRecord record) => MapperFunc(record);

    public static FrozenDictionary<string, object> MapToProperties(T obj)
    {
        var properties = new Dictionary<string, object>();
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
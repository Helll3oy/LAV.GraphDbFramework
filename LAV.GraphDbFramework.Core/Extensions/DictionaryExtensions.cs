using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Extensions;

public static class DictionaryExtensions
{
    public static T ToObject<T>(this IReadOnlyDictionary<string, object> dict) where T : new()
    {
        var obj = new T();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (dict.TryGetValue(property.Name, out var value) &&
                property.CanWrite &&
                (value == null || property.PropertyType.IsInstanceOfType(value) ||
                 property.PropertyType == typeof(string) ||
                 !property.PropertyType.IsValueType))
            {
                try
                {
                    property.SetValue(obj, value);
                }
                catch
                {
                    // Игнорируем свойства, которые не могут быть установлены
                }
            }
        }

        return obj;
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace MyYamlParser
{
    public static class MyYamlReflectionUtils
    {
        
              
        private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> Properties = new Dictionary<Type, Dictionary<string, PropertyInfo>>();


        public static readonly IReadOnlyDictionary<Type, Type> SimpleTypes = new Dictionary<Type, Type>
        {
            [typeof(string)] = typeof(string),
            [typeof(byte)] = typeof(byte),
            [typeof(int)] = typeof(int),
            [typeof(uint)] = typeof(uint),
            [typeof(long)] = typeof(long),
            [typeof(ulong)] = typeof(ulong),
            [typeof(short)] = typeof(short),
            [typeof(ushort)] = typeof(ushort),
            [typeof(DateTime)] = typeof(DateTime),
            [typeof(TimeSpan)] = typeof(TimeSpan),
            [typeof(bool)] = typeof(bool),
        };
        
        
        public static Dictionary<string, PropertyInfo> ExtractProperties(this Type type)
        {
            if (!Properties.ContainsKey(type))
                Properties.Add(type, type.GetProperties().Where(itm => itm.CanWrite).ToDictionary(itm => itm.Name));
            
            return Properties[type];
        }
        
        
        
        public static bool IsDictionary(this Type toType)
        {
            return toType.GetInterfaces().Any(itm => itm == typeof(IDictionary));
        }
        
        private static object CreateInstanceUsingDefaultConstructor(this Type type)
        {
            var constructor = type.GetConstructors().FirstOrDefault(itm => itm.GetParameters().Length == 0);
            if (constructor == null)
                throw new Exception($"No default constructor for type {type}");
            
            return constructor.Invoke(Array.Empty<Type>());
        }

        public static object ChangeType(this Type toType, string value)
        {

            if (toType == typeof(int))
                return int.Parse(value);
            
            if (toType == typeof(long))
                return long.Parse(value);

            if (toType == typeof(byte))
                return byte.Parse(value);
            
            if (toType == typeof(short))
                return short.Parse(value);

            if (toType == typeof(ushort))
                return ushort.Parse(value);
            
            if (toType == typeof(DateTime))
                return DateTime.Parse(value);

            if (toType == typeof(bool))
            {
                return value.ToLower() == "true" || value == "1";
            }
            
            if (toType == typeof(double))
                return double.Parse(value, CultureInfo.InvariantCulture);

            if (toType == typeof(string))
                return value;
            
            if (toType == typeof(TimeSpan))
                return TimeSpan.Parse(value);

            if (toType.IsEnum)
                return Enum.Parse(toType, value);

            if (toType.IsDictionary())
                return toType.CreateInstanceUsingDefaultConstructor();

            if (toType.IsArray)
                return new List<string>();

            if (toType.IsClass)
            {
                var constructor = toType.GetConstructor(Array.Empty<Type>());
                
                if (constructor == null)
                    throw new Exception("No Default Constructor for type "+toType);

                return constructor.Invoke(Array.Empty<object>());
            }
            
            throw new Exception("Unsupported type: "+toType);
        }
        
        private static IEnumerable<(YamlLine key, IReadOnlyList<YamlLine> value)> FormatLines(this IEnumerable<YamlLine> lines)
        {
            YamlLine resultKey = null;
            List<YamlLine> resultValue = null;

            foreach (var line in lines)
            {
                if (line.Keys.Length == 1)
                {
                    if (resultKey != null)
                    {
                        yield return (resultKey, resultValue);
                        resultValue = null;
                    }

                    resultKey = line;
                    continue;
                }
                
                if (resultValue == null)
                    resultValue = new List<YamlLine>();
                
                resultValue.Add(line.RemoveFirstLevel());
            }
            
            if (resultKey != null || resultValue != null)
                yield return (resultKey, resultValue);
            
        }
        
        private static void SetDictionaryValues(this IDictionary dictionary, IReadOnlyList<YamlLine> childItems)
        {

            var items = childItems.FormatLines();
            
            foreach (var (dictKey, values) in items)
            {
                var type = dictionary.GetType().GetGenericArguments()[1];
                var dictItem = type.CreateInstanceUsingDefaultConstructor();
                
                SetValue(dictItem, values);
                dictionary.Add(dictKey.Keys[0], dictItem);
            }
            
        }

        public static void SetYamlValue(this object instance, PropertyInfo pi, string yamlValue)
        {
            var value = pi.PropertyType.ChangeType(yamlValue);
            pi.SetValue(instance, value);
        }
        
        public static void SetValue(this object instance, IEnumerable<YamlLine> yamlLines)
        {

            var items = yamlLines.FormatLines().ToList();
            
            foreach (var (line, subLines) in items)
                instance.SetValue(line, subLines);
        }
        
        
        private static void SetArrayValues(List<string> list, IReadOnlyList<YamlLine> childItems)
        {

            foreach (var line in childItems)
            {
                list.Add(line.GetArrayValue());
            }
            
        }
        
        
        public static void SetValue(this object instance, YamlLine line, IReadOnlyList<YamlLine> childItems)
        {
            var type = instance.GetType();

            var properties = type.ExtractProperties();

            if (!properties.ContainsKey(line.Keys[0]))
                return;

            var propertyInfo = properties[line.Keys[0]];

            try
            {
                var theValue = propertyInfo.PropertyType.ChangeType(line.Value);

                if (theValue is List<string> list)
                {
                    SetArrayValues(list, childItems);
                    propertyInfo.SetValue(instance,list.ToArray());
                    return;
                }
                
                propertyInfo.SetValue(instance, theValue);
                
                if (SimpleTypes.ContainsKey(propertyInfo.PropertyType) || propertyInfo.PropertyType.IsEnum)
                    return;

                if (theValue is IDictionary dict && childItems != null)
                    dict.SetDictionaryValues(childItems);
                
                if (theValue.GetType().IsClass)
                    SetValue(theValue, childItems);
            }
            catch
            {
                Console.WriteLine("Can not set Value: " + line.Value + " to property: " + propertyInfo.Name +
                                  " of type " + type);
            }
        }


        internal static object CreateObjectByDefaultConstructor(this Type type)
        {
            var ctor = type
                .GetConstructors()
                .FirstOrDefault(itm => itm.GetParameters().Length == 0);
            
            if (ctor == null)
                throw new Exception("There is no default constructor for type: "+type);

            return ctor.Invoke(Array.Empty<object>());
        }



        internal static (Type keyType, Type valueType) GetDictionaryType(this PropertyInfo pi)
        {
            var interfaceType = pi.PropertyType.GetInterfaces()
                .First(itm => itm.FullName.StartsWith("System.Collections.Generic.IDictionary") &&
                              itm.GenericTypeArguments.Length == 2);
            
            return (interfaceType.GetGenericArguments()[0], interfaceType.GetGenericArguments()[1]);
        }

        internal static IDictionary CreateDictionary(this PropertyInfo propertyInfo)
        {
            return (IDictionary)Activator.CreateInstance(propertyInfo.PropertyType);
        }


        internal static Type GetEnumerableElementType(this PropertyInfo pi)
        {

            var enumerableInterfaceType = pi.PropertyType.GetInterfaces()
                .First(itm =>
                    itm.FullName.StartsWith("System.Collections.Generic.IEnumerable") &&
                    itm.GenericTypeArguments.Length == 1);
            
            
            
            return enumerableInterfaceType.GetGenericArguments()[0];
        }


        internal static object CastEnumerable(this PropertyInfo pi, IList data, Type itemType)
        {

            if (pi.PropertyType.BaseType == typeof(Array))
            {
                var result = Array.CreateInstance(itemType, data.Count);
                for (var i=0; i<data.Count; i++)
                    result.SetValue(data[i], i);

                return result;
            }

            return data;
        }
        
        internal static bool IsPropertyDictionary(this PropertyInfo pi)
        {
          return pi.PropertyType.GetInterfaces().Any(itm => itm == typeof(IDictionary));
        }

        
    }
}
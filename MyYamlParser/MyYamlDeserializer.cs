using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MyYamlParser
{
    public static class MyYamlDeserializer
    {

        internal static void PopulateFields(object o, IReadOnlyList<YamlLine> yamlLines, List<string> notFound = null)
        {

            var dict = yamlLines.ToDictionary(itm => itm.KeyAsString);

            foreach (var pi in o.GetType().GetProperties())
            {
                var attribute = pi.GetCustomAttribute<YamlPropertyAttribute>();

                if (attribute == null)
                    continue;

                var key = attribute.Name ?? pi.Name;


                if (dict.TryGetValue(key, out var yamlLine))
                {
                    YamlValueConverter.SetYamlValue(o, pi, yamlLine, yamlLines, notFound);
                    continue;
                }

                if (attribute.DefaultValue != null)
                {
                    if (pi.PropertyType.IsSimpleProperty())
                    {
                        if (attribute.DefaultValue is string strValue)
                            pi.SetYamlValue(pi, strValue);
                        else
                            pi.SetValue(o, attribute.DefaultValue);
                    }
                    else
                    {
                        throw new Exception(
                            $"Can not use default value {attribute.DefaultValue} for the field {pi.Name}");
                    }
                }
                else
                    notFound?.Add(key);
            }

        }
        
        
        public static object Deserialize(byte[] yaml, Type type)
        {
            var result = type.CreateObjectByDefaultConstructor();

            var yamlLines = yaml
                .ParseYaml().ToList();

            PopulateFields(result, yamlLines);
            
            return result;
        }
        
        public static T Deserialize<T>(byte[] yaml, List<string> notFound = null) where T : class, new()
        {
            var result = new T();

            var yamlLines = yaml
                .ParseYaml()
                .ToList();

            PopulateFields(result, yamlLines, notFound);
            
            return result;
        }

        public static T Deserialize<T>(string yaml) where T : class, new()
        {
            return Deserialize<T>(Encoding.UTF8.GetBytes(yaml));
        }
        
    }
}


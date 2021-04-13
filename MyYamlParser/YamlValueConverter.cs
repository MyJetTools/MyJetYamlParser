using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MyYamlParser
{
    public static class YamlValueConverter
    {
        
                
        internal static bool IsSimpleProperty(this Type type)
        {
            return MyYamlReflectionUtils.SimpleTypes.ContainsKey(type) || type.IsEnum;
        }


        private static bool IsPropertyClass(this Type type)
        {
            return type.IsClass && type != typeof(string);
        }

        private static bool IsPropertyEnumerable(this Type type)
        {
            return type.GetInterfaces().Any(itm => itm == typeof(IEnumerable));
        }


        private static void PopulateDictionary(IDictionary src, IReadOnlyList<YamlLine> yamlLines, Type elementType, List<string> notFound)
        {
            var isSimpleProperty = elementType.IsSimpleProperty();
            
            foreach (var line in yamlLines)
            {

                if (line.Keys.Length == 1 && isSimpleProperty)
                {
                    src.Add(line.KeyAsString, line.GetValue());
                    continue;
                }

                if (line.Keys.Length == 1)
                {
            
                    var subLines = GetSubLines(yamlLines, line).ToList();
                    
                    var el = elementType.CreateObjectByDefaultConstructor();
                    MyYamlDeserializer.PopulateFields(el, subLines, notFound);

                    src.Add(line.Keys[0], el);
                }

            }
            
        }


        private static IEnumerable<object> IterateElements(Type type, IEnumerable<YamlLine> yamlLines)
        {
            foreach (var yamlLine in yamlLines)
                yield return type.ChangeType(yamlLine.KeyAsString);
        }



        
        private static IEnumerable<YamlLine> GetSubLines(IEnumerable<YamlLine> yamlLines, YamlLine line)
        {
            return yamlLines
                .Where(itm => itm.IsHigherLevelOf(line))
                .Select(itm => itm.CreateAsSubItem(line));
        }
        
        public static void SetYamlValue(object o, PropertyInfo pi, YamlLine yamlLine, IReadOnlyList<YamlLine> yamlLines, List<string> notFound)
        {
            
            if (pi.PropertyType.IsSimpleProperty())
            {
                o.SetYamlValue(pi, yamlLine.Value);
                return;
            }
            
            if (pi.IsPropertyDictionary())
            {
                var dictInstance = pi.CreateDictionary();
                var subYamlLines = GetSubLines(yamlLines, yamlLine);
                var dictItemType = pi.GetDictionaryType();
                PopulateDictionary(dictInstance, subYamlLines.ToList(), dictItemType.valueType, notFound);
                pi.SetValue(o, dictInstance);
                return;
            }                
            
            if (pi.PropertyType.IsPropertyEnumerable())
            {
                var subYamlLines = GetSubLines(yamlLines, yamlLine);
                var elementType = pi.GetEnumerableElementType();

                var resultEnumerable = IterateElements(elementType, subYamlLines);
                var result = pi.CastEnumerable(resultEnumerable.ToList(), elementType);
                pi.SetValue(o, result);
                return;
            }                
           
            if (pi.PropertyType.IsPropertyClass())
            {
                var classInstance = pi.PropertyType.CreateObjectByDefaultConstructor();
                var subYamlLines = GetSubLines(yamlLines, yamlLine);
                MyYamlDeserializer.PopulateFields(classInstance, subYamlLines.ToList(), notFound);
                pi.SetValue(o, classInstance);
            }

        }
    }
}
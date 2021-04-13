using System;

namespace MyYamlParser
{
    public class YamlPropertyAttribute : Attribute
    {

        public YamlPropertyAttribute(string name = null, object defaultValue = null)
        {
            Name = name;
            DefaultValue = defaultValue;
        }
        
        public string Name { get; }
        
        public object DefaultValue { get; }
        
    }
}
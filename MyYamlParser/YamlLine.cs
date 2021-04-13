using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyYamlParser
{
    public class YamlLine
    {
        public YamlLine(string[] keys, string value)
        {
            Keys = keys;
            Value = value;
        }
        
        
        public string[] Keys { get; }
        public string Value { get; }



        private string _keyAsString;

        public string KeyAsString => _keyAsString ??= Keys.KeysAsString();


        public string GetValue()
        {
            if (!string.IsNullOrEmpty(Value))
                return Value;

            var key = KeyAsString;
            if (key.StartsWith("-"))
                return key.Substring(1, key.Length - 1).Trim();

            return null;
        }


        public bool IsHigherLevelOf(YamlLine yamlLine)
        {
            if (Keys.Length <= yamlLine.Keys.Length)
                return false;

            for (var i = 0; i < yamlLine.Keys.Length; i++)
            {
                if (Keys[i] != yamlLine.Keys[i])
                    return false;
            }

            return true;

        }
        

        public override string ToString()
        {
            return KeyAsString + ":" + (Value ?? "<<NULL>>");
        }

        public YamlLine RemoveFirstLevel()
        {
            return new YamlLine(Keys.Skip(1).ToArray(), Value);
        }

        public string GetArrayValue()
        {
            return Keys[0].Replace("- ", "").Trim();
        }

        public YamlLine CreateAsSubItem(YamlLine yamlLine)
        {

            var keys = Keys.Skip(yamlLine.Keys.Length).ToArray();
            
            return new YamlLine(keys, Value);
        }
    }

    public static class YamlLineUtils
    {
        internal static string KeysAsString(this IEnumerable<string> src)
        {
            var sb = new StringBuilder();
            foreach (var value in src)
            {
                if (sb.Length > 0)
                    sb.Append('.');
                sb.Append(value);
            }

            var result =  sb.ToString();

            if (result.StartsWith("-"))
                return result.Substring(1, result.Length - 1).Trim();

            return result;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyYamlParser
{
 
    public static class MyYamlParser
    {
        
        internal enum ParseMode
        {
            Space, Key, Value
        }

        private const byte Cl = 13;
        private const byte Cr = 10;
        private const byte Space = 32;

        private const byte YamlSeparator = (byte) ':';
        private const byte ArrayStart = (byte) '-';

        private static bool IsClOrCr(this byte b)
        {
            return b == Cl || b == Cr;
        }

        private static bool IsArrayElement(this byte b)
        {
            return b == ArrayStart;
        }

        private static (int spaces, string key, string value) ParseYamlLine(this ReadOnlyMemory<byte> line, int lineNo)
        {

            var i = 0;


            var spacesResult = 0;
            string key = null;


            var parseMode = ParseMode.Space;

            var hasTab = false;

            var arraySymbol = false;

            foreach (var b in line.Span)
            {

                try
                {

                    if (parseMode == ParseMode.Space)
                    {
                        if (b == 9)
                            hasTab = true;

                        if (b > Space)
                        {

                            if (b == ArrayStart)
                                arraySymbol = true;

                            if (b == YamlSeparator)
                                throw new Exception($"Line #{lineNo} can not be started with Yaml Separator ':'. " +
                                                    Encoding.UTF8.GetString(line.Span));

                            spacesResult = i;
                            parseMode = ParseMode.Key;
                            if (hasTab)
                                throw new Exception($"LineNo: Tab can not be used as space markers at line #{lineNo}: " +
                                                    Encoding.UTF8.GetString(line.Span));
                        }

                        continue;
                    }

                    if (parseMode == ParseMode.Key)
                    {
                        if (b == YamlSeparator)
                        {
                            key = Encoding.UTF8.GetString(line.Span.Slice(spacesResult, i - spacesResult));
                            break;
                        }

                    }

                }
                finally
                {
                    i++;
                }
            }


            if (arraySymbol && key==null)
            {
                key = Encoding.UTF8.GetString(line.Span.Slice(spacesResult, line.Length - spacesResult));
                return (spacesResult, key.Trim(), null);
            }
            
            
            string value = null;

            if (i < line.Length)
            {
                value = Encoding.UTF8.GetString(line.Span.Slice(i, line.Length - i));
                value = value.Trim();
            }

            return (spacesResult, key, value);

        }


        public static IEnumerable<(int lineNo, ReadOnlyMemory<byte> data)> ParseLineByLine(this byte[] yaml)
        {
            
            var i = 0;
            var startIndex = 0;
            var lineNo = 0;
            while (i<yaml.Length)
            {

                try
                {
                    if (yaml[i].IsClOrCr())
                    {
                        
                        if (i>0)
                            if (yaml[i - 1].IsClOrCr())
                            {
                                startIndex = i+1;
                                continue;
                            }

                        lineNo++;

                        if (i > startIndex)
                        {
                            var toYield = yaml.AsMemory(startIndex, i - startIndex);
                            yield return (lineNo, toYield);
                        }
                            
                        startIndex = i+1;
                    }

                }
                finally
                {
                    i++;
                }

            }
            
            if (i > startIndex)
            {
                lineNo++;
                var toYield = yaml.AsMemory(startIndex, i - startIndex);
                yield return (lineNo, toYield);
            }
            
        }



        
        public static IEnumerable<(int Spaces, string Key, string Value)> ParseLinesYaml(this byte[] yaml)
        {

            foreach (var (lineNo, data) in yaml.ParseLineByLine())
            {
                var result = data.ParseYamlLine(lineNo);

                if (!string.IsNullOrEmpty(result.key))
                    yield return result;

            }

        }



        private static void ReduceLevel(SortedList<int, string> items, int spaces)
        {
            while (items.Keys[^1]>=spaces)
            {
                items.RemoveAt(items.Count-1);
            }
        }

        public static IEnumerable<YamlLine> ParseYaml(this byte[] yaml)
        {
            var items = new SortedList<int, string>();

            foreach (var (spaces, key, value) in yaml.ParseLinesYaml())
            {

                if (items.Count == 0)
                {
                    items.Add(spaces, key);
                    yield return new YamlLine(items.Values.ToArray(), value);
                    continue;
                }

                if (spaces == 0)
                {
                    items.Clear();
                    items.Add(spaces, key);
                    yield return new YamlLine(items.Values.ToArray(), value);
                    continue;
                }

                if (items.Keys[^1] >= spaces)
                {
                    if (!items.ContainsKey(spaces))
                        throw new Exception("Invalid Yaml with Key: [" + key + "] after [" +items.Values.KeysAsString() + "]");

                    ReduceLevel(items, spaces);
                }
                
                items.Add(spaces, key);

                
                yield return new YamlLine(items.Values.ToArray(), value);

            }

        }

    }
}
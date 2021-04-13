using System.Collections.Generic;
using NUnit.Framework;

namespace MyYamlParser.Tests
{
    public class TestDictionariesWithSimpleValues
    {

        public class TestSimpleDictionary
        {
            [YamlProperty("MyField")]
            public string MyField { get; set; }
            
            [YamlProperty]
            public Dictionary<string, string> Dict { get; set; }
            
        }
        
        

        [Test]
        public void TestSimpleDictionaries()
        {
            var testYaml =
                "MyField: value\n" +
                "Dict:\n" +
                "  - key1: value1\n" +
                "  - key2: value2\n";
            
            var result = MyYamlDeserializer.Deserialize<TestSimpleDictionary>(testYaml);
            
            Assert.AreEqual("value", result.MyField);
            
            Assert.AreEqual(2, result.Dict.Count);
            Assert.AreEqual("value1", result.Dict["key1"]);
            Assert.AreEqual("value2", result.Dict["key2"]);

        }
    }
}
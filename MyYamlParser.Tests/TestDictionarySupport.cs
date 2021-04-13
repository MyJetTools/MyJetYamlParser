using System.Collections.Generic;
using NUnit.Framework;

namespace MyYamlParser.Tests
{

    public class DictionaryItem
    {
        [YamlProperty("FieldA")]
        public string FieldA { get; set; }
        
        [YamlProperty("FieldB")]
        public int FieldB { get; set; }
    }

    
    public class TestWithDictionary
    {
        [YamlProperty("MyField")]
        public string MyField { get; set; }
        
        [YamlProperty("Dict")]
        public Dictionary<string, DictionaryItem> Dict { get; set; }
    }
    
    
    public class TestDictionarySupport
    {

        [Test]
        public void TestYamlWithDictionary()
        {
            var testYaml =
                "MyField: value\n" +
                "Dict:\n" +
                "  Key1:\n" +
                "    - FieldA: Value1A\n" +
                "    - FieldB: 1\n" +
                "  Key2:\n" +
                "    - FieldA: Value2A\n" +
                "    - FieldB: 2\n";


            var result = MyYamlDeserializer.Deserialize<TestWithDictionary>(testYaml);
            
            Assert.AreEqual("value", result.MyField);
            
            Assert.AreEqual(2, result.Dict.Count);
            
            Assert.AreEqual("Value1A", result.Dict["Key1"].FieldA);
            Assert.AreEqual(1, result.Dict["Key1"].FieldB);
            
            Assert.AreEqual("Value2A", result.Dict["Key2"].FieldA);
            Assert.AreEqual(2, result.Dict["Key2"].FieldB);
        }





    }
}
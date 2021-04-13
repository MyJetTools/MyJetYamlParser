using System.Text;
using NUnit.Framework;

namespace MyYamlParser.Tests
{
    public class TestDefaultValues
    {
        public class TestModelWithDefaultValue
        {
            [YamlProperty]
            public int Version { get; set; }
            
            [YamlProperty(defaultValue: 45)]
            public int DefaultValue { get; set; }
        }
        
        [Test]
        public void TestModelParse()
        {
            var yamlText = "Version:2" + (char)13+(char)10+
                           "Field:\n" +
                           "   \n"+
                           "  MyDataValue:value\n" +
                           "  MyDataValue2:2\n\n" +
                           "field2:asd\n" +
                           "MyBool: True\n" +
                           "EnumField: Two\n"+
                           "MyArray:\n" +
                           "  - value1\n" +
                           "  - value2";
            
            var yamlBytes = Encoding.UTF8.GetBytes(yamlText);

            var model = MyYamlDeserializer.Deserialize<TestModelWithDefaultValue>(yamlBytes);
            
            Assert.AreEqual(2, model.Version);

            Assert.AreEqual(45, model.DefaultValue);
            
  
        }
    }
}
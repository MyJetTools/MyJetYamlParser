using System;
using System.Text;
using NUnit.Framework;

namespace MyYamlParser.Tests
{
    public class ParseSimpleYaml
    {

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
                           "TimeSpanField: 00:00:20\n"+
                           "EnumField: Two\n"+
                           "MyArray:\n" +
                           "  - value1\n" +
                           "  - value2";
            
            var yamlBytes = Encoding.UTF8.GetBytes(yamlText);

            var model = MyYamlDeserializer.Deserialize<TestModel>(yamlBytes);
            
            Assert.AreEqual(2, model.Version);
            Assert.AreEqual("value", model.Field.MyDataValue);
            Assert.AreEqual(2, model.Field.MyDataValue2);
            
            Assert.AreEqual(TimeSpan.Parse("00:00:20"), model.TimeSpanField);
            
            Assert.AreEqual(2, model.MyArray.Length);
            Assert.AreEqual("value1", model.MyArray[0]);
            Assert.AreEqual("value2", model.MyArray[1]);
            
            Assert.AreEqual(MyEnum.Two, model.EnumField);
            Assert.IsTrue(model.MyBool);
        }
        
        
    }


    public class TestModel
    {
        public class SubModel
        {
            [YamlProperty]
            public string MyDataValue { get; set; }
            
            [YamlProperty]
            public long MyDataValue2 { get; set; }
        }
        
        [YamlProperty]
        public int Version { get; set; }
        
        [YamlProperty]
        public SubModel Field { get; set; }
        
        [YamlProperty]
        public string[] MyArray { get; set; }
        
        [YamlProperty]
        public bool MyBool { get; set; }
        
        [YamlProperty]
        public MyEnum EnumField { get; set; }
        
        [YamlProperty]
        public TimeSpan TimeSpanField { get; set; }

        public string ReadOnlyField => null;



    }
    
    public class TestModelException
    {
        public class SubModel
        {
            [YamlProperty]
            public string MyDataValue { get; set; }
            [YamlProperty]
            public long MyDataValue2 { get; set; }
            [YamlProperty]
            public string Field3 { get; set; }
        }
        
        [YamlProperty]
        public int Version { get; set; }
        
        [YamlProperty]
        public SubModel Field { get; set; }
        
    }


    public enum MyEnum
    {
        One, Two, Three
    }
    
    
}
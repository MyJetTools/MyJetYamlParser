using System.Linq;
using System.Text;
using NUnit.Framework;

namespace MyYamlParser.Tests
{
    public class TestYamlLinesParsing
    {

        [Test]
        public void TestSimpleYamlLinesParsing()
        {
            var yamlText = 
                "version:2\nfield:\n"+
                "  mydatavalue:value\n\n"+
                "field2:asd";
            var yamlBytes = Encoding.UTF8.GetBytes(yamlText);


            var result = yamlBytes.ParseLinesYaml().ToArray();

            Assert.AreEqual(4, result.Length);

            Assert.AreEqual(0, result[0].Spaces);
            Assert.AreEqual("version", result[0].Key);
            Assert.AreEqual("2", result[0].Value);

            Assert.AreEqual(0, result[1].Spaces);
            Assert.AreEqual("field", result[1].Key);
            Assert.IsNull(result[1].Value);

            Assert.AreEqual(2, result[2].Spaces);
            Assert.AreEqual("mydatavalue", result[2].Key);
            Assert.AreEqual("value", result[2].Value);

            Assert.AreEqual(0, result[3].Spaces);
            Assert.AreEqual("field2", result[3].Key);
            Assert.AreEqual("asd", result[3].Value);
        }

        [Test]
        public void TestSimpleYamlParsing()
        {
            var yamlText =
                "version:2\nfield:\n"+
                "  mydatavalue:value\n"+
                "  mydatavalue2:value2\n\n"+
                "field2:asd";
            var yamlBytes = Encoding.UTF8.GetBytes(yamlText);


            var result = yamlBytes.ParseYaml().ToArray();

            Assert.AreEqual(5, result.Length);

            Assert.AreEqual("version", result[0].Keys[0]);
            Assert.AreEqual("2", result[0].Value);

            Assert.AreEqual("field", result[1].Keys[0]);
            Assert.IsNull(result[1].Value);

            Assert.AreEqual("field", result[2].Keys[0]);
            Assert.AreEqual("mydatavalue", result[2].Keys[1]);
            Assert.AreEqual("value", result[2].Value);

            Assert.AreEqual("field", result[3].Keys[0]);
            Assert.AreEqual("mydatavalue2", result[3].Keys[1]);
            Assert.AreEqual("value2", result[3].Value);

            Assert.AreEqual("field2", result[4].Keys[0]);
            Assert.AreEqual("asd", result[4].Value);
        }

     
        
        
        
    }
}
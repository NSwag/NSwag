using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NJsonSchema;
using NSwag.CodeGeneration.CSharp;
using NSwag.CodeGeneration.TypeScript;

namespace NSwag.Generation.WebApi.Tests
{
    [TestClass]
    public class XmlTests
    {
        public class MyXmlController : ApiController
        {
            public void Post([FromBody]XmlDocument xmlDocument)
            {
            }
        }

        [TestMethod]
        public async Task When_FromBody_and_xml_document_then_consumes_is_xml()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllerAsync<MyXmlController>();
            var json = document.ToJson();

            // Assert
            var operation = document.Operations.First().Operation;
            Assert.AreEqual("application/xml", operation.Consumes[0]);
            Assert.AreEqual(JsonObjectType.String, operation.Parameters.First().Schema.ActualSchema.Type);
        }

        [TestMethod]
        public async Task When_body_is_xml_then_correct_csharp_is_generated()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await generator.GenerateForControllerAsync<MyXmlController>();

            // Act
            var gen = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings());
            var code = gen.GenerateFile();

            // Assert
            Assert.IsTrue(code.Contains("(string xmlDocument, "));
            Assert.IsTrue(code.Contains("var content_ = new System.Net.Http.StringContent(xmlDocument);"));
            Assert.IsTrue(code.Contains("content_.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(\"application/xml\");"));
        }

        [TestMethod]
        public async Task When_body_is_xml_then_correct_TypeScript_is_generated()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await generator.GenerateForControllerAsync<MyXmlController>();

            var settings = new TypeScriptClientGeneratorSettings { Template = TypeScriptTemplate.JQueryCallbacks };
            settings.TypeScriptGeneratorSettings.TypeScriptVersion = 1.8m;

            // Act
            var gen = new TypeScriptClientGenerator(document, settings);
            var code = gen.GenerateFile();

            // Assert
            Assert.IsTrue(code.Contains("(xmlDocument: string, "));
            Assert.IsTrue(code.Contains("const content_ = xmlDocument;"));
            Assert.IsTrue(code.Contains("\"Content-Type\": \"application/xml\""));
        }
    }
}

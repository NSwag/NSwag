using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSwag.CodeGeneration.OperationNameGenerators;
using NSwag.CodeGeneration.TypeScript;

namespace NSwag.Generation.WebApi.Tests.OperationNameGenerator
{
    [TestClass]
    public class MultipleClientsFromOperationIdOperationNameGeneratorTests
    {
        public class PointController
        {
            [HttpPost]
            public Point[] Get()
            {
                throw new NotImplementedException();
            }

            [HttpGet]
            public Point Get(int id)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public async Task When_two_methods_have_same_name_then_generated_id_is_still_different()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await generator.GenerateForControllerAsync<PointController>();
            var codeGenerator = new TypeScriptClientGenerator(document, new TypeScriptClientGeneratorSettings
            {
                OperationNameGenerator = new MultipleClientsFromOperationIdOperationNameGenerator()
            });

            // Act
            var code = codeGenerator.GenerateFile();

            // Assert
            Assert.IsTrue(code.Contains(" get("));
            Assert.IsTrue(code.Contains(" getAll("));
        }
    }
}

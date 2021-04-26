using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSwag.Annotations;
using NSwag.CodeGeneration.OperationNameGenerators;
using NSwag.CodeGeneration.TypeScript;
using System.Windows;
using System.Linq;

namespace NSwag.Generation.WebApi.Tests.OperationNameGenerator
{
    /// <summary>
    /// Test class for MultipleClientsFromFirstTagAndPathSegmentsOperationNameGenerator
    /// </summary>
    /// <remarks>
    /// When endpoints are given OperationIds in the format {ControllerClassName}_{id}, use
    /// MultipleClientsFromOperationIdOperationNameGenerator to ensure each controller gets its own client.
    /// However, when the OperationId is in a different format use tags with the
    /// MultipleClientsFromFirstTagAndPathSegmentsOperationNameGenerator to generate unique clients.
    /// </remarks>
    [TestClass]
    public class MultipleClientsFromFirstTagAndPathSegmentsOperationNameGeneratorTests
    {
        [SwaggerTag("PointControllerA")]
        public class PointControllerA
        {
            [SwaggerOperation("PointControllerAGetPoint")]
            [HttpGet]
            public Point Get(int id) 
            {
                throw new NotImplementedException();
            }
        }

        [SwaggerTag("PointControllerB")]
        public class PointControllerB
        {
            [SwaggerOperation("PointControllerBGetPoint")]
            [HttpGet]
            public Point Get(int id)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public async Task When_operations_have_different_tags_they_are_grouped_into_different_clients()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await generator.GenerateForControllersAsync(new List<Type>() { typeof(PointControllerA), typeof(PointControllerB) });
            var codeGenerator = new TypeScriptClientGenerator(document, new TypeScriptClientGeneratorSettings
            {
                OperationNameGenerator = new MultipleClientsFromFirstTagAndPathSegmentsOperationNameGenerator()
            });

            // Act
            var code = codeGenerator.GenerateFile();

            // Assert
            Assert.IsTrue(code.Contains("export class PointControllerAClient"));
            Assert.IsTrue(code.Contains("export class PointControllerBClient"));
        }

        [TestMethod]
        public async Task When_operations_have_no_tags_they_are_grouped_into_one_client()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await generator.GenerateForControllersAsync(new List<Type>() { typeof(PointControllerA), typeof(PointControllerB) });

            // Remove tags
            foreach (var path in document.Paths.Values)
            {
                foreach (var operation in path.Values)
                {
                    operation.Tags.Clear();
                }
            }
            var codeGenerator = new TypeScriptClientGenerator(document, new TypeScriptClientGeneratorSettings
            {
                OperationNameGenerator = new MultipleClientsFromFirstTagAndPathSegmentsOperationNameGenerator()
            });

            // Act
            var code = codeGenerator.GenerateFile();

            // Assert
            Assert.IsTrue(code.Contains("export class Client"));
            Assert.IsTrue(!code.Contains("export class PointControllerAClient"));
            Assert.IsTrue(!code.Contains("export class PointControllerBClient"));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.Http;
using NSwag.Generation.Processors;

namespace NSwag.Generation.WebApi.Tests.OperationProcessors
{
    [TestClass]
    public class ApiVersionProcessorWithWebApiTests
    {
        [ApiVersion("1")]
        public class VersionedControllerV1 : ApiController
        {
            [HttpGet, Route("api/v{version:apiVersion}/foo")]
            public void Foo()
            {
            }

            [HttpGet, Route("api/v{version:apiVersion}/bar")]
            public void Bar()
            {
            }
        }

        [ApiVersion("2")]
        public class VersionedControllerV2 : ApiController
        {
            [HttpGet, Route("api/v{version:apiVersion}/foo")]
            public void Foo()
            {
            }

            [HttpGet, Route("api/v{version:apiVersion}/bar")]
            public void Bar()
            {
            }
        }

        [TestMethod]
        public async Task When_no_IncludedVersions_are_defined_then_all_routes_are_available_and_replaced()
        {
            // Arrange
            var settings = new WebApiOpenApiDocumentGeneratorSettings();
            var generator = new WebApiOpenApiDocumentGenerator(settings);

            // Act
            var document = await generator.GenerateForControllersAsync(new List<Type>
            {
                typeof(VersionedControllerV1),
                typeof(VersionedControllerV2)
            });

            // Assert
            Assert.AreEqual(4, document.Paths.Count);

            Assert.IsTrue(document.Paths.ContainsKey("/api/v1/foo"));
            Assert.IsTrue(document.Paths.ContainsKey("/api/v1/bar"));

            Assert.IsTrue(document.Paths.ContainsKey("/api/v2/foo"));
            Assert.IsTrue(document.Paths.ContainsKey("/api/v2/bar"));
        }

        [TestMethod]
        public async Task When_IncludedVersions_are_set_then_only_these_are_available_in_document()
        {
            // Arrange
            var settings = new WebApiOpenApiDocumentGeneratorSettings();
            settings.OperationProcessors.TryGet<ApiVersionProcessor>().IncludedVersions = new[] { "1" };

            var generator = new WebApiOpenApiDocumentGenerator(settings);

            // Act
            var document = await generator.GenerateForControllersAsync(new List<Type>
            {
                typeof(VersionedControllerV1),
                typeof(VersionedControllerV2)
            });
            var json = document.ToJson();

            // Assert
            Assert.AreEqual(2, document.Paths.Count);

            Assert.IsTrue(document.Paths.ContainsKey("/api/v1/foo"));
            Assert.IsTrue(document.Paths.ContainsKey("/api/v1/bar"));
        }
    }
}

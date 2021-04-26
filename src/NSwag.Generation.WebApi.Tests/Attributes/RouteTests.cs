using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSwag.Generation.WebApi;

namespace NSwag.Generation.WebApi.Tests.Attributes
{
    [TestClass]
    public class RouteTests
    {
        // https://github.com/RicoSuter/NSwag/issues/510

        public abstract class RegionalItemController : ApiController
        {
            public int RegionId => 0;
        }

        [System.Web.Http.Route("api/region/{regionId:int}/food")]
        public class FoodsController : RegionalItemController
        {
            [System.Web.Http.HttpGet]
            public string[] Get(int regionId)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public async Task When_route_contains_path_parameter_and_action_method_proper_parameter_then_it_is_generated_as_parameter()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings { IsAspNetCore = true });

            // Act
            var document = await generator.GenerateForControllerAsync<FoodsController>();
            var json = document.ToJson();

            // Assert
            var operation = document.Operations.First();
            Assert.IsTrue(operation.Path.Contains("{regionId}"));
            Assert.AreEqual("regionId", operation.Operation.Parameters.First().Name);
        }
    }
}

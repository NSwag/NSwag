using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NSwag.Generation.WebApi.Tests
{
    [TestClass]
    public class InheritanceTests
    {
        public class TestController : ApiController
        {
            public void Post([FromBody]CC value)
            {
            }
        }

        public class AA
        {
            public string FirstName { get; set; }
        }

        public class BB : AA
        {
            public string LastName { get; set; }
        }

        public class CC : BB
        {
            public string Address { get; set; }
        }

        [TestMethod]
        public async Task When_generating_type_with_deep_inheritance_then_allOf_has_one_item()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllerAsync<TestController>();
            var swaggerSpecification = document.ToJson();

            // Assert
            Assert.AreEqual(2, Regex.Matches(Regex.Escape(swaggerSpecification), "allOf").Count); // must have an allOf in BB and CC, no more (rest are refs)
        }

        [RoutePrefix("api/common/standard")]
        public class StandardController : ApiController
        {
            [HttpPost, Route("export")]
            public void ExportStandard(string criteria) {  }

            [HttpPost, Route("foo")]
            public void Foo(string criteria) { }
        }

        [RoutePrefix("api/whatever/specific")]
        public class SpecificController : StandardController
        {
            [HttpPost, Route("export")]
            public void ExportSpecific(string criteria) {  }
        }

        [TestMethod]
        public async Task When_there_are_duplicate_paths_through_inheritance_then_the_base_method_is_ignored()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllersAsync(new Type[] { typeof(StandardController), typeof(SpecificController) });
            var json = document.ToJson();

            // Assert
            Assert.AreEqual(4, document.Operations.Count());

            Assert.IsTrue(document.Operations.Any(o => o.Path == "/api/common/standard/export"));
            Assert.IsTrue(document.Operations.Any(o => o.Path == "/api/common/standard/foo"));

            Assert.IsTrue(document.Operations.Any(o => o.Path == "/api/whatever/specific/export"));
            Assert.IsTrue(document.Operations.Any(o => o.Path == "/api/whatever/specific/foo"));
        }
    }
}

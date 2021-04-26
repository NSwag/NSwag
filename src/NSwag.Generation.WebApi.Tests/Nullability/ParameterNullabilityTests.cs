using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NJsonSchema;
using NJsonSchema.Annotations;

namespace NSwag.Generation.WebApi.Tests.Nullability
{
    [TestClass]
    public class ParameterNullabilityTests
    {
        public class NotNullParameterTestController : Controller
        {
            [Route("Abc")]
            public object Abc([NotNull]object foo, object bar)
            {
                return null;
            }
        }

        [TestMethod]
        public async Task When_parameter_has_NotNullAttribute_then_it_is_not_nullable()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllerAsync<NotNullParameterTestController>();
            var json = document.ToJson();

            // Assert 
            Assert.IsFalse(document.Operations.First().Operation.Parameters[0].IsNullable(SchemaType.Swagger2));
            Assert.IsTrue(document.Operations.First().Operation.Parameters[1].IsNullable(SchemaType.Swagger2));
        }

        public class RequiredParameterTestController : Controller
        {
            [Route("Abc")]
            public object Abc([FromBody, Required] object foo, object bar)
            {
                return null;
            }
        }

        [TestMethod]
        public async Task When_AllowNullableBodyFalse_parameter_has_RequiredAttribute_then_it_is_not_nullable()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings() {AllowNullableBodyParameters = false});

            // Act
            var document = await generator.GenerateForControllerAsync<RequiredParameterTestController>();
            var json = document.ToJson();

            // Assert 
            Assert.IsFalse(document.Operations.First().Operation.Parameters[0].IsNullable(SchemaType.Swagger2));
            Assert.IsTrue(document.Operations.First().Operation.Parameters[1].IsNullable(SchemaType.Swagger2));
        }


        public class ParameterTestController : Controller
        {
            [Route("Abc")]
            public object Abc([FromBody] object foo, object bar)
            {
                return null;
            }
        }

        [TestMethod]
        public async Task When_AllowNullableBodyTrue_parameter_then_it_is_nullable()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings() { AllowNullableBodyParameters = true });

            // Act
            var document = await generator.GenerateForControllerAsync<ParameterTestController>();
            var json = document.ToJson();

            // Assert 
            Assert.IsTrue(document.Operations.First().Operation.Parameters[0].IsNullable(SchemaType.Swagger2));
            Assert.IsTrue(document.Operations.First().Operation.Parameters[1].IsNullable(SchemaType.Swagger2));
        }

        [TestMethod]
        public async Task When_AllowNullableBodyFalse_parameter_then_it_is_not_nullable()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings() { AllowNullableBodyParameters = false });

            // Act
            var document = await generator.GenerateForControllerAsync<ParameterTestController>();
            var json = document.ToJson();

            // Assert 
            Assert.IsFalse(document.Operations.First().Operation.Parameters[0].IsNullable(SchemaType.Swagger2));
            Assert.IsTrue(document.Operations.First().Operation.Parameters[1].IsNullable(SchemaType.Swagger2));
        }
    }
}
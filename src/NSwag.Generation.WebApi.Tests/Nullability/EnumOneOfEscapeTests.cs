using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NSwag.Annotations;

namespace NSwag.Generation.WebApi.Tests.Nullability
{

    [TestClass]
    public class EnumOneOfEscapeTests
    {
        public class EnumModelBinder : IModelBinder
        {
            public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
            {
                throw new NotImplementedException();
            }
        }

        public enum MyEnum
        {
            Foo = 1,
            Bar = 20
        }

        private class TestModel
        {
            public string foo { get; set; }
        }

        public class EnumOneOfEscapeController : ApiController
        {

            [ResponseType(typeof(List<TestModel>))]
            public IHttpActionResult GetEventLogRows(
                [ModelBinder(typeof(EnumModelBinder))] MyEnum? integrationName = MyEnum.Foo
            )
            {
                return null;
            }
        }

        [TestMethod]
        public async Task Get_NullableQueryParamEnums_ShouldHaveSingleOneOf()
        {
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings()
            {
                SchemaType = SchemaType.OpenApi3,
                AllowNullableBodyParameters = false,
                FlattenInheritanceHierarchy = true,
                AllowReferencesWithProperties = false,
                IsAspNetCore = false
            });
            var document = await generator.GenerateForControllerAsync<EnumOneOfEscapeController>();
            document.Generator = null;
            var json = document.ToJson(SchemaType.OpenApi3, Formatting.None);
            JObject jObj = JObject.Parse(json);
            string enumParameterJson = jObj["paths"]["/api/EnumOneOfEscape"]["get"]["parameters"].ToString(Formatting.None);
            string expectedResult =
                "[{\"name\":\"integrationName\",\"in\":\"query\",\"schema\":{\"default\":1,\"nullable\":true,\"oneOf\":[{\"$ref\":\"#/components/schemas/MyEnum\"}]},\"x-position\":1}]";

            Assert.AreEqual(expectedResult, enumParameterJson);
        }
    }
}

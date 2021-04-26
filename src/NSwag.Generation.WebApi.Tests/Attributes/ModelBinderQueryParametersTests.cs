using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NJsonSchema;
using NSwag.Annotations;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace NSwag.Generation.WebApi.Tests.Attributes
{
    [TestClass]
    public class ModelBinderQueryParametersTests
    {
        public class TempType {
            public enum Season
            {
                Spring,
                Summer,
                Autumn,
                Winter
            }
        }

        public class CustomModelBinder : IModelBinder
        {
            public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
            {
                return true;
            }
        }


        public enum SortDirection
        {
            Asc,
            Desc
        }

        [Route("api/modelbinder")]
        public class ModelBinderQueryParametersController : Controller
        {

            [Route("/temptypes")]
            [ResponseType(typeof(List<TempType>))]
            public IHttpActionResult GetTempTypes(
                bool? active,
                int? firstRow = 0,
                int? rowCount = 0,
                string textToSearch = "",
                bool calculateRowCount = false,
                bool? includedInCalendarSync = false,
                [ModelBinder(typeof(CustomModelBinder))] List<TempType.Season> seasons = null,
                [FromUri] List<KeyValuePair<string, SortDirection>> sortings = null
            )
            {
                return null;
            }
        }

        [TestMethod]
        public async Task When_model_binder_is_used_in_get_its_defined_as_query_param()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllerAsync<ModelBinderQueryParametersController>();
            var operation = document.Operations.First(o => o.Path == "/temptypes").Operation;

            // Assert
            Assert.AreEqual(OpenApiParameterKind.Query, operation.ActualParameters[0].Kind);
            Assert.AreEqual(OpenApiParameterKind.Query, operation.ActualParameters[1].Kind);
            Assert.AreEqual(OpenApiParameterKind.Query, operation.ActualParameters[2].Kind);
            Assert.AreEqual(OpenApiParameterKind.Query, operation.ActualParameters[3].Kind);
            Assert.AreEqual(OpenApiParameterKind.Query, operation.ActualParameters[4].Kind);
            Assert.AreEqual(OpenApiParameterKind.Query, operation.ActualParameters[5].Kind);
            Assert.AreEqual(OpenApiParameterKind.Query, operation.ActualParameters[6].Kind);
            Assert.AreEqual(OpenApiParameterKind.Query, operation.ActualParameters[7].Kind);
        }
    }
}
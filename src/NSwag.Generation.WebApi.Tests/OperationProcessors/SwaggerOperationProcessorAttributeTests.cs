using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using NSwag.Annotations;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace NSwag.Generation.WebApi.Tests.OperationProcessors
{
    [TestClass]
    public class SwaggerOperationProcessorAttributeTests
    {
        public class TestController : ApiController
        {
            [ReDocCodeSample("foo", "bar")]
            [ReDocCodeSample("baz", "buz")]
            public void Run()
            {
            }
        }

        public class ReDocCodeSampleAttribute : SwaggerOperationProcessorAttribute
        {
            public ReDocCodeSampleAttribute(string language, string source)
                : base(typeof(ReDocCodeSampleAppender), language, source)
            {
            }

            internal class ReDocCodeSampleAppender : IOperationProcessor
            {
                private readonly string _language;
                private readonly string _source;
                private const string ExtensionKey = "x-code-samples";

                public ReDocCodeSampleAppender(string language, string source)
                {
                    _language = language;
                    _source = source;
                }

                public bool Process(OperationProcessorContext context)
                {
                    if (context.OperationDescription.Operation.ExtensionData == null)
                        context.OperationDescription.Operation.ExtensionData = new Dictionary<string, object>();

                    var data = context.OperationDescription.Operation.ExtensionData;
                    if (!data.ContainsKey(ExtensionKey))
                        data[ExtensionKey] = new List<ReDocCodeSample>();

                    var samples = (List<ReDocCodeSample>)data[ExtensionKey];
                    samples.Add(new ReDocCodeSample
                    {
                        Language = _language,
                        Source = _source,
                    });

                    return true;
                }
            }

            internal class ReDocCodeSample
            {
                [JsonProperty("lang")]
                public string Language { get; set; }

                [JsonProperty("source")]
                public string Source { get; set; }
            }
        }

        [TestMethod]
        public async Task When_custom_operation_processor_is_added_via_attribute_then_it_is_processed()
        {
            // Arrange
            var settings = new WebApiOpenApiDocumentGeneratorSettings();
            var generator = new WebApiOpenApiDocumentGenerator(settings);

            // Act
            var document = await generator.GenerateForControllerAsync<TestController>();

            // Assert
            Assert.IsTrue(document.Operations.First().Operation.ExtensionData.ContainsKey("x-code-samples"));
            Assert.AreEqual(2, ((IList)document.Operations.First().Operation.ExtensionData["x-code-samples"]).Count);
        }
    }
}

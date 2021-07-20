using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NSwag.Generation.AspNetCore.Tests.Web.Controllers;
using NSwag.Generation.Processors;
using Xunit;
using Xunit.Abstractions;

namespace NSwag.Generation.AspNetCore.Tests
{
    public class CallbackTests : AspNetCoreTestsBase
    {
        private readonly ITestOutputHelper output;

        public CallbackTests(ITestOutputHelper output) => this.output = output;


        [Fact]
        public async Task When_operation_has_callback_attributes_then_they_are_processed()
        {
            // Arrange
            var settings = new AspNetCoreOpenApiDocumentGeneratorSettings();

            // Act
            var document = await GenerateDocumentAsync(settings, typeof(SwaggerCallbackController));

            //// Assert
            var operation = document.Operations.First().Operation;
            var callbacks = operation.Callbacks;

            /*
                [OpenApiCallback("url1")]
                [OpenApiCallback("url2", "emptyGet", "get")]
                [OpenApiCallback("url3", "single", null, typeof(string))]
                [OpenApiCallback("url4", "multiple", null, typeof(string), typeof(MyPayload))]        
            */

            Assert.Equal(4, callbacks.Count);

            var callback1 = callbacks[operation.OperationId + "_Callback"];
            Assert.Equal("url1", callback1.First().Key);
            Assert.Equal("post", callback1.First().Value.First().Key);
            Assert.Null(callback1.First().Value.First().Value.RequestBody);

            var callback2 = callbacks["emptyGet"];
            Assert.Equal("url2", callback2.First().Key);
            Assert.Equal("get", callback2.First().Value.First().Key);
            Assert.Null(callback2.First().Value.First().Value.RequestBody);

            var callback3 = callbacks["single"];
            Assert.Equal("url3", callback3.First().Key);
            Assert.Equal("post", callback3.First().Value.First().Key);
            Assert.Equal(NJsonSchema.JsonObjectType.String, callback3.First().Value.First().Value.RequestBody.Content["application/json"].Schema.Type);

            var callback4 = callbacks["multiple"];
            Assert.Equal("url4", callback4.First().Key);
            Assert.Equal("post", callback4.First().Value.First().Key);
            var schema = callback4.First().Value.First().Value.RequestBody.Content["application/json"].Schema;           
            // output.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(schema));
            Assert.Equal(NJsonSchema.JsonObjectType.None, schema.Type);
            Assert.Equal(NJsonSchema.JsonObjectType.String, schema.OneOf.First().Type);


            Assert.Equal(NJsonSchema.JsonObjectType.Object, schema.OneOf.Last().Type);
            Assert.Equal("myProperty", schema.OneOf.Last().Properties.First().Key);
            Assert.Equal(NJsonSchema.JsonObjectType.Integer, schema.OneOf.Last().Properties["myProperty"].Type);

        }

        /// <summary>
        /// Custom rule for callback responses: user must answer with 200 or 400 and write the callback name in the body
        /// </summary>
        private class CustomExpectedResponseOperationCallbackProcessor : OperationCallbackProcessor
        {
            public override IDictionary<string, OpenApiResponse> ExpectedResponses(string callbackName)
            {
                return new Dictionary<string, OpenApiResponse> {
                    {
                        "200", new OpenApiResponse
                        {
                            Description = $"Callback {callbackName} was accepted."
                        }
                    },{
                        "400", new OpenApiResponse
                        {
                            Description = $"Callback {callbackName} was rejected."
                        }
                    }
                };
            }
        }


    [Fact]
        public async Task When_callback_expected_responses_are_overridden_they_are_processed()
        {
            // Arrange
            var settings = new AspNetCoreOpenApiDocumentGeneratorSettings();
            var customProcessor = new CustomExpectedResponseOperationCallbackProcessor();
            settings.OperationProcessors.Add(customProcessor);

            // Act
            var document = await GenerateDocumentAsync(settings, typeof(SwaggerCallbackController));

            //// Assert
            var operation = document.Operations.First().Operation;
            var callbacks = operation.Callbacks;
            Func<string, IDictionary<string, OpenApiResponse>> getResponses = (name) => callbacks[name].First().Value.First().Value.Responses;

            /*
                [OpenApiCallback("url1")]
                [OpenApiCallback("url2", "emptyGet", "get")]
                [OpenApiCallback("url3", "single", null, typeof(string))]
                [OpenApiCallback("url4", "multiple", null, typeof(string), typeof(MyPayload))]        
            */

            foreach (var callbackName in new [] { operation.OperationId + "_Callback" , "emptyGet", "single", "multiple"})
            {
                var definedResponses = customProcessor.ExpectedResponses(callbackName);
                var generatedResponses = getResponses(callbackName);
                Assert.Equal(definedResponses.Count, generatedResponses.Count);
                Assert.All(definedResponses, dr => Assert.Equal(dr.Value.Description, generatedResponses[dr.Key].Description));
            }

        }
    }
}

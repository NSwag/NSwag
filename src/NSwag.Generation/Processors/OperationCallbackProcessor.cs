//-----------------------------------------------------------------------
// <copyright file="OperationTagsProcessor.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/RicoSuter/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Namotion.Reflection;
using NSwag.Generation.Processors.Contexts;
using NJsonSchema;

namespace NSwag.Generation.Processors
{
    /// <summary>Processes the OpenApiCallbackAttribute on the operation method.</summary>
    public class OperationCallbackProcessor : IOperationProcessor
    {
        /// <summary>
        /// Processes the specified method information for any callbacks.
        /// </summary>
        /// <param name="context">The processor context.</param>
        /// <returns>Always returns true, since callbacks are optional.</returns>
        public bool Process(OperationProcessorContext context)
        {
            var operation = context.OperationDescription.Operation;
            if (operation.Callbacks == null)
            {
                operation.Callbacks = new Dictionary<string, OpenApiCallback>();
            }

            foreach (var CallbackAttribute in
                    from CallbackAttribute
                    in context.MethodInfo.GetCustomAttributes()
                        .GetAssignableToTypeName("OpenApiCallbackAttribute", TypeNameStyle.Name)
                    select (dynamic)CallbackAttribute)
            {
                string url = CallbackAttribute.CallbackUrl;
                string key = CallbackAttribute.Name ?? (context.OperationDescription.Operation.OperationId + "_Callback");
                string method = CallbackAttribute.Method ?? OpenApiOperationMethod.Post;
                string mimeType = CallbackAttribute.MimeType;
                Type[] types = CallbackAttribute.Types;

                var payloadSchemas = types.Select(t =>
                    context.SchemaGenerator.GenerateWithReferenceAndNullability<JsonSchema>(
                        t.ToContextualType(),
                        context.SchemaResolver
                    )).ToArray();

                var binarySchemas = payloadSchemas.Where(x => x != null && x.IsBinary).ToArray();
                var jsonSchemas = payloadSchemas.Where(x => x != null && !x.IsBinary).ToArray();

                OpenApiRequestBody requestBody;
                

                if (payloadSchemas.Length == 0)
                {
                    requestBody = null;
                }
                else
                {
                    requestBody = new OpenApiRequestBody { IsRequired = true };

                    if (!String.IsNullOrWhiteSpace(mimeType))
                    {
                        requestBody.Content[mimeType] = CreateMediaType(payloadSchemas);
                    }
                    else
                    {
                        var binaryMedia = CreateMediaType(binarySchemas);
                        if (binaryMedia != null)
                        {
                            requestBody.Content["application/octet-stream"] = binaryMedia;
                        }

                        var jsonMedia = CreateMediaType(jsonSchemas);
                        if(jsonMedia != null)
                        {
                            requestBody.Content["application/json"] = jsonMedia;
                        }
                    }
                };

                var callbackOperation = new OpenApiOperation
                {
                    RequestBody = requestBody
                };

                foreach (var expectedResponse in this.ExpectedResponses(key))
                {
                    callbackOperation.Responses.Add(expectedResponse);
                }

                operation.Callbacks[key] =
                    new OpenApiCallback {
                            {
                                url,
                                new OpenApiPathItem
                                {
                                    {
                                        method,
                                        callbackOperation
                                    }
                                }
                            }
                        };
            }

            return true;
        }

        /// <summary>
        /// Defines what responses are expected for a given callback.
        /// <br />The default implementation expects for any callback a 200 response, which has the generic description "Your server returns this code if it accepts the callback".
        /// </summary>
        /// <param name="callbackName">The name of the callback which expects these responses.</param>
        /// <returns>The responses expected by the callback, keyed by status code.</returns>
        public virtual IDictionary<string, OpenApiResponse> ExpectedResponses(string callbackName)
        {
            return new Dictionary<string, OpenApiResponse> {
                {
                    "200", new OpenApiResponse
                    {
                        Description = "Your server returns this code if it accepts the callback"
                    }
                }
            };
        }


        private OpenApiMediaType CreateMediaType(JsonSchema[] schemas)
        {
            if (schemas.Length == 0)
            {
                return null;
            }
            else if (schemas.Length == 1)
            {
                return new OpenApiMediaType { Schema = schemas[0] };
            }
            else
            {
                var parentSchema = new JsonSchema();
                foreach (var schema in schemas)
                {
                    parentSchema.OneOf.Add(schema);
                }
                return new OpenApiMediaType { Schema = parentSchema }; ;
            }
        }
    }
}
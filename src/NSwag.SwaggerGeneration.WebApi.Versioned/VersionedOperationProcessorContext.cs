namespace NSwag.SwaggerGeneration.WebApi.Versioned
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Microsoft.Web.Http.Description;
    using NJsonSchema;
    using NJsonSchema.Generation;
    using NSwag;
    using SwaggerGeneration;
    using SwaggerGeneration.Processors.Contexts;

    public class VersionedOperationProcessorContext : OperationProcessorContext
    {
        public VersionedOperationProcessorContext(
            SwaggerDocument document,
            SwaggerOperationDescription operationDescription,
            Type controllerType,
            MethodInfo methodInfo,
            SwaggerGenerator swaggerGenerator,
            JsonSchemaGenerator schemaGenerator,
            JsonSchemaResolver schemaResolver,
            SwaggerGeneratorSettings settings,
            IList<SwaggerOperationDescription> allOperationDescriptions)
            : base(document, operationDescription, controllerType, methodInfo, swaggerGenerator, schemaGenerator,
                schemaResolver, settings, allOperationDescriptions)
        {
        }

        public VersionedApiDescription ApiDescription { get; set; }
    }
}
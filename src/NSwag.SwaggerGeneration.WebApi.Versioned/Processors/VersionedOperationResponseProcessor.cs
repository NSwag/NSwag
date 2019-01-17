namespace NSwag.SwaggerGeneration.WebApi.Versioned.Processors
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Threading.Tasks;
    using Annotations;
    using NJsonSchema;
    using NJsonSchema.Infrastructure;
    using SwaggerGeneration.Processors;
    using SwaggerGeneration.Processors.Contexts;

    public class VersionedOperationResponseProcessor : OperationResponseProcessorBase, IOperationProcessor
    {
        private readonly VersionedWebApiToSwaggerGeneratorSettings _settings;

        /// <summary>Initializes a new instance of the <see cref="VersionedOperationParameterProcessor"/> class.</summary>
        /// <param name="settings">The settings.</param>
        public VersionedOperationResponseProcessor(VersionedWebApiToSwaggerGeneratorSettings settings)
            : base(settings)
        {
            _settings = settings;
        }

        /// <summary>Processes the specified method information.</summary>
        /// <param name="operationProcessorContext"></param>
        /// <returns>true if the operation should be added to the Swagger specification.</returns>
        public async Task<bool> ProcessAsync(OperationProcessorContext operationProcessorContext)
        {
            if (!(operationProcessorContext is VersionedOperationProcessorContext context))
                return false;

            var parameter = context.MethodInfo.ReturnParameter;

            var responseTypeAttributes = context.MethodInfo.GetCustomAttributes()
                .Where(a => a.GetType().Name == "ResponseTypeAttribute" ||
                            a.GetType().Name == "SwaggerResponseAttribute" ||
                            a.GetType().Name == "SwaggerDefaultResponseAttribute")
                .Concat(context.MethodInfo.DeclaringType.GetTypeInfo().GetCustomAttributes()
                    .Where(a => a.GetType().Name == "SwaggerResponseAttribute" ||
                                a.GetType().Name == "SwaggerDefaultResponseAttribute"))
                .ToList();

            var operation = context.OperationDescription.Operation;


            if (responseTypeAttributes.Count > 0)
            {
                // if SwaggerResponseAttribute \ ResponseTypeAttributes are present, we'll only use those.
                await ProcessResponseTypeAttributes(context, parameter, responseTypeAttributes);
            }
            else
            {
                var returnType = context.ApiDescription.ResponseDescription.ResponseType ?? context.ApiDescription.ResponseDescription.DeclaredType;
                var response = new SwaggerResponse();
                string httpStatusCode;

                if (context.MethodInfo.GetCustomAttribute<SwaggerDefaultResponseAttribute>() != null)
                    httpStatusCode = "default";
                else
                    httpStatusCode = "200";

                var typeDescription = _settings.ReflectionService.GetDescription(
                    returnType, GetParameterAttributes(parameter), _settings);

                if (IsVoidResponse(returnType) == false)
                {
                    response.IsNullableRaw = typeDescription.IsNullable;
                    var testSchema = await context.SchemaGenerator
                        .GenerateWithReferenceAndNullabilityAsync<JsonSchema4>(
                            returnType, null, typeDescription.IsNullable, context.SchemaResolver)
                        .ConfigureAwait(false);
                    if (testSchema.Type == JsonObjectType.File)
                        testSchema.Type = JsonObjectType.Object;
                    response.Schema = testSchema;
                }

                context.OperationDescription.Operation.Responses.Add(httpStatusCode, response);

                if (operation.Produces == null)
                    operation.Produces = new List<string>();

                foreach (var responseFormat in context.ApiDescription.SupportedResponseFormatters.SelectMany(r =>
                    r.SupportedMediaTypes))
                {
                    if (operation.Produces == null)
                        operation.Produces = new List<string>();

                    if (!operation.Produces.Contains(responseFormat.MediaType, StringComparer.OrdinalIgnoreCase))
                    {
                        operation.Produces.Add(responseFormat.MediaType);
                    }
                }
            }

            if (context.OperationDescription.Operation.Responses.Count == 0)
            {
                context.OperationDescription.Operation.Responses[GetVoidResponseStatusCode()] = new SwaggerResponse
                {
                    IsNullableRaw = true,
                    Schema = new JsonSchema4
                    {
                        Type = JsonObjectType.File
                    }
                };
            }

            var successXmlDescription = await parameter.GetDescriptionAsync(GetParameterAttributes(parameter))
                                            .ConfigureAwait(false) ?? string.Empty;

            if (!string.IsNullOrEmpty(successXmlDescription))
            {
                foreach (var response in context.OperationDescription.Operation.Responses
                    .Where(r => HttpUtilities.IsSuccessStatusCode(r.Key)))
                {
                    if (!string.IsNullOrEmpty(response.Value.Description))
                    {
                        response.Value.Description = successXmlDescription;
                    }
                }
            }

            return true;
        }

        /// <summary>Gets the response HTTP status code for an empty/void response and the given generator.</summary>
        /// <returns>The status code.</returns>
        protected override string GetVoidResponseStatusCode()
        {
            return"200";
        }

        private bool IsVoidResponse(Type returnType)
        {
            return returnType == null || returnType.FullName == "System.Void";
        }
    }
}
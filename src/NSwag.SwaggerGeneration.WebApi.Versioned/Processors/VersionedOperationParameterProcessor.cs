namespace NSwag.SwaggerGeneration.WebApi.Versioned.Processors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web.Http.Description;
    using NJsonSchema;
    using NJsonSchema.Generation;
    using NJsonSchema.Infrastructure;
    using SwaggerGeneration.Processors;
    using SwaggerGeneration.Processors.Contexts;

    public class VersionedOperationParameterProcessor : IOperationProcessor
    {
        private readonly VersionedWebApiToSwaggerGeneratorSettings _settings;

        public VersionedOperationParameterProcessor(VersionedWebApiToSwaggerGeneratorSettings settings)
        {
            _settings = settings;
        }

        public async Task<bool> ProcessAsync(OperationProcessorContext operationProcessorContext)
        {
            if (!(operationProcessorContext is VersionedOperationProcessorContext context))
            {
                return false;
            }

            foreach (var parameter in context.ApiDescription.ParameterDescriptions)
            {
                SwaggerParameterKind operationParameterKind = SwaggerParameterKind.Undefined;
                var attributes = parameter.ParameterDescriptor.GetCustomAttributes<Attribute>();

                if (attributes.Any(a => a.GetType().Name == "SwaggerIgnoreAttribute"))
                    continue;

                if (parameter.Source == ApiParameterSource.FromUri)
                {
                    var index = context.OperationDescription.Path.IndexOf("{" + parameter.Name + "}",
                        StringComparison.OrdinalIgnoreCase);

                    if (index > 0 && context.OperationDescription.Path.Substring(index - 1, 1) != "=")
                    {
                        operationParameterKind = SwaggerParameterKind.Path;
                    }
                    else
                    {
                        // Parameters inside parentheses are OData path parameters
                        var odataPaths = Regex.Matches(context.OperationDescription.Path, @"(\([^\)]+\))");
                        var matchedPath = false;
                        foreach (Match match in odataPaths)
                        {
                            var odataIndex = match.Groups[1].Value.IndexOf("{" + parameter.Name + "}",
                                StringComparison.OrdinalIgnoreCase);
                            if (odataIndex > 0)
                            {
                                operationParameterKind = SwaggerParameterKind.Path;
                                matchedPath = true;
                                break;
                            }
                        }

                        if (!matchedPath)
                        {
                            operationParameterKind = SwaggerParameterKind.Query;
                        }
                    }

                    SwaggerParameter operationParameter =
                        await CreatePrimitiveParameterAsync(context, parameter).ConfigureAwait(false);

                    operationParameter.Kind = operationParameterKind;
                    operationParameter.IsNullableRaw = null;
                    context.OperationDescription.Operation.Parameters.Add(operationParameter);
                }
                else if (parameter.Source == ApiParameterSource.FromBody)
                {
                    if (!await TryAddFileParameterAsync(context, parameter).ConfigureAwait(false))
                    {
                        await AddBodyParameterAsync(context, parameter).ConfigureAwait(false);
                    }
                }
            }

            return true;
        }

        private async Task AddBodyParameterAsync(VersionedOperationProcessorContext context,
            ApiParameterDescription parameter)
        {
            var operation = context.OperationDescription.Operation;
            var parameterType = parameter.ParameterDescriptor.ParameterType;
            if (parameterType.Name == "XmlDocument" ||
                parameterType.InheritsFrom("XmlDocument", TypeNameStyle.Name))
            {
                operation.Consumes = new List<string> { "application/xml" };
                operation.Parameters.Add(new SwaggerParameter
                {
                    Name = parameter.Name,
                    Kind = SwaggerParameterKind.Body,
                    Schema = new JsonSchema4 { Type = JsonObjectType.String },
                    IsNullableRaw = true,
                    IsRequired = true,
                    Description = parameter.Documentation
                });
            }
            else
            {
                var typeDescription = _settings.ReflectionService.GetDescription(
                    parameter.ParameterDescriptor.ParameterType,
                    parameter.ParameterDescriptor.GetCustomAttributes<Attribute>(), _settings);

                var operationParameter = new SwaggerParameter
                {
                    Name = parameter.Name,
                    Kind = SwaggerParameterKind.Body,
                    IsRequired = true, // FromBody parameters are always required.
                    IsNullableRaw = typeDescription.IsNullable,
                    Description = parameter.Documentation,
                    Schema = await context.SchemaGenerator.GenerateWithReferenceAndNullabilityAsync<JsonSchema4>(
                        parameter.ParameterDescriptor.ParameterType,
                        parameter.ParameterDescriptor.GetCustomAttributes<Attribute>(), isNullable: false,
                        schemaResolver: context.SchemaResolver).ConfigureAwait(false)
                };

                operationParameter.Schema.Default = parameter.ParameterDescriptor.DefaultValue;

                operation.Parameters.Add(operationParameter);
            }
        }

        private bool IsFileArray(Type type, JsonTypeDescription typeInfo)
        {
            var isFormFileCollection = type.Name == "IFormFileCollection";
            var isFileArray = typeInfo.Type == JsonObjectType.Array && type.GenericTypeArguments.Any() &&
                              _settings.ReflectionService.GetDescription(type.GenericTypeArguments[0], null, _settings)
                                  .Type == JsonObjectType.File;
            return isFormFileCollection || isFileArray;
        }

        private async Task<bool> TryAddFileParameterAsync(
            OperationProcessorContext context, ApiParameterDescription parameter)
        {
            var attributes = (IEnumerable<Attribute>) parameter.ParameterDescriptor.GetCustomAttributes<Attribute>();
            var info = _settings.ReflectionService.GetDescription(parameter.ParameterDescriptor.ParameterType,
                attributes, _settings);

            var isFileArray = IsFileArray(parameter.ParameterDescriptor.ParameterType, info);

            attributes = attributes
                .Union(parameter.ParameterDescriptor.ParameterType.GetTypeInfo().GetCustomAttributes());

            var hasSwaggerFileAttribute = attributes.Any(a =>
                a.GetType().IsAssignableTo("SwaggerFileAttribute", TypeNameStyle.Name));

            if (info.Type == JsonObjectType.File || hasSwaggerFileAttribute || isFileArray)
            {
                await AddFileParameterAsync(context, parameter, isFileArray).ConfigureAwait(false);
                return true;
            }

            return false;
        }

        private async Task AddFileParameterAsync(OperationProcessorContext context,
            ApiParameterDescription parameter, bool isFileArray)
        {
            var operationParameter = await CreatePrimitiveParameterAsync(context, parameter).ConfigureAwait(false);
            InitializeFileParameter(operationParameter, isFileArray);

            context.OperationDescription.Operation.Parameters.Add(operationParameter);
        }

        private void InitializeFileParameter(SwaggerParameter operationParameter, bool isFileArray)
        {
            operationParameter.Type = JsonObjectType.File;
            operationParameter.Kind = SwaggerParameterKind.FormData;

            if (isFileArray)
                operationParameter.CollectionFormat = SwaggerParameterCollectionFormat.Multi;
        }


        private async Task<SwaggerParameter> CreatePrimitiveParameterAsync(
            OperationProcessorContext context,
            ApiParameterDescription parameter)
        {
            var operationParameter = await context.SwaggerGenerator.CreatePrimitiveParameterAsync(
                parameter.Name,
                parameter.Documentation,
                parameter.ParameterDescriptor.ParameterType,
                parameter.ParameterDescriptor.GetCustomAttributes<Attribute>());

            operationParameter.IsRequired = !parameter.ParameterDescriptor.IsOptional;
            return operationParameter;
        }
    }
}
//-----------------------------------------------------------------------
// <copyright file="TypeScriptOperationModel.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/NSwag/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using NJsonSchema;
using NJsonSchema.CodeGeneration;
using NSwag.CodeGeneration.Models;

namespace NSwag.CodeGeneration.TypeScript.Models
{
    /// <summary>The TypeScript operation model.</summary>
    public class TypeScriptOperationModel : OperationModelBase<TypeScriptParameterModel, TypeScriptResponseModel>
    {
        private readonly SwaggerToTypeScriptClientGeneratorSettings _settings;
        private readonly SwaggerToTypeScriptClientGenerator _generator;
        private readonly SwaggerOperation _operation;

        /// <summary>Initializes a new instance of the <see cref="TypeScriptOperationModel" /> class.</summary>
        /// <param name="operation">The operation.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="generator">The generator.</param>
        /// <param name="resolver">The resolver.</param>
        public TypeScriptOperationModel(
            SwaggerOperation operation,
            SwaggerToTypeScriptClientGeneratorSettings settings,
            SwaggerToTypeScriptClientGenerator generator,
            TypeResolverBase resolver)
            : base(null, operation, resolver, generator, settings)
        {
            _operation = operation;
            _settings = settings;
            _generator = generator;

            var parameters = _operation.ActualParameters.ToList();
            if (settings.GenerateOptionalParameters)
            {
                parameters = parameters
                    .OrderBy(p => p.Position ?? 0)
                    .OrderBy(p => !p.IsRequired)
                    .ToList();
            }

            Parameters = parameters.Select(parameter =>
                new TypeScriptParameterModel(parameter.Name,
                    GetParameterVariableName(parameter, _operation.Parameters), ResolveParameterType(parameter),
                    parameter, parameters, _settings, _generator, resolver))
                .ToList();
        }

        /// <summary>Gets the actual name of the operation (language specific).</summary>
        public override string ActualOperationName => ConversionUtilities.ConvertToLowerCamelCase(OperationName, false)
            + (MethodAccessModifier == "protected " ? "Core" : string.Empty);

        /// <summary>Gets the actual name of the operation (language specific).</summary>
        public string ActualOperationNameUpper => ConversionUtilities.ConvertToUpperCamelCase(OperationName, false);

        /// <summary>Gets or sets the type of the result.</summary>
        public override string ResultType
        {
            get
            {
                var response = GetSuccessResponse();
                var isNullable = response?.IsNullable(_settings.CodeGeneratorSettings.SchemaType) == true;

                var resultType = isNullable && SupportsStrictNullChecks && !_settings.UseAngularResultProcessing && UnwrappedResultType != "void" && UnwrappedResultType != "null" ?
                    UnwrappedResultType + " | null" :
                    UnwrappedResultType;

                if (WrapResponse)
                {
                    return _settings.ResponseClass.Replace("{controller}", ControllerName) + "<" + resultType + ">";
                }
                else
                {
                    return resultType;
                }
            }
        }

        /// <summary>Gets a value indicating whether the operation requires mappings for DTO generation.</summary>
        public bool RequiresMappings => Responses.Any(r => r.HasType && r.ActualResponseSchema.UsesComplexObjectSchema());

        /// <summary>Gets a value indicating whether the target TypeScript version supports strict null checks.</summary>
        public bool SupportsStrictNullChecks => _settings.TypeScriptGeneratorSettings.TypeScriptVersion >= 2.0m;

        /// <summary>Gets a value indicating whether to handle references.</summary>
        public bool HandleReferences => _settings.TypeScriptGeneratorSettings.HandleReferences;

        /// <summary>Gets a value indicating whether the template can request blobs.</summary>
        public bool CanRequestBlobs => !IsJQuery;

        /// <summary>Gets a value indicating whether to use blobs with Angular.</summary>
        public bool RequestAngularBlobs => IsAngular && IsFile;

        /// <summary>Gets a value indicating whether to use blobs with AngularJS.</summary>
        public bool RequestAngularJSBlobs => IsAngularJS && IsFile;

        /// <summary>Gets a value indicating whether to render for AngularJS.</summary>
        public bool IsAngularJS => _settings.Template == TypeScriptTemplate.AngularJS;

        /// <summary>Gets a value indicating whether to render for Angular2.</summary>
        public bool IsAngular => _settings.Template == TypeScriptTemplate.Angular;

        /// <summary>Gets a value indicating whether to render for JQuery.</summary>
        public bool IsJQuery => _settings.Template == TypeScriptTemplate.JQueryCallbacks ||
                                _settings.Template == TypeScriptTemplate.JQueryPromises;

        /// <summary>Gets a value indicating whether to render for Fetch or Aurelia</summary>
        public bool IsFetchOrAurelia => _settings.Template == TypeScriptTemplate.Fetch ||
                                        _settings.Template == TypeScriptTemplate.Aurelia;

        /// <summary>Gets a value indicating whether to use HttpClient with the Angular template.</summary>
        public bool UseAngularHttpClient => IsAngular && _settings.HttpClass == HttpClass.HttpClient;

        /// <summary>Gets or sets the type of the exception.</summary>
        public override string ExceptionType
        {
            get
            {
                if (_operation.ActualResponses.Count(r => !HttpUtilities.IsSuccessStatusCode(r.Key)) == 0)
                    return "string";

                return string.Join(" | ", _operation.ActualResponses
                    .Where(r => !HttpUtilities.IsSuccessStatusCode(r.Key) && r.Value.ActualResponseSchema != null)
                    .Select(r => _generator.GetTypeName(r.Value.ActualResponseSchema, r.Value.IsNullable(_settings.CodeGeneratorSettings.SchemaType), "Exception"))
                    .Concat(new[] { "string" }));
            }
        }

        /// <summary>Gets the method's access modifier.</summary>
        public string MethodAccessModifier
        {
            get
            {
                var controllerName = _settings.GenerateControllerName(ControllerName);
                if (_settings.ProtectedMethods?.Contains(controllerName + "." + ConversionUtilities.ConvertToLowerCamelCase(OperationName, false)) == true)
                    return "protected ";

                return "";
            }
        }

        /// <summary>Gets a value indicating whether to wrap success responses to allow full response access.</summary>
        public bool WrapResponses => _settings.WrapResponses;

        /// <summary>Gets the response class name.</summary>
        public string ResponseClass => _settings.ResponseClass.Replace("{controller}", ControllerName);

        /// <summary>Resolves the type of the parameter.</summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The parameter type name.</returns>
        protected override string ResolveParameterType(SwaggerParameter parameter)
        {
            var schema = parameter.ActualSchema;
            if (schema.Type == JsonObjectType.File)
            {
                if (parameter.CollectionFormat == SwaggerParameterCollectionFormat.Multi && !schema.Type.HasFlag(JsonObjectType.Array))
                    return "FileParameter[]";

                return "FileParameter";
            }

            return base.ResolveParameterType(parameter);
        }

        /// <summary>Creates the response model.</summary>
        /// <param name="statusCode">The status code.</param>
        /// <param name="response">The response.</param>
        /// <param name="exceptionSchema">The exception schema.</param>
        /// <param name="generator">The generator.</param>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        protected override TypeScriptResponseModel CreateResponseModel(string statusCode, SwaggerResponse response,
            JsonSchema4 exceptionSchema, IClientGenerator generator, ClientGeneratorBaseSettings settings)
        {
            return new TypeScriptResponseModel(this, statusCode, response, response == GetSuccessResponse(),
                exceptionSchema, generator, (SwaggerToTypeScriptClientGeneratorSettings)settings);
        }
    }
}

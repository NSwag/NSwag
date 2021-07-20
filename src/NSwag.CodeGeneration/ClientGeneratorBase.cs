//-----------------------------------------------------------------------
// <copyright file="ClientGeneratorBase.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/RicoSuter/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using NJsonSchema;
using NJsonSchema.CodeGeneration;
using NSwag.CodeGeneration.Models;

namespace NSwag.CodeGeneration
{
    /// <summary>The client generator base.</summary>
    /// <typeparam name="TOperationModel">The type of the operation model.</typeparam>
    /// <typeparam name="TParameterModel">The type of the parameter model.</typeparam>
    /// <typeparam name="TResponseModel">The type of the response model.</typeparam>
    /// <seealso cref="IClientGenerator" />
    public abstract class ClientGeneratorBase<TOperationModel, TParameterModel, TResponseModel> : IClientGenerator
        where TOperationModel : OperationModelBase<TParameterModel, TResponseModel>
        where TResponseModel : ResponseModelBase
        where TParameterModel : ParameterModelBase
    {
        private readonly OpenApiDocument _document;

        /// <summary>Initializes a new instance of the <see cref="ClientGeneratorBase{TOperationModel, TParameterModel, TResponseModel}"/> class.</summary>
        /// <param name="document">The document.</param>
        /// <param name="settings">The code generator settings.</param>
        /// <param name="resolver">The type resolver.</param>
        protected ClientGeneratorBase(OpenApiDocument document, CodeGeneratorSettingsBase settings, TypeResolverBase resolver)
        {
            _document = document;
            Resolver = resolver;

            settings.SchemaType = document.SchemaType; // enforce Swagger schema output
        }

        /// <summary>Gets the base settings.</summary>
        public abstract ClientGeneratorBaseSettings BaseSettings { get; }

        /// <summary>Gets the type resolver.</summary>
        protected TypeResolverBase Resolver { get; }

        /// <summary>Gets the type.</summary>
        /// <param name="schema">The schema.</param>
        /// <param name="isNullable">Specifies whether the type is nullable..</param>
        /// <param name="typeNameHint">The type name hint.</param>
        /// <returns>The type name.</returns>
        public abstract string GetTypeName(JsonSchema schema, bool isNullable, string typeNameHint);

        /// <summary>Gets the file response type name.</summary>
        /// <returns>The type name.</returns>
        public virtual string GetBinaryResponseTypeName()
        {
            return "FileResponse";
        }

        /// <summary>Generates the the whole file containing all needed types.</summary>
        /// <returns>The code</returns>
        public string GenerateFile()
        {
            return GenerateFile(ClientGeneratorOutputType.Full);
        }

        /// <summary>Generates the the whole file containing all needed types.</summary>
        /// <param name="outputType">Type of the output.</param>
        /// <returns>The code</returns>
        public string GenerateFile(ClientGeneratorOutputType outputType)
        {
            if (!OutputRequiresRefresh(BaseSettings.OutputFilePath))
            {
                // just give back the original file as it hasn't changed
                return File.ReadAllText(BaseSettings.OutputFilePath);
            }

            var clientTypes = GenerateAllClientTypes();

            var dtoTypes = BaseSettings.GenerateDtoTypes ?
                GenerateDtoTypes() :
                Enumerable.Empty<CodeArtifact>();

            clientTypes =
                outputType == ClientGeneratorOutputType.Full ? clientTypes :
                outputType == ClientGeneratorOutputType.Implementation ? clientTypes.Where(t => t.Category != CodeArtifactCategory.Contract) :
                outputType == ClientGeneratorOutputType.Contracts ? clientTypes.Where(t => t.Category == CodeArtifactCategory.Contract) :
                Enumerable.Empty<CodeArtifact>();

            dtoTypes =
                outputType == ClientGeneratorOutputType.Full ||
                outputType == ClientGeneratorOutputType.Contracts ? dtoTypes : Enumerable.Empty<CodeArtifact>();

            return GenerateFile(clientTypes, dtoTypes, outputType)
                .Replace("\r", string.Empty)
                .Replace("\n\n\n\n", "\n\n")
                .Replace("\n\n\n", "\n\n");
        }

        private bool OutputRequiresRefresh(string outputFilePath)
        {
            if (!BaseSettings.ChecksumCacheEnabled || string.IsNullOrWhiteSpace(outputFilePath) || !File.Exists(outputFilePath))
            {
                // no point in checking
                return true;
            }

            var sourceDocumentChecksum = _document.GetChecksum();
            if (string.IsNullOrWhiteSpace(sourceDocumentChecksum))
            {
                return true;
            }

            // if we can match both source checksum and toolchain, we can presume file hasn't been changed
            var checksumMatches = false;
            var toolchainVersionMatches = false;
            const string checksumMarker = "SourceSHA: ";
            const string toolchainVersionMarker = "toolchain v";

            try
            {
                using (var reader = new StreamReader(new FileStream(outputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    // read multiple lines as we can have headers before version info
                    var counter = 10;
                    string line;
                    while ((line = reader.ReadLine()) != null && counter > 0)
                    {
                        counter--;
                        var checksumStartIndex = line.IndexOf(checksumMarker);
                        if (checksumStartIndex > -1)
                        {
                            var startIndex = checksumStartIndex + checksumMarker.Length;
                            var fileSourceSha = line.Substring(startIndex).Trim();
                            checksumMatches = fileSourceSha == sourceDocumentChecksum;
                        }

                        var toolchainStartIndex = line.IndexOf(toolchainVersionMarker);
                        if (toolchainStartIndex > -1)
                        {
                            var startIndex = toolchainStartIndex + toolchainVersionMarker.Length;
                            var length = line.IndexOf(" ", startIndex) - startIndex;
                            var fileToolchainVersion = line.Substring(startIndex, length).Trim();
                            toolchainVersionMatches = fileToolchainVersion == OpenApiDocument.ToolchainVersion;
                        }

                        if (checksumMatches && toolchainVersionMatches)
                        {
                            // we are good to go with cached version, no need to refresh
                            return false;
                        }
                    }
                }
            }
            catch
            {
                // no-go
            }

            return true;
        }

        /// <summary>Generates the file.</summary>
        /// <param name="clientTypes">The client types.</param>
        /// <param name="dtoTypes">The DTO types.</param>
        /// <param name="outputType">Type of the output.</param>
        /// <returns>The code.</returns>
        protected abstract string GenerateFile(IEnumerable<CodeArtifact> clientTypes, IEnumerable<CodeArtifact> dtoTypes, ClientGeneratorOutputType outputType);

        /// <summary>Generates the client types.</summary>
        /// <returns>The code artifact collection.</returns>
        protected virtual IEnumerable<CodeArtifact> GenerateAllClientTypes()
        {
            var operations = GetOperations(_document);
            var clientTypes = new List<CodeArtifact>();

            if (BaseSettings.OperationNameGenerator.SupportsMultipleClients)
            {
                var controllerOperationsGroups = operations.GroupBy(o => o.ControllerName).ToList();
                foreach (var controllerOperations in controllerOperationsGroups)
                {
                    var controllerName = controllerOperations.Key;
                    var controllerClassName = BaseSettings.GenerateControllerName(controllerOperations.Key);
                    var clientType = GenerateClientTypes(controllerName, controllerClassName, controllerOperations.ToList());
                    clientTypes.AddRange(clientType);
                }
            }
            else
            {
                var controllerName = string.Empty;
                var controllerClassName = BaseSettings.GenerateControllerName(controllerName);
                var clientType = GenerateClientTypes(controllerName, controllerClassName, operations);
                clientTypes.AddRange(clientType);
            }

            return clientTypes;
        }

        /// <summary>Generates the client class.</summary>
        /// <param name="controllerName">Name of the controller.</param>
        /// <param name="controllerClassName">Name of the controller class.</param>
        /// <param name="operations">The operations.</param>
        /// <returns>The code.</returns>
        protected abstract IEnumerable<CodeArtifact> GenerateClientTypes(string controllerName, string controllerClassName, IEnumerable<TOperationModel> operations);

        /// <summary>Generates all DTO types.</summary>
        /// <returns>The code artifact collection.</returns>
        protected abstract IEnumerable<CodeArtifact> GenerateDtoTypes();

        /// <summary>Creates an operation model.</summary>
        /// <param name="operation">The operation.</param>
        /// <param name="settings">The settings.</param>
        /// <returns>The operation model.</returns>
        protected abstract TOperationModel CreateOperationModel(OpenApiOperation operation, ClientGeneratorBaseSettings settings);

        private List<TOperationModel> GetOperations(OpenApiDocument document)
        {
            document.GenerateOperationIds();

            return document.Paths
                .SelectMany(pair => pair.Value.ActualPathItem
                    .Select(p => new { Path = pair.Key.TrimStart('/'), HttpMethod = p.Key, Operation = p.Value }))
                .Select(tuple =>
                {
                    var operationName = BaseSettings.OperationNameGenerator.GetOperationName(document, tuple.Path, tuple.HttpMethod, tuple.Operation);

                    if (operationName.Contains("."))
                    {
                        operationName = operationName.Replace(".", "_");
                    }

                    if (operationName.EndsWith("Async"))
                    {
                        operationName = operationName.Substring(0, operationName.Length - "Async".Length);
                    }
                    var controllerName = BaseSettings.OperationNameGenerator.GetClientName(document, tuple.Path, tuple.HttpMethod, tuple.Operation);

                    operationName = CSharpIdentifierValidator.RemoveInvalidIdentifierCharacters(operationName);
                    controllerName = CSharpIdentifierValidator.RemoveInvalidIdentifierCharacters(controllerName);

                    var operationModel = CreateOperationModel(tuple.Operation, BaseSettings);
                    operationModel.ControllerName = controllerName;
                    operationModel.Path = tuple.Path;
                    operationModel.HttpMethod = tuple.HttpMethod;
                    operationModel.OperationName = operationName;
                    return operationModel;
                })
                .ToList();
        }
    }
}

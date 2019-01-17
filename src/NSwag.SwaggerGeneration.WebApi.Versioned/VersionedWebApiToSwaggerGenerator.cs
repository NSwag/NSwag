namespace NSwag.SwaggerGeneration.WebApi.Versioned
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Remoting.Contexts;
    using System.Runtime.Remoting.Messaging;
    using System.Threading.Tasks;
    using System.Web.Http.Controllers;
    using System.Web.Http.Description;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Description;
    using NJsonSchema.Infrastructure;
    using Processors;
    using SwaggerGeneration.Processors;
    using SwaggerGeneration.Processors.Contexts;

    public class VersionedWebApiToSwaggerGenerator
    {
        private readonly SwaggerJsonSchemaGenerator _schemaGenerator;
        private readonly VersionedApiExplorer _explorer;

        public VersionedWebApiToSwaggerGenerator(VersionedApiExplorer explorer,
            VersionedWebApiToSwaggerGeneratorSettings settings)
            : this(explorer, settings, new SwaggerJsonSchemaGenerator(settings))
        {
        }

        public VersionedWebApiToSwaggerGenerator(VersionedApiExplorer explorer,
            VersionedWebApiToSwaggerGeneratorSettings settings, SwaggerJsonSchemaGenerator schemaGenerator)
        {
            Settings = settings;
            _schemaGenerator = schemaGenerator;
            _explorer = explorer;
        }

        public VersionedWebApiToSwaggerGeneratorSettings Settings { get; }

        public async Task<SwaggerDocument> GenerateAsync()
        {
            var controllerGroups = _explorer.ApiDescriptions
                .Where(g => Settings.ApiVersions.Contains(g.ApiVersion))
                .SelectMany(g => g.ApiDescriptions)
                .GroupBy(a => a.ActionDescriptor.ControllerDescriptor);
            var document = await CreateDocumentAsync().ConfigureAwait(false);
            var schemaResolver = new SwaggerSchemaResolver(document, Settings);

            var usedControllerTypes = new List<Type>();
            var controllerTypes = new List<Type>();
            foreach (var controllerGroup in controllerGroups)
            {
                var controllerDescriptor = controllerGroup.Key;
                var apiDescriptions = controllerGroup.ToList();
                var generator = new SwaggerGenerator(_schemaGenerator, Settings, schemaResolver);
                var isIncluded =
                    await GenerateForControllerAsync(document, controllerDescriptor, apiDescriptions, generator,
                        schemaResolver).ConfigureAwait(false);
                if (isIncluded)
                    usedControllerTypes.Add(controllerDescriptor.ControllerType);
                controllerTypes.Add(controllerDescriptor.ControllerType);
            }

            document.GenerateOperationIds();

            foreach (var processor in Settings.DocumentProcessors)
                await processor.ProcessAsync(new DocumentProcessorContext(document, controllerTypes,
                    usedControllerTypes, schemaResolver, _schemaGenerator, Settings));

            return document;
        }

        private async Task<SwaggerDocument> CreateDocumentAsync()
        {
            var document = !string.IsNullOrEmpty(Settings.DocumentTemplate)
                ? await SwaggerDocument.FromJsonAsync(Settings.DocumentTemplate).ConfigureAwait(false)
                : new SwaggerDocument();

            document.SchemaType = Settings.SchemaType;

            document.Info = new SwaggerInfo
            {
                Title = Settings.Title,
                Description = Settings.Description,
                Version = Settings.Version,
            };

            return document;
        }

        private async Task<bool> GenerateForControllerAsync(SwaggerDocument document,
            HttpControllerDescriptor controller,
            List<VersionedApiDescription> apiDescriptions, SwaggerGenerator swaggerGenerator,
            SwaggerSchemaResolver schemaResolver)
        {
            var hasIgnoreAttribute = controller.GetCustomAttributes<Attribute>()
                .Any(x => x.GetType().Name == "SwaggerIgnoreAttribute");
            if (hasIgnoreAttribute)
                return false;

            var operations = new List<Tuple<SwaggerOperationDescription, VersionedApiDescription>>();
            foreach (var apiDescription in apiDescriptions)
            {
                if (apiDescription.ActionDescriptor.GetCustomAttributes<Attribute>()
                    .Any(x => x.GetType().Name == "SwaggerIgnoreAttribute"))
                    continue;

                var path = apiDescription.RelativePath;
                if (!path.StartsWith("/", StringComparison.Ordinal))
                    path = "/" + path;

                var operationDescription = new SwaggerOperationDescription
                {
                    Path = path.Split('?')[0],
                    Method = apiDescription.HttpMethod.Method,
                    Operation = new SwaggerOperation
                    {
                        IsDeprecated = apiDescription.ActionDescriptor.GetCustomAttributes<ObsoleteAttribute>().Any(),
                        OperationId = apiDescription.GetUniqueID()
                    }
                };
                operations.Add(
                    new Tuple<SwaggerOperationDescription, VersionedApiDescription>(operationDescription,
                        apiDescription));
            }

            return await AddOperationDescriptionsToDocumentAsync(document, operations, swaggerGenerator, schemaResolver)
                .ConfigureAwait(false);
        }

        private async Task<bool> AddOperationDescriptionsToDocumentAsync(SwaggerDocument document,
            List<Tuple<SwaggerOperationDescription, VersionedApiDescription>> operations,
            SwaggerGenerator swaggerGenerator, SwaggerSchemaResolver schemaResolver)
        {
            var addedOperations = 0;
            var allOperations = operations.Select(t => t.Item1).ToList();

            foreach (var tuple in operations)
            {
                var operation = tuple.Item1;
                var apiDescription = tuple.Item2;

                var addOperation = await RunOperationProcessorsAsync(document, apiDescription, operation, allOperations,
                    swaggerGenerator, schemaResolver).ConfigureAwait(false);
                if (addOperation)
                {
                    var path = operation.Path.Replace("//", "/")
                        .Replace("{version}", apiDescription.ApiVersion.ToString());

                    if (!document.Paths.ContainsKey(path))
                        document.Paths[path] = new SwaggerPathItem();

                    if (document.Paths[path].ContainsKey(operation.Method))
                    {
                        throw new InvalidOperationException(
                            $"The method '{operation.Method}' on path '{path}' is registered multiple times.");
                    }

                    document.Paths[path][operation.Method] = operation.Operation;
                    addedOperations++;
                }
            }

            return addedOperations > 0;
        }

        private async Task<bool> RunOperationProcessorsAsync(SwaggerDocument document,
            VersionedApiDescription apiDescription,
            SwaggerOperationDescription operationDescription, List<SwaggerOperationDescription> allOperations,
            SwaggerGenerator swaggerGenerator, SwaggerSchemaResolver schemaResolver)
        {
            var controllerType = apiDescription.ActionDescriptor.ControllerDescriptor.ControllerType;
            var actionDescriptor = apiDescription.ActionDescriptor as ReflectedHttpActionDescriptor;
            var context = new VersionedOperationProcessorContext(document, operationDescription, controllerType,
                actionDescriptor.MethodInfo, swaggerGenerator, _schemaGenerator, schemaResolver, Settings,
                allOperations)
            {
                ApiDescription = apiDescription
            };

            foreach (var operationProcessor in Settings.OperationProcessors)
            {
                if (!await operationProcessor.ProcessAsync(context))
                    return false;
            }

            var operationProcessorAttributes = controllerType.GetCustomAttributes()
                .Concat(actionDescriptor.MethodInfo.GetCustomAttributes()).Where(a =>
                    a.GetType().IsAssignableTo("SwaggerOperationProcessorAttribute", TypeNameStyle.Name));

            foreach (dynamic attribute in operationProcessorAttributes)
            {
                var operationProcessor = ReflectionExtensions.HasProperty(attribute, "Parameters")
                    ? (IOperationProcessor) Activator.CreateInstance(attribute.Type, attribute.Parameters)
                    : (IOperationProcessor) Activator.CreateInstance(attribute.Type);

                if (!await operationProcessor.ProcessAsync(context))
                    return false;
            }

            return true;
        }
    }
}
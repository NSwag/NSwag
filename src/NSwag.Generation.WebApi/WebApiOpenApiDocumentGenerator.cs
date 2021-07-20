//-----------------------------------------------------------------------
// <copyright file="WebApiToSwaggerGenerator.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/RicoSuter/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Namotion.Reflection;
using NJsonSchema;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using NSwag.Generation.WebApi.Infrastructure;

namespace NSwag.Generation.WebApi
{
    /// <summary>Generates a <see cref="OpenApiDocument"/> object for the given Web API class type. </summary>
    public class WebApiOpenApiDocumentGenerator
    {
        /// <summary>Initializes a new instance of the <see cref="WebApiOpenApiDocumentGenerator" /> class.</summary>
        /// <param name="settings">The settings.</param>
        public WebApiOpenApiDocumentGenerator(WebApiOpenApiDocumentGeneratorSettings settings)
        {
            Settings = settings;
        }

        /// <summary>Gets all controller class types of the given assembly.</summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>The controller classes.</returns>
        public static IEnumerable<Type> GetControllerClasses(Assembly assembly)
        {
            // TODO: Move to IControllerClassLoader interface
            return assembly.ExportedTypes
                .Where(t => t.GetTypeInfo().IsAbstract == false)
                .Where(t => t.Name.EndsWith("Controller") ||
                            t.InheritsFromTypeName("ApiController", TypeNameStyle.Name) ||
                            t.InheritsFromTypeName("ControllerBase", TypeNameStyle.Name)) // in ASP.NET Core, a Web API controller inherits from Controller
                .Where(t => t.GetTypeInfo().ImplementedInterfaces.All(i => i.FullName != "System.Web.Mvc.IController")) // no MVC controllers (legacy ASP.NET)
                .Where(t =>
                {
                    return t.GetTypeInfo().GetCustomAttributes()
                        .SingleOrDefault(a => a.GetType().Name == "ApiExplorerSettingsAttribute")?
                        .TryGetPropertyValue("IgnoreApi", false) != true;

                });
        }

        /// <summary>Gets or sets the generator settings.</summary>
        public WebApiOpenApiDocumentGeneratorSettings Settings { get; }

        /// <summary>Generates a Swagger specification for the given controller type.</summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <returns>The <see cref="OpenApiDocument" />.</returns>
        /// <exception cref="InvalidOperationException">The operation has more than one body parameter.</exception>
        public Task<OpenApiDocument> GenerateForControllerAsync<TController>()
        {
            return GenerateForControllersAsync(new[] { typeof(TController) });
        }

        /// <summary>Generates a Swagger specification for the given controller type.</summary>
        /// <param name="controllerType">The type of the controller.</param>
        /// <returns>The <see cref="OpenApiDocument" />.</returns>
        /// <exception cref="InvalidOperationException">The operation has more than one body parameter.</exception>
        public Task<OpenApiDocument> GenerateForControllerAsync(Type controllerType)
        {
            return GenerateForControllersAsync(new[] { controllerType });
        }

        /// <summary>Generates a Swagger specification for the given controller types.</summary>
        /// <param name="controllerTypes">The types of the controller.</param>
        /// <returns>The <see cref="OpenApiDocument" />.</returns>
        /// <exception cref="InvalidOperationException">The operation has more than one body parameter.</exception>
        public async Task<OpenApiDocument> GenerateForControllersAsync(IEnumerable<Type> controllerTypes)
        {
            var document = await CreateDocumentAsync().ConfigureAwait(false);
            var schemaResolver = new OpenApiSchemaResolver(document, Settings);

            var usedControllerTypes = new List<Type>();
            foreach (var controllerType in controllerTypes)
            {
                var generator = new OpenApiDocumentGenerator(Settings, schemaResolver);
                var isIncluded = GenerateForController(document, controllerType, generator, schemaResolver);
                if (isIncluded)
                {
                    usedControllerTypes.Add(controllerType);
                }
            }

            document.GenerateOperationIds();

            foreach (var processor in Settings.DocumentProcessors)
            {
                processor.Process(new DocumentProcessorContext(document, controllerTypes,
                    usedControllerTypes, schemaResolver, Settings.SchemaGenerator, Settings));
            }

            AddNamespaceToDuplicateTypeNames(document);

            return document;
        }

        private void AddNamespaceToDuplicateTypeNames(OpenApiDocument document)
        {
            var duplicateDefinitionGroups = document.DefinitionsMap.GroupBy(m => m.AttemptedTypeName).Where(g => g.Count() > 1).ToList();

            foreach (var duplicateDefinitionGroup in duplicateDefinitionGroups)
            {
                var typeNameWrappers = duplicateDefinitionGroup.Select(i => new TypeNameWrapper()
                {
                    fullName = i.Type.FullName,
                    parts = i.Type.FullName.Split('.').Reverse().ToList(),
                    differentDepth = 0
                })
                .ToList();


                FindUncommonPart(typeNameWrappers, 1);

                foreach (var definition in duplicateDefinitionGroup)
                {
                    StringBuilder typeNameSB = new StringBuilder();

                    var correspondingTypeNameWrappers = typeNameWrappers.Where(p => p.fullName == definition.Type.FullName).Single();

                    for (int i = correspondingTypeNameWrappers.differentDepth; i >= 0; i--)
                    {
                        if (correspondingTypeNameWrappers.depthsToIgnore.Contains(i))
                        {
                            continue;
                        }

                        typeNameSB.Append(correspondingTypeNameWrappers.parts[i]);
                        if (i != 0)
                        {
                            typeNameSB.Append("_");
                        }
                    }

                    var schema = document.Definitions[definition.GeneratedTypeName];
                    document.Definitions.Remove(definition.GeneratedTypeName);
                    document.Definitions.Add(typeNameSB.ToString(), schema);
                }
            }
        }

        private void FindUncommonPart(List<TypeNameWrapper> typeNameWrappers, int depth)
        {
            var sameDepthParts = typeNameWrappers.Select(w => w.parts[depth]).ToList();

            //If all parts at that depth are the same, ignore them
            if (sameDepthParts.Distinct().Count() == 1)
            {
                foreach (var typeNameWrapper in typeNameWrappers)
                {
                    typeNameWrapper.depthsToIgnore.Add(depth);
                }

            }

            //If a part does not exist in other paths, this is the desired depth for that path
            foreach (var typeNameWrapper in typeNameWrappers)
            {
                if (sameDepthParts.Where(part => part == typeNameWrapper.parts[depth]).Count() == 1)
                {
                    typeNameWrapper.differentDepth = depth;
                }
            }

            //If this is the end of the namespace for a type, assign the last part
            foreach (var typeNameWrapper in typeNameWrappers)
            {
                if(typeNameWrapper.parts.Count == depth + 1)
                {
                    typeNameWrapper.differentDepth = depth;
                    typeNameWrapper.depthsToIgnore.Remove(depth);
                }
            }

            //Only continue with paths that are not assigned a desired depth
            var eligiblePaths = typeNameWrappers.Where(p => p.differentDepth == 0).ToList();

            if (eligiblePaths.Any())
            {
                FindUncommonPart(eligiblePaths, depth + 1);
            }
        }

        private class TypeNameWrapper
        {
            public string fullName;
            public List<string> parts;
            public int differentDepth;
            public List<int> depthsToIgnore = new List<int>();
        }

        private async Task<OpenApiDocument> CreateDocumentAsync()
        {
            var document = !string.IsNullOrEmpty(Settings.DocumentTemplate) ?
                await OpenApiDocument.FromJsonAsync(Settings.DocumentTemplate).ConfigureAwait(false) :
                new OpenApiDocument();

            document.Generator = "NSwag v" + OpenApiDocument.ToolchainVersion + " (NJsonSchema v" + JsonSchema.ToolchainVersion + ")";
            document.SchemaType = Settings.SchemaType;

            document.Consumes = new List<string> { "application/json" };
            document.Produces = new List<string> { "application/json" };

            if (document.Info == null)
            {
                document.Info = new OpenApiInfo();
            }

            if (string.IsNullOrEmpty(Settings.DocumentTemplate))
            {
                if (!string.IsNullOrEmpty(Settings.Title))
                {
                    document.Info.Title = Settings.Title;
                }

                if (!string.IsNullOrEmpty(Settings.Description))
                {
                    document.Info.Description = Settings.Description;
                }

                if (!string.IsNullOrEmpty(Settings.Version))
                {
                    document.Info.Version = Settings.Version;
                }
            }

            return document;
        }

        /// <exception cref="InvalidOperationException">The operation has more than one body parameter.</exception>
        private bool GenerateForController(OpenApiDocument document, Type controllerType, OpenApiDocumentGenerator swaggerGenerator, OpenApiSchemaResolver schemaResolver)
        {
            var hasIgnoreAttribute = controllerType.GetTypeInfo()
                .GetCustomAttributes()
                .GetAssignableToTypeName("SwaggerIgnoreAttribute", TypeNameStyle.Name)
                .Any();

            if (hasIgnoreAttribute)
            {
                return false;
            }

            var operations = new List<Tuple<OpenApiOperationDescription, MethodInfo>>();

            var currentControllerType = controllerType;
            while (currentControllerType != null)
            {
                foreach (var method in GetActionMethods(currentControllerType))
                {
                    var httpPaths = GetHttpPaths(controllerType, method).ToList();
                    var httpMethods = GetSupportedHttpMethods(method).ToList();

                    foreach (var httpPath in httpPaths)
                    {
                        foreach (var httpMethod in httpMethods)
                        {
                            var isPathAlreadyDefinedInInheritanceHierarchy =
                                operations.Any(o => o.Item1.Path == httpPath &&
                                                    o.Item1.Method == httpMethod &&
                                                    o.Item2.DeclaringType != currentControllerType &&
                                                    o.Item2.DeclaringType.IsAssignableToTypeName(currentControllerType.FullName, TypeNameStyle.FullName));

                            if (isPathAlreadyDefinedInInheritanceHierarchy == false)
                            {
                                var operationDescription = new OpenApiOperationDescription
                                {
                                    Path = httpPath,
                                    Method = httpMethod,
                                    Operation = new OpenApiOperation
                                    {
                                        IsDeprecated = method.GetCustomAttribute<ObsoleteAttribute>() != null,
                                        OperationId = GetOperationId(document, controllerType.Name, method, httpMethod)
                                    }
                                };

                                operations.Add(new Tuple<OpenApiOperationDescription, MethodInfo>(operationDescription, method));
                            }
                        }
                    }
                }

                currentControllerType = currentControllerType.GetTypeInfo().BaseType;
            }

            return AddOperationDescriptionsToDocument(document, controllerType, operations, swaggerGenerator, schemaResolver);
        }

        private bool AddOperationDescriptionsToDocument(OpenApiDocument document, Type controllerType, List<Tuple<OpenApiOperationDescription, MethodInfo>> operations, OpenApiDocumentGenerator swaggerGenerator, OpenApiSchemaResolver schemaResolver)
        {
            var addedOperations = 0;
            var allOperation = operations.Select(t => t.Item1).ToList();
            foreach (var tuple in operations)
            {
                var operation = tuple.Item1;
                var method = tuple.Item2;

                var addOperation = RunOperationProcessors(document, controllerType, method, operation, allOperation, swaggerGenerator, schemaResolver);
                if (addOperation)
                {
                    var path = operation.Path.Replace("//", "/");

                    if (!document.Paths.ContainsKey(path))
                    {
                        document.Paths[path] = new OpenApiPathItem();
                    }

                    if (document.Paths[path].ContainsKey(operation.Method))
                    {
                        throw new InvalidOperationException("The method '" + operation.Method + "' on path '" + path + "' is registered multiple times " +
                            "(check the DefaultUrlTemplate setting [default for Web API: 'api/{controller}/{id}'; for MVC projects: '{controller}/{action}/{id?}']).");
                    }

                    document.Paths[path][operation.Method] = operation.Operation;
                    addedOperations++;
                }
            }

            return addedOperations > 0;
        }

        private bool RunOperationProcessors(OpenApiDocument document, Type controllerType, MethodInfo methodInfo, OpenApiOperationDescription operationDescription, List<OpenApiOperationDescription> allOperations, OpenApiDocumentGenerator swaggerGenerator, OpenApiSchemaResolver schemaResolver)
        {
            var context = new OperationProcessorContext(document, operationDescription, controllerType,
                methodInfo, swaggerGenerator, Settings.SchemaGenerator, schemaResolver, Settings, allOperations);

            // 1. Run from settings
            foreach (var operationProcessor in Settings.OperationProcessors)
            {
                if (operationProcessor.Process(context) == false)
                {
                    return false;
                }
            }

            // 2. Run from class attributes
            var operationProcessorAttribute = methodInfo.DeclaringType.GetTypeInfo()
                .GetCustomAttributes()
            // 3. Run from method attributes
                .Concat(methodInfo.GetCustomAttributes())
                .Where(a => a.GetType().IsAssignableToTypeName("SwaggerOperationProcessorAttribute", TypeNameStyle.Name));

            foreach (dynamic attribute in operationProcessorAttribute)
            {
                var operationProcessor = ObjectExtensions.HasProperty(attribute, "Parameters") ?
                    (IOperationProcessor)Activator.CreateInstance(attribute.Type, attribute.Parameters) :
                    (IOperationProcessor)Activator.CreateInstance(attribute.Type);

                if (operationProcessor.Process(context) == false)
                {
                    return false;
                }
            }

            return true;
        }

        private static IEnumerable<MethodInfo> GetActionMethods(Type controllerType)
        {
            var methods = controllerType.GetRuntimeMethods().Where(m => m.IsPublic);
            return methods.Where(m =>
                m.IsSpecialName == false && // avoid property methods
                m.DeclaringType == controllerType && // no inherited methods (handled in GenerateForControllerAsync)
                m.DeclaringType != typeof(object) &&
                m.IsStatic == false &&
                m.GetCustomAttributes().Select(a => a.GetType()).All(a =>
                    !a.IsAssignableToTypeName("SwaggerIgnoreAttribute", TypeNameStyle.Name) &&
                    !a.IsAssignableToTypeName("NonActionAttribute", TypeNameStyle.Name)) &&
                m.DeclaringType.FullName.StartsWith("Microsoft.AspNet") == false && // .NET Core (Web API & MVC)
                m.DeclaringType.FullName != "System.Web.Http.ApiController" &&
                m.DeclaringType.FullName != "System.Web.Mvc.Controller")
                .Where(m =>
                {
                    return m.GetCustomAttributes()
                        .SingleOrDefault(a => a.GetType().Name == "ApiExplorerSettingsAttribute")?
                        .TryGetPropertyValue("IgnoreApi", false) != true;
                });
        }

        private string GetOperationId(OpenApiDocument document, string controllerName, MethodInfo method, string httpMethod)
        {
            string operationId;

            dynamic swaggerOperationAttribute = method
                .GetCustomAttributes()
                .FirstAssignableToTypeNameOrDefault("SwaggerOperationAttribute", TypeNameStyle.Name);

            dynamic httpAttribute = null;
            if (!string.IsNullOrWhiteSpace(httpMethod))
            {
                var attributeName = Char.ToUpperInvariant(httpMethod[0]) + httpMethod.Substring(1).ToLowerInvariant();
                var typeName = string.Format("Microsoft.AspNetCore.Mvc.Http{0}Attribute", attributeName);
                httpAttribute = method
                    .GetCustomAttributes()
                    .FirstAssignableToTypeNameOrDefault(typeName);
            }

            if (swaggerOperationAttribute != null && !string.IsNullOrEmpty(swaggerOperationAttribute.OperationId))
            {
                operationId = swaggerOperationAttribute.OperationId;
            }
            else if (Settings.UseHttpAttributeNameAsOperationId && httpAttribute != null && !string.IsNullOrWhiteSpace(httpAttribute.Name))
            {
                operationId = httpAttribute.Name;
            }
            else
            {
                if (controllerName.EndsWith("Controller"))
                {
                    controllerName = controllerName.Substring(0, controllerName.Length - 10);
                }

                operationId = controllerName + "_" + GetActionName(method);
            }

            var number = 1;
            while (document.Operations.Any(o => o.Operation.OperationId == operationId + (number > 1 ? "_" + number : string.Empty)))
            {
                number++;
            }

            return operationId + (number > 1 ? number.ToString() : string.Empty);
        }

        private IEnumerable<string> GetHttpPaths(Type controllerType, MethodInfo method)
        {
            var httpPaths = new List<string>();
            var controllerName = controllerType.Name.Replace("Controller", string.Empty);

            var routeAttributes = GetRouteAttributes(method.GetCustomAttributes()).ToList();

            // .NET Core: RouteAttribute on class level
            var routeAttributeOnClass = GetRouteAttribute(controllerType);
            var routePrefixAttribute = GetRoutePrefixAttribute(controllerType);

            if (routeAttributes.Any())
            {
                foreach (var attribute in routeAttributes)
                {
                    if (attribute.Template.StartsWith("~/")) // ignore route prefixes
                    {
                        httpPaths.Add(attribute.Template.Substring(1));
                    }
                    else if (routePrefixAttribute != null)
                    {
                        httpPaths.Add(routePrefixAttribute.Prefix + "/" + attribute.Template);
                    }
                    else if (routeAttributeOnClass != null)
                    {
                        if (attribute.Template.StartsWith("/"))
                        {
                            httpPaths.Add(attribute.Template);
                        }
                        else
                        {
                            httpPaths.Add(routeAttributeOnClass.Template + "/" + attribute.Template);
                        }
                    }
                    else
                    {
                        httpPaths.Add(attribute.Template);
                    }
                }
            }
            else if (routePrefixAttribute != null && routeAttributeOnClass != null)
            {
                httpPaths.Add(routePrefixAttribute.Prefix + "/" + routeAttributeOnClass.Template);
            }
            else if (routePrefixAttribute != null)
            {
                httpPaths.Add(routePrefixAttribute.Prefix);
            }
            else if (routeAttributeOnClass != null)
            {
                httpPaths.Add(routeAttributeOnClass.Template);
            }
            else
            {
                httpPaths.Add(Settings.DefaultUrlTemplate ?? string.Empty);
            }

            var actionName = GetActionName(method);
            return httpPaths
                .SelectMany(p => ExpandOptionalHttpPathParameters(p, method))
                .Select(p =>
                    "/" + p
                        .Replace("[", "{")
                        .Replace("]", "}")
                        .Replace("{controller}", controllerName)
                        .Replace("{action}", actionName)
                        .Replace("{*", "{") // wildcard path parameters are not supported in Swagger
                        .Trim('/'))
                .Distinct()
                .ToList();
        }

        private IEnumerable<string> ExpandOptionalHttpPathParameters(string path, MethodInfo method)
        {
            var segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];
                if (segment.EndsWith("?}"))
                {
                    // Only expand if optional parameter is available in action method
                    if (method.GetParameters().Any(p => segment.StartsWith("{" + p.Name + ":") || segment.StartsWith("{" + p.Name + "?")))
                    {
                        foreach (var p in ExpandOptionalHttpPathParameters(string.Join("/", segments.Take(i).Concat(new[] { segment.Replace("?", "") }).Concat(segments.Skip(i + 1))), method))
                        {
                            yield return p;
                        }
                    }
                    else
                    {
                        foreach (var p in ExpandOptionalHttpPathParameters(string.Join("/", segments.Take(i).Concat(segments.Skip(i + 1))), method))
                        {
                            yield return p;
                        }
                    }

                    yield break;
                }
            }

            yield return path;
        }

        private RouteAttributeFacade GetRouteAttribute(Type type)
        {
            do
            {
                var attributes = type.GetTypeInfo().GetCustomAttributes(false).Cast<Attribute>();

                var attribute = GetRouteAttributes(attributes).SingleOrDefault();
                if (attribute != null)
                {
                    return attribute;
                }

                type = type.GetTypeInfo().BaseType;
            } while (type != null);

            return null;
        }

        private RoutePrefixAttributeFacade GetRoutePrefixAttribute(Type type)
        {
            do
            {
                var attributes = type.GetTypeInfo().GetCustomAttributes(false).Cast<Attribute>();

                var attribute = GetRoutePrefixAttributes(attributes).SingleOrDefault();
                if (attribute != null)
                {
                    return attribute;
                }

                type = type.GetTypeInfo().BaseType;
            } while (type != null);

            return null;
        }

        private IEnumerable<RouteAttributeFacade> GetRouteAttributes(IEnumerable<Attribute> attributes)
        {
            return attributes.Select(RouteAttributeFacade.TryMake).Where(a => a?.Template != null);
        }

        private IEnumerable<RoutePrefixAttributeFacade> GetRoutePrefixAttributes(IEnumerable<Attribute> attributes)
        {
            return attributes.Select(RoutePrefixAttributeFacade.TryMake).Where(a => a != null);
        }

        private string GetActionName(MethodInfo method)
        {
            dynamic actionNameAttribute = method.GetCustomAttributes()
                .SingleOrDefault(a => a.GetType().Name == "ActionNameAttribute");

            if (actionNameAttribute != null)
            {
                return actionNameAttribute.Name;
            }

            var methodName = method.Name;
            if (methodName.EndsWith("Async"))
            {
                methodName = methodName.Substring(0, methodName.Length - 5);
            }

            return methodName;
        }

        private IEnumerable<string> GetSupportedHttpMethods(MethodInfo method)
        {
            // See http://www.asp.net/web-api/overview/web-api-routing-and-actions/routing-in-aspnet-web-api

            var actionName = GetActionName(method);

            var httpMethods = GetSupportedHttpMethodsFromAttributes(method).ToArray();
            foreach (var httpMethod in httpMethods)
            {
                yield return httpMethod;
            }

            if (httpMethods.Length == 0)
            {
                if (actionName.StartsWith("Get", StringComparison.OrdinalIgnoreCase))
                {
                    yield return OpenApiOperationMethod.Get;
                }
                else if (actionName.StartsWith("Post", StringComparison.OrdinalIgnoreCase))
                {
                    yield return OpenApiOperationMethod.Post;
                }
                else if (actionName.StartsWith("Put", StringComparison.OrdinalIgnoreCase))
                {
                    yield return OpenApiOperationMethod.Put;
                }
                else if (actionName.StartsWith("Delete", StringComparison.OrdinalIgnoreCase))
                {
                    yield return OpenApiOperationMethod.Delete;
                }
                else if (actionName.StartsWith("Patch", StringComparison.OrdinalIgnoreCase))
                {
                    yield return OpenApiOperationMethod.Patch;
                }
                else if (actionName.StartsWith("Options", StringComparison.OrdinalIgnoreCase))
                {
                    yield return OpenApiOperationMethod.Options;
                }
                else if (actionName.StartsWith("Head", StringComparison.OrdinalIgnoreCase))
                {
                    yield return OpenApiOperationMethod.Head;
                }
                else
                {
                    yield return OpenApiOperationMethod.Post;
                }
            }
        }

        private IEnumerable<string> GetSupportedHttpMethodsFromAttributes(MethodInfo method)
        {
            if (method.GetCustomAttributes().Any(a => a.GetType().Name == "HttpGetAttribute"))
            {
                yield return OpenApiOperationMethod.Get;
            }

            if (method.GetCustomAttributes().Any(a => a.GetType().Name == "HttpPostAttribute"))
            {
                yield return OpenApiOperationMethod.Post;
            }

            if (method.GetCustomAttributes().Any(a => a.GetType().Name == "HttpPutAttribute"))
            {
                yield return OpenApiOperationMethod.Put;
            }

            if (method.GetCustomAttributes().Any(a => a.GetType().Name == "HttpDeleteAttribute"))
            {
                yield return OpenApiOperationMethod.Delete;
            }

            if (method.GetCustomAttributes().Any(a => a.GetType().Name == "HttpOptionsAttribute"))
            {
                yield return OpenApiOperationMethod.Options;
            }

            if (method.GetCustomAttributes().Any(a => a.GetType().Name == "HttpPatchAttribute"))
            {
                yield return OpenApiOperationMethod.Patch;
            }

            if (method.GetCustomAttributes().Any(a => a.GetType().Name == "HttpHeadAttribute"))
            {
                yield return OpenApiOperationMethod.Head;
            }

            dynamic acceptVerbsAttribute = method.GetCustomAttributes()
                .SingleOrDefault(a => a.GetType().Name == "AcceptVerbsAttribute");

            if (acceptVerbsAttribute != null)
            {
                var httpMethods = acceptVerbsAttribute.HttpMethods is ICollection
                    ? ((ICollection)acceptVerbsAttribute.HttpMethods).OfType<object>().Select(v => v.ToString().ToLowerInvariant())
                    : ((IEnumerable<string>)acceptVerbsAttribute.HttpMethods).Select(v => v.ToLowerInvariant());

                foreach (var verb in httpMethods)
                {
                    if (verb == "get")
                    {
                        yield return OpenApiOperationMethod.Get;
                    }
                    else if (verb == "post")
                    {
                        yield return OpenApiOperationMethod.Post;
                    }
                    else if (verb == "put")
                    {
                        yield return OpenApiOperationMethod.Put;
                    }
                    else if (verb == "delete")
                    {
                        yield return OpenApiOperationMethod.Delete;
                    }
                    else if (verb == "options")
                    {
                        yield return OpenApiOperationMethod.Options;
                    }
                    else if (verb == "head")
                    {
                        yield return OpenApiOperationMethod.Head;
                    }
                    else if (verb == "patch")
                    {
                        yield return OpenApiOperationMethod.Patch;
                    }
                }
            }
        }
    }
}

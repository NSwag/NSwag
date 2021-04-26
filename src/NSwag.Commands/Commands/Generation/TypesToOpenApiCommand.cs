//-----------------------------------------------------------------------
// <copyright file="TypesToSwaggerCommand.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/RicoSuter/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NConsole;
using Newtonsoft.Json;
using NJsonSchema;
using NJsonSchema.Generation;
using NJsonSchema.Infrastructure;
using NSwag.AssemblyLoader.Utilities;

namespace NSwag.Commands.Generation
{
    /// <summary></summary>
    [Command(Name = "types2openapi")]
    public class TypesToOpenApiCommand : TypesToSwaggerCommand
    {
    }

    /// <summary></summary>
    [Command(Name = "types2swagger")]
    public class TypesToSwaggerCommand : IsolatedSwaggerOutputCommandBase<JsonSchemaGeneratorSettings>
    {
        /// <summary>Initializes a new instance of the <see cref="TypesToSwaggerCommand"/> class.</summary>
        public TypesToSwaggerCommand()
        {
            Settings = new JsonSchemaGeneratorSettings { SchemaType = SchemaType.Swagger2 };
            ClassNames = new string[] { };
        }

        [JsonIgnore]
        protected override JsonSchemaGeneratorSettings Settings { get; }

        [Argument(Name = "ClassNames", Description = "The class names.")]
        public string[] ClassNames { get; set; }

        [Argument(Name = "DefaultPropertyNameHandling", IsRequired = false, Description = "The default property name handling ('Default' or 'CamelCase').")]
        public PropertyNameHandling DefaultPropertyNameHandling
        {
            get { return Settings.DefaultPropertyNameHandling; }
            set { Settings.DefaultPropertyNameHandling = value; }
        }

        [Argument(Name = nameof(DefaultReferenceTypeNullHandling), IsRequired = false, Description = "The default reference type null handling (Null (default) or NotNull).")]
        public ReferenceTypeNullHandling DefaultReferenceTypeNullHandling
        {
            get => Settings.DefaultReferenceTypeNullHandling;
            set => Settings.DefaultReferenceTypeNullHandling = value;
        }

        [Argument(Name = nameof(DefaultDictionaryValueReferenceTypeNullHandling), IsRequired = false, Description = "The default reference type null handling of dictionary value types (NotNull (default) or Null).")]
        public ReferenceTypeNullHandling DefaultDictionaryValueReferenceTypeNullHandling
        {
            get => Settings.DefaultDictionaryValueReferenceTypeNullHandling;
            set => Settings.DefaultDictionaryValueReferenceTypeNullHandling = value;
        }

        [Argument(Name = "DefaultEnumHandling", IsRequired = false, Description = "The default enum handling ('String' or 'Integer'), default: Integer.")]
        public EnumHandling DefaultEnumHandling
        {
            get { return Settings.DefaultEnumHandling; }
            set { Settings.DefaultEnumHandling = value; }
        }

        [Argument(Name = "FlattenInheritanceHierarchy", IsRequired = false, Description = "Flatten the inheritance hierarchy instead of using allOf to describe inheritance (default: false).")]
        public bool FlattenInheritanceHierarchy
        {
            get { return Settings.FlattenInheritanceHierarchy; }
            set { Settings.FlattenInheritanceHierarchy = value; }
        }

        [Argument(Name = "IgnoreObsoleteProperties", IsRequired = false, Description = "Ignore properties with the ObsoleteAttribute (default: false).")]
        public bool IgnoreObsoleteProperties
        {
            get { return Settings.IgnoreObsoleteProperties; }
            set { Settings.IgnoreObsoleteProperties = value; }
        }

        [Argument(Name = "AllowReferencesWithProperties", IsRequired = false, Description = "Use $ref references even if additional properties are defined on " +
            "the object (otherwise allOf/oneOf with $ref is used, default: false).")]
        public bool AllowReferencesWithProperties
        {
            get { return Settings.AllowReferencesWithProperties; }
            set { Settings.AllowReferencesWithProperties = value; }
        }

        [Argument(Name = "GenerateKnownTypes", IsRequired = false, Description = "Generate schemas for types in KnownTypeAttribute attributes (default: true).")]
        public bool GenerateKnownTypes
        {
            get { return Settings.GenerateKnownTypes; }
            set { Settings.GenerateKnownTypes = value; }
        }

        [Argument(Name = "GenerateXmlObjects", IsRequired = false, Description = "Generate xmlObject representation for definitions (default: false).")]
        public bool GenerateXmlObjects
        {
            get { return Settings.GenerateXmlObjects; }
            set { Settings.GenerateXmlObjects = value; }
        }

        protected override Task<string> RunIsolatedAsync(AssemblyLoader.AssemblyLoader assemblyLoader)
        {
            var document = new OpenApiDocument();
            var generator = new JsonSchemaGenerator(Settings);
            var schemaResolver = new OpenApiSchemaResolver(document, Settings);

#if FullNet
            var assemblies = PathUtilities.ExpandFileWildcards(AssemblyPaths)
                .Select(path => Assembly.LoadFrom(path)).ToArray();
#else
            var currentDirectory = DynamicApis.DirectoryGetCurrentDirectory();
            var assemblies = PathUtilities.ExpandFileWildcards(AssemblyPaths)
                .Select(path => assemblyLoader.Context.LoadFromAssemblyPath(PathUtilities.MakeAbsolutePath(path, currentDirectory))).ToArray();
#endif

            var allExportedClassNames = assemblies.SelectMany(a => a.ExportedTypes).Select(t => t.FullName).ToList();
            var matchedClassNames = ClassNames
                .SelectMany(n => PathUtilities.FindWildcardMatches(n, allExportedClassNames, '.'))
                .Distinct();

            foreach (var className in matchedClassNames)
            {
                var type = assemblies.Select(a => a.GetType(className)).FirstOrDefault(t => t != null);
                generator.Generate(type, schemaResolver);
            }

            return Task.FromResult(document.ToJson(OutputType));
        }
    }
}
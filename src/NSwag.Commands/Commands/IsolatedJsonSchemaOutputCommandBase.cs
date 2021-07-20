//-----------------------------------------------------------------------
// <copyright file="AssemblyOutputCommandBase.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/NSwag/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NConsole;
using Newtonsoft.Json;
using NJsonSchema;
using NJsonSchema.Generation;
using NJsonSchema.Infrastructure;
using NJsonSchema.Yaml;
using NSwag.AssemblyLoader.Utilities;

namespace NSwag.Commands
{
    /// <summary>A command which is run in isolation.</summary>
    public abstract class IsolatedJsonSchemaOutputCommandBase<T> : IsolatedCommandBase<string>, IOutputCommand
        where T : JsonSchemaGeneratorSettings
    {
        [JsonIgnore]
        protected abstract T Settings { get; }

        [Argument(Name = "Output", IsRequired = false, Description = "The output file path (optional).")]
        [JsonProperty("output", NullValueHandling = NullValueHandling.Include)]
        public string OutputFilePath { get; set; }

        public override async Task<object> RunAsync(CommandLineProcessor processor, IConsoleHost host)
        {
            JsonReferenceResolver ReferenceResolverFactory(JsonSchema d) =>
                new JsonAndYamlReferenceResolver(new JsonSchemaResolver(d, Settings));

            var documentJson = await RunIsolatedAsync((string)null);
            var document = await JsonSchema.FromJsonAsync(documentJson, null, ReferenceResolverFactory).ConfigureAwait(false);
            await OutputCommandExtensions.TryWriteFileOutputAsync(this, this.OutputFilePath, host, () => document.ToJson()).ConfigureAwait(false);
            return document;
        }

        protected async Task<Assembly[]> LoadAssembliesAsync(IEnumerable<string> assemblyPaths, AssemblyLoader.AssemblyLoader assemblyLoader)
        {
#if FullNet
            var assemblies = PathUtilities.ExpandFileWildcards(assemblyPaths)
                .Select(path => Assembly.LoadFrom(path)).ToArray();
#else
            var currentDirectory = DynamicApis.DirectoryGetCurrentDirectory();
            var assemblies = PathUtilities.ExpandFileWildcards(assemblyPaths)
                .Select(path => assemblyLoader.Context.LoadFromAssemblyPath(PathUtilities.MakeAbsolutePath(path, currentDirectory)))
                .ToArray();
#endif
            return assemblies;
        }
    }
}
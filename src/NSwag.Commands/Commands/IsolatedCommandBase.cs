//-----------------------------------------------------------------------
// <copyright file="IsolatedCommandBase.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/RicoSuter/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Namotion.Reflection;
using NConsole;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NSwag.Annotations;
using NSwag.AssemblyLoader;
using NSwag.AssemblyLoader.Utilities;
using NSwag.Generation;
using NSwag.Generation.AspNetCore;
using NSwag.Generation.WebApi;

namespace NSwag.Commands
{
    /// <summary>A command which is run in isolation.</summary>
    public abstract class IsolatedCommandBase<TResult> : IConsoleCommand
    {
        [Argument(Name = "Assembly", IsRequired = false, Description = "The path or paths to the .NET assemblies (comma separated).")]
        public string[] AssemblyPaths { get; set; } = new string[0];

        [Argument(Name = "AssemblyConfig", IsRequired = false, Description = "The path to the assembly App.config or Web.config (optional).")]
        public string AssemblyConfig { get; set; }

        [Argument(Name = "ReferencePaths", IsRequired = false, Description = "The paths to search for referenced assembly files (comma separated).")]
        public string[] ReferencePaths { get; set; } = new string[0];

        [Argument(Name = "UseNuGetCache", IsRequired = false, Description = "Determines if local Nuget's cache folder should be put in the ReferencePaths by default")]
        public bool UseNuGetCache { get; set; } = false;

        public abstract Task<object> RunAsync(CommandLineProcessor processor, IConsoleHost host);

        protected async Task<TResult> RunIsolatedAsync(string configurationFile)
        {
            var assemblyDirectory = AssemblyPaths.Any() ? Path.GetDirectoryName(Path.GetFullPath(PathUtilities.ExpandFileWildcards(AssemblyPaths).First())) : configurationFile;
            var bindingRedirects = GetBindingRedirects();
            var assemblies = GetAssemblies(assemblyDirectory);

            if (UseNuGetCache)
            {
                var defaultNugetPackages = LoadDefaultNugetCache();
                ReferencePaths = ReferencePaths.Concat(defaultNugetPackages).ToArray();
            }

            using (var isolated = new AppDomainIsolation<IsolatedCommandAssemblyLoader>(assemblyDirectory, AssemblyConfig, bindingRedirects, assemblies))
            {
                return await Task.Run(() => isolated.Object.Run(GetType().FullName, JsonConvert.SerializeObject(this), AssemblyPaths, ReferencePaths)).ConfigureAwait(false);
            }
        }

        protected abstract Task<TResult> RunIsolatedAsync(AssemblyLoader.AssemblyLoader assemblyLoader);

        private class IsolatedCommandAssemblyLoader : AssemblyLoader.AssemblyLoader
        {
            internal TResult Run(string commandType, string commandData, string[] assemblyPaths, string[] referencePaths)
            {
                RegisterReferencePaths(GetAllReferencePaths(assemblyPaths, referencePaths));

                var type = Type.GetType(commandType);
                var command = (IsolatedCommandBase<TResult>)JsonConvert.DeserializeObject(commandData, type);

                return command.RunIsolatedAsync(this).GetAwaiter().GetResult();
            }
        }

        private static string[] GetAllReferencePaths(string[] assemblyPaths, string[] referencePaths)
        {
            return assemblyPaths.Select(p => Path.GetDirectoryName(PathUtilities.MakeAbsolutePath(p, Directory.GetCurrentDirectory())))
                .Concat(referencePaths)
                .Distinct()
                .ToArray();
        }

        private static string[] LoadDefaultNugetCache()
        {
            var path = Environment.GetEnvironmentVariable("NUGET_PACKAGES");

            if (String.IsNullOrEmpty(path))
            {
                var envHome = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "USERPROFILE" : "HOME";
                var home = Environment.GetEnvironmentVariable(envHome);
                path = Path.Combine(home, ".nuget", "packages");
            }

            return new[] { Path.GetFullPath(path) };
        }

#if NET461
        public IEnumerable<string> GetAssemblies(string assemblyDirectory)
        {
            var codeBaseDirectory = Path.GetDirectoryName(new Uri(typeof(IsolatedCommandBase<>).GetTypeInfo().Assembly.CodeBase).LocalPath);

            yield return codeBaseDirectory + Path.DirectorySeparatorChar + "Newtonsoft.Json.dll";
            yield return codeBaseDirectory + Path.DirectorySeparatorChar + "NJsonSchema.dll";
            yield return codeBaseDirectory + Path.DirectorySeparatorChar + "NSwag.Core.dll";
            yield return codeBaseDirectory + Path.DirectorySeparatorChar + "NSwag.Commands.dll";
            yield return codeBaseDirectory + Path.DirectorySeparatorChar + "NSwag.Generation.dll";
            yield return codeBaseDirectory + Path.DirectorySeparatorChar + "NSwag.Generation.WebApi.dll";
            yield return codeBaseDirectory + Path.DirectorySeparatorChar + "NSwag.Generation.AspNetCore.dll";
        }
#else
        public IEnumerable<Assembly> GetAssemblies(string assemblyDirectory)
        {
            yield return typeof(JToken).GetTypeInfo().Assembly;
            yield return typeof(ContextualType).GetTypeInfo().Assembly;
            yield return typeof(JsonSchema).GetTypeInfo().Assembly;
            yield return typeof(OpenApiDocument).GetTypeInfo().Assembly;
            yield return typeof(InputOutputCommandBase).GetTypeInfo().Assembly;
            yield return typeof(OpenApiSchemaGenerator).GetTypeInfo().Assembly;
            yield return typeof(WebApiOpenApiDocumentGenerator).GetTypeInfo().Assembly;
            yield return typeof(AspNetCoreOpenApiDocumentGeneratorSettings).GetTypeInfo().Assembly;
            yield return typeof(OpenApiYamlDocument).GetTypeInfo().Assembly;
        }
#endif

        public IEnumerable<BindingRedirect> GetBindingRedirects()
        {
#if NET461
            yield return new BindingRedirect("Newtonsoft.Json", typeof(JToken), "30ad4fe6b2a6aeed");
            yield return new BindingRedirect("Namotion.Reflection", typeof(ContextualType), "c2f9c3bdfae56102");
            yield return new BindingRedirect("NJsonSchema", typeof(JsonSchema), "c2f9c3bdfae56102");
            yield return new BindingRedirect("NSwag.Core", typeof(OpenApiDocument), "c2d88086e098d109");
            yield return new BindingRedirect("NSwag.Generation", typeof(OpenApiSchemaGenerator), "c2d88086e098d109");
            yield return new BindingRedirect("NSwag.Generation.WebApi", typeof(WebApiOpenApiDocumentGenerator), "c2d88086e098d109");
            yield return new BindingRedirect("NSwag.Generation.AspNetCore", typeof(AspNetCoreOpenApiDocumentGenerator), "c2d88086e098d109");
            yield return new BindingRedirect("NSwag.Annotations", typeof(SwaggerTagsAttribute), "c2d88086e098d109");
            yield return new BindingRedirect("NSwag.Core.Yaml", typeof(OpenApiYamlDocument), "c2d88086e098d109");
            yield return new BindingRedirect("System.Runtime", "4.0.0.0", "b03f5f7f11d50a3a");
#else
            return new BindingRedirect[0];
#endif
        }
    }
}

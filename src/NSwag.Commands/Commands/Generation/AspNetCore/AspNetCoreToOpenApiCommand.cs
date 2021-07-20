//-----------------------------------------------------------------------
// <copyright file="NSwagSettings.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/RicoSuter/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using NConsole;
using Newtonsoft.Json;
using NSwag.Generation.AspNetCore;
using NJsonSchema.Yaml;
using NJsonSchema;
using Microsoft.AspNetCore.Hosting;
using NSwag.Generation;
using NJsonSchema.Generation;
using Namotion.Reflection;

#if NET5_0 || NETCOREAPP || NETSTANDARD
using System.Runtime.Loader;
#endif

namespace NSwag.Commands.Generation.AspNetCore
{
    /// <summary>The generator.</summary>
    [Command(Name = "aspnetcore2openapi", Description = "Generates a Swagger specification ASP.NET Core Mvc application using ApiExplorer.")]
    public class AspNetCoreToOpenApiCommand : AspNetCoreToSwaggerCommand
    {
    }

    /// <summary>The generator.</summary>
    [Command(Name = "aspnetcore2swagger", Description = "Generates a Swagger specification ASP.NET Core Mvc application using ApiExplorer (obsolete: use aspnetcore2openapi instead).")]
    public class AspNetCoreToSwaggerCommand : OpenApiGeneratorCommandBase<AspNetCoreOpenApiDocumentGeneratorSettings>
    {
        private const string LauncherBinaryName = "NSwag.AspNetCore.Launcher";

        [Argument(Name = nameof(Project), IsRequired = false, Description = "The project to use.")]
        public string Project { get; set; }

        [Argument(Name = nameof(MSBuildProjectExtensionsPath), IsRequired = false, Description = "The MSBuild project extensions path. Defaults to \"obj\".")]
        public string MSBuildProjectExtensionsPath { get; set; }

        [Argument(Name = nameof(Configuration), IsRequired = false, Description = "The configuration to use.")]
        public string Configuration { get; set; }

        [Argument(Name = nameof(Runtime), IsRequired = false, Description = "The runtime to use.")]
        public string Runtime { get; set; }

        [Argument(Name = nameof(TargetFramework), IsRequired = false, Description = "The target framework to use.")]
        public string TargetFramework { get; set; }

        [Argument(Name = nameof(NoBuild), IsRequired = false, Description = "Don't build the project. Only use this when the build is up-to-date.")]
        public bool NoBuild { get; set; }

        [Argument(Name = nameof(MSBuildOutputPath), IsRequired = false, Description = "The MSBuild output path")]
        public string MSBuildOutputPath { get; set; }

        [Argument(Name = nameof(Verbose), IsRequired = false, Description = "Print verbose output.")]
        public bool Verbose { get; set; } = true;

        [Argument(Name = nameof(WorkingDirectory), IsRequired = false, Description = "The working directory to use.")]
        public string WorkingDirectory { get; set; }

        [Argument(Name = "RequireParametersWithoutDefault", IsRequired = false, Description = "Parameters without default value are always required" +
                                                                                              "(i.e. api explorer info and only optional when default is set, legacy, default: false).")]
        public bool RequireParametersWithoutDefault
        {
            get => Settings.RequireParametersWithoutDefault;
            set => Settings.RequireParametersWithoutDefault = value;
        }

        [Argument(Name = "ApiGroupNames", IsRequired = false, Description = "The ASP.NET Core API Explorer group names to include (comma separated, default: empty = all).")]
        public string[] ApiGroupNames
        {
            get => Settings.ApiGroupNames;
            set => Settings.ApiGroupNames = value;
        }

        public override async Task<object> RunAsync(CommandLineProcessor processor, IConsoleHost host)
        {
            if (!string.IsNullOrEmpty(Project) && AssemblyPaths.Any())
            {
                throw new InvalidOperationException("Either provide a Project or an assembly but not both.");
            }

            if (string.IsNullOrEmpty(Project))
            {
                // Run with assembly
                return await base.RunAsync(processor, host);
            }
            else
            {
                // Run with .csproj
                var verboseHost = Verbose ? host : null;

                var projectFile = ProjectMetadata.FindProject(Project);
                var projectMetadata = await ProjectMetadata.GetProjectMetadata(
                    projectFile,
                    MSBuildProjectExtensionsPath,
                    TargetFramework,
                    Configuration,
                    Runtime,
                    NoBuild,
                    MSBuildOutputPath,
                    verboseHost).ConfigureAwait(false);

                if (!File.Exists(Path.Combine(projectMetadata.OutputPath, projectMetadata.TargetFileName)))
                {
                    throw new InvalidOperationException($"Project outputs could not be located in " +
                                                        $"'{projectMetadata.OutputPath}'. Ensure that the project has been built.");
                }

                var cleanupFiles = new List<string>();

                var args = new List<string>();
                string executable;

#if NET461
                var toolDirectory = AppDomain.CurrentDomain.BaseDirectory;
                if (!Directory.Exists(toolDirectory))
                {
                    toolDirectory = Path.GetDirectoryName(typeof(AspNetCoreToSwaggerCommand).GetTypeInfo().Assembly.Location);
                }

                if (projectMetadata.TargetFrameworkIdentifier == ".NETFramework")
                {
                    string binaryName;
                    var is32BitProject = string.Equals(projectMetadata.PlatformTarget, "x86", StringComparison.OrdinalIgnoreCase);
                    if (is32BitProject)
                    {
                        if (Environment.Is64BitProcess)
                        {
                            throw new InvalidOperationException($"The ouput of {projectFile} is a 32-bit application and requires NSwag.Console.x86 to be processed.");
                        }

                        binaryName = LauncherBinaryName + ".x86.exe";
                    }
                    else
                    {
                        if (!Environment.Is64BitProcess)
                        {
                            throw new InvalidOperationException($"The ouput of {projectFile} is a 64-bit application and requires NSwag.Console to be processed.");
                        }

                        binaryName = LauncherBinaryName + ".exe";
                    }

                    var executableSource = Path.Combine(toolDirectory, binaryName);
                    if (!File.Exists(executableSource))
                    {
                        throw new InvalidOperationException($"Unable to locate {binaryName} in {toolDirectory}.");
                    }

                    executable = Path.Combine(projectMetadata.OutputPath, binaryName);
                    File.Copy(executableSource, executable, overwrite: true);
                    cleanupFiles.Add(executable);

                    var appConfig = Path.Combine(projectMetadata.OutputPath, projectMetadata.TargetFileName + ".config");
                    if (File.Exists(appConfig))
                    {
                        var copiedAppConfig = Path.ChangeExtension(executable, ".exe.config");
                        File.Copy(appConfig, copiedAppConfig, overwrite: true);
                        cleanupFiles.Add(copiedAppConfig);
                    }
                }
#elif NET5_0 || NETCOREAPP || NETSTANDARD
                var toolDirectory = AppContext.BaseDirectory;
                if (!Directory.Exists(toolDirectory))
                {
                    toolDirectory = Path.GetDirectoryName(typeof(AspNetCoreToSwaggerCommand).GetTypeInfo().Assembly.Location);
                }

                if (projectMetadata.TargetFrameworkIdentifier == ".NETCoreApp" ||
                    projectMetadata.TargetFrameworkIdentifier == "net5.0")
                {
                    executable = "dotnet";
                    args.Add("exec");
                    args.Add("--depsfile");
                    args.Add(projectMetadata.ProjectDepsFilePath);

                    args.Add("--runtimeconfig");
                    args.Add(projectMetadata.ProjectRuntimeConfigFilePath);

                    var binaryName = LauncherBinaryName + ".dll";
                    var executorBinary = Path.Combine(toolDirectory, binaryName);
                   
                    if (!File.Exists(executorBinary))
                    {
                        binaryName = LauncherBinaryName + ".exe";
                        executorBinary = Path.Combine(toolDirectory, binaryName);
                    }

                    if (!File.Exists(executorBinary))
                    {
                        throw new InvalidOperationException($"Unable to locate {binaryName} in {toolDirectory}.");
                    }

                    args.Add(executorBinary);
                }
#endif
                else
                {
                    throw new InvalidOperationException($"Unsupported target framework '{projectMetadata.TargetFrameworkIdentifier}'.");
                }

                var commandFile = Path.GetTempFileName();
                var outputFile = Path.GetTempFileName();
                File.WriteAllText(commandFile, JsonConvert.SerializeObject(this));
                cleanupFiles.Add(commandFile);
                cleanupFiles.Add(outputFile);

                args.Add(commandFile);
                args.Add(outputFile);
                args.Add(projectMetadata.AssemblyName);
                args.Add(toolDirectory);

                try
                {
                    var exitCode = await Exe.RunAsync(executable, args, verboseHost).ConfigureAwait(false);
                    if (exitCode != 0)
                    {
                        throw new InvalidOperationException($"Swagger generation failed with non-zero exit code '{exitCode}'.");
                    }

                    host?.WriteMessage($"Output written to {outputFile}.{Environment.NewLine}");

                    JsonReferenceResolver ReferenceResolverFactory(OpenApiDocument d) => new JsonAndYamlReferenceResolver(new JsonSchemaResolver(d, Settings));

                    var documentJson = File.ReadAllText(outputFile);
                    var document = await OpenApiDocument.FromJsonAsync(documentJson, null, OutputType, ReferenceResolverFactory).ConfigureAwait(false);
                    await this.TryWriteDocumentOutputAsync(host, NewLineBehavior, () => document).ConfigureAwait(false);
                    return document;
                }
                finally
                {
                    TryDeleteFiles(cleanupFiles);
                }
            }
        }

        internal string ChangeWorkingDirectoryAndSetAspNetCoreEnvironment()
        {
            if (!string.IsNullOrEmpty(AspNetCoreEnvironment))
            {
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", AspNetCoreEnvironment);
            }

            var currentWorkingDirectory = Directory.GetCurrentDirectory();

            if (!string.IsNullOrEmpty(WorkingDirectory))
            {
                Directory.SetCurrentDirectory(WorkingDirectory);
            }
            else if (!string.IsNullOrEmpty(Project))
            {
                var workingDirectory = Path.GetDirectoryName(Project);
                if (Directory.Exists(workingDirectory))
                {
                    Directory.SetCurrentDirectory(workingDirectory);
                }
            }
            else if (AssemblyPaths.Any())
            {
                var workingDirectory = Path.GetDirectoryName(AssemblyPaths.First());
                if (Directory.Exists(workingDirectory))
                {
                    Directory.SetCurrentDirectory(workingDirectory);
                }
            }

            return currentWorkingDirectory;
        }

        public async Task<OpenApiDocument> GenerateDocumentAsync(AssemblyLoader.AssemblyLoader assemblyLoader, IServiceProvider serviceProvider, string currentWorkingDirectory)
        {
            Directory.SetCurrentDirectory(currentWorkingDirectory);

            if (UseDocumentProvider)
            {
                return await GenerateDocumentWithDocumentProviderAsync(serviceProvider);
            }
            else
            {
                return await GenerateDocumentWithApiDescriptionAsync(assemblyLoader, serviceProvider, currentWorkingDirectory);
            }
        }

        private async Task<OpenApiDocument> GenerateDocumentWithDocumentProviderAsync(IServiceProvider serviceProvider)
        {
            var documentGenerator = serviceProvider.GetRequiredService<IOpenApiDocumentGenerator>();
            var document = await documentGenerator.GenerateAsync(DocumentName);
            return document;
        }

        private async Task<OpenApiDocument> GenerateDocumentWithApiDescriptionAsync(AssemblyLoader.AssemblyLoader assemblyLoader, IServiceProvider serviceProvider, string currentWorkingDirectory)
        {
            InitializeCustomTypes(assemblyLoader);

            // In the case of KeyNotFoundException, see https://github.com/aspnet/Mvc/issues/5690
            var apiDescriptionProvider = serviceProvider.GetRequiredService<IApiDescriptionGroupCollectionProvider>();

            var settings = await CreateSettingsAsync(assemblyLoader, serviceProvider, currentWorkingDirectory);
            var generator = new AspNetCoreOpenApiDocumentGenerator(settings);
            var document = await generator.GenerateAsync(apiDescriptionProvider.ApiDescriptionGroups).ConfigureAwait(false);

            PostprocessDocument(document);

            return document;
        }

        protected override async Task<string> RunIsolatedAsync(AssemblyLoader.AssemblyLoader assemblyLoader)
        {
            using (var webHost = await CreateWebHostAsync(assemblyLoader))
            {
                var document = await GenerateDocumentAsync(assemblyLoader, webHost.TryGetPropertyValue<IServiceProvider>("Services"), Directory.GetCurrentDirectory());
                return UseDocumentProvider ? document.ToJson() : document.ToJson(OutputType);
            }
        }

        private static void TryDeleteFiles(List<string> files)
        {
            foreach (var file in files)
            {
                try
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
                catch
                {
                    // Don't throw any if any clean up operation fails.
                }
            }
        }
    }
}

//-----------------------------------------------------------------------
// <copyright file="ListWebApiControllersCommand.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/RicoSuter/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NConsole;
using NJsonSchema.Infrastructure;
using NSwag.AssemblyLoader.Utilities;
using NSwag.Generation.WebApi;
using System.IO;
using NSwag.Commands.Generation.WebApi;

namespace NSwag.Commands.Generation
{
    [Command(Name = "list-controllers", Description = "List all controllers classes for the given assembly and settings.")]
    public class ListWebApiControllersCommand : IsolatedCommandBase<string[]>
    {
        [Argument(Name = nameof(File), IsRequired = false, Description = "The nswag.json configuration file path.")]
        public string File { get; set; }

        [Argument(Name = nameof(Variables), IsRequired = false)]
        public string Variables { get; set; }

        public override async Task<object> RunAsync(CommandLineProcessor processor, IConsoleHost host)
        {
            if (!string.IsNullOrEmpty(File))
            {
                var document = await NSwagDocument.LoadWithTransformationsAsync(File, Variables);
                var command = (WebApiToSwaggerCommand)document.SelectedSwaggerGenerator;

                AssemblyPaths = command.AssemblyPaths;
                AssemblyConfig = command.AssemblyConfig;
                ReferencePaths = command.ReferencePaths;
            }

            var classNames = await RunIsolatedAsync(!string.IsNullOrEmpty(File) ? Path.GetDirectoryName(File) : null);

            host.WriteMessage("\r\n");
            foreach (var className in classNames)
            {
                host.WriteMessage(className + "\r\n");
            }

            host.WriteMessage("\r\n");

            return classNames;
        }

        protected override Task<string[]> RunIsolatedAsync(AssemblyLoader.AssemblyLoader assemblyLoader)
        {
#if FullNet
            var result = PathUtilities.ExpandFileWildcards(AssemblyPaths)
                .Select(Assembly.LoadFrom)
#else
            var currentDirectory = DynamicApis.DirectoryGetCurrentDirectory();
            var result = PathUtilities.ExpandFileWildcards(AssemblyPaths)
                .Select(p => assemblyLoader.Context.LoadFromAssemblyPath(PathUtilities.MakeAbsolutePath(p, currentDirectory)))
#endif
                .SelectMany(WebApiOpenApiDocumentGenerator.GetControllerClasses)
                .Select(t => t.FullName)
                .OrderBy(c => c)
                .ToArray();

            return Task.FromResult(result);
        }
    }
}
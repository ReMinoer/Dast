using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Reflection;
using System.Runtime.Loader;
using Dast.Catalogs;
using Dast.Extensibility;
using Dast.Inputs;
using Dast.Outputs;

namespace Dast.Console
{
    static internal class Program
    {

#if !DEBUG
        const string PluginsDirectory = "plugins";
#else
        static private IEnumerable<string> DebugPlugins
        {
            get
            {
                yield return "../../../../../Outputs";
                yield return "../../../../../Inputs";
                yield return "../../../../../Media/Html";
                yield return "../../../../../Media/Markdown";
            }
        }
#endif

        static private void Main(string[] args)
        {
#if DEBUG
            if (args.Length == 0)
            {
                Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
                args = new[] { "../../../../../External/DashCSharp/External/Dash/doc.dh", "md", "html" };
            }
#endif

            if (args.Length <= 1)
            {
                System.Console.WriteLine("Error: Not enough arguments !");
                System.Console.WriteLine("Usage: Dast.Console.exe <filePath> <outputExtension>+");
                System.Console.WriteLine("Press any key to quit...");
                System.Console.ReadKey();
                return;
            }

            string filePath = args[0];
            IEnumerable<string> outputExtensions = args.Skip(1);

            if (Process(filePath, outputExtensions))
                return;

            System.Console.WriteLine("Press any key to quit...");
            System.Console.ReadKey();
        }

        static private bool Process(string filePath, IEnumerable<string> outputExtensions)
        {
            string executableDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string workingDirectory = Directory.GetCurrentDirectory();

            string rootedFilePath = Path.IsPathRooted(filePath) ? filePath : Path.Combine(workingDirectory, filePath);
            var file = new FileInfo(rootedFilePath);
            if (!file.Exists)
            {
                System.Console.WriteLine($"Error: {file.FullName} not found !");
                return false;
            }

#if !DEBUG
            string pluginsPath = Path.Combine(executableDirectory, PluginsDirectory);
            Directory.CreateDirectory(pluginsPath);

            Assembly[] assemblies = Directory.GetFiles(pluginsPath, "*.dll", SearchOption.AllDirectories).Select(AssemblyLoadContext.Default.LoadFromAssemblyPath).ToArray();
#else
            var assemblies = new List<Assembly>();
            foreach (string debugPlugin in DebugPlugins)
            {
                string pluginsPath = Path.Combine(executableDirectory, debugPlugin);
                foreach (DirectoryInfo projectDirectory in new DirectoryInfo(pluginsPath).EnumerateDirectories())
                {
                    string dllDirectoryPath = new DirectoryInfo(Path.Combine(projectDirectory.FullName, "bin", "Debug")).EnumerateDirectories().First().FullName;

                    foreach (string dllPath in Directory.GetFiles(dllDirectoryPath, "*.dll", SearchOption.AllDirectories))
                    {
                        try
                        {
                            Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dllPath);
                            assemblies.Add(assembly);
                        }
                        catch (FileLoadException)
                        {
                        }
                    }
                }
            }
#endif

            var documentConventionBuilder = new ConventionBuilder();
            documentConventionBuilder.ForTypesDerivedFrom<IDocumentInput<string>>().Export<IDocumentInput<string>>();
            documentConventionBuilder.ForTypesDerivedFrom<IDocumentOutput<string>>().Export<IDocumentOutput<string>>();

            ContainerConfiguration documentContainerConfiguration = new ContainerConfiguration().WithAssemblies(assemblies, documentConventionBuilder);

            var inputCatalog = new FormatCatalog<IDocumentInput<string>>();
            var outputCatalog = new FormatCatalog<IDocumentOutput<string>>();

            using (CompositionHost documentContainer = documentContainerConfiguration.CreateContainer())
            {
                IDocumentInput<string>[] documentInputs = documentContainer.GetExports<IDocumentInput<string>>().ToArray();
                if (documentInputs.Length == 0)
                {
                    System.Console.WriteLine("Error: No input format available !");
                    return false;
                }

                IDocumentOutput<string>[] documentOutputs = documentContainer.GetExports<IDocumentOutput<string>>().ToArray();
                if (documentOutputs.Length == 0)
                {
                    System.Console.WriteLine("Error: No output format available !");
                    return false;
                }

                inputCatalog.AddRange(documentInputs);
                outputCatalog.AddRange(documentOutputs);

                foreach (IExtensible extensibleDocumentFormat in Enumerable.Concat(inputCatalog.OfType<IExtensible>(), outputCatalog.OfType<IExtensible>()))
                {
                    var mediaConventionBuilder = new ConventionBuilder();
                    foreach (Type extensionType in extensibleDocumentFormat.ExtensionTypes)
                        mediaConventionBuilder.ForTypesDerivedFrom(extensionType).Export(x => x.AsContractType(extensionType));

                    ContainerConfiguration mediaContainerConfiguration = new ContainerConfiguration().WithAssemblies(assemblies, mediaConventionBuilder);
                    using (CompositionHost mediaContainer = mediaContainerConfiguration.CreateContainer())
                        extensibleDocumentFormat.Extend(mediaContainer);
                }
            }

            IDocumentInput<string> input = inputCatalog.BestMatch(file.Extension.TrimStart('.'));

            if (input == null)
            {
                System.Console.WriteLine("Error: No input format can handle that file extension !");
                return false;
            }

            var outputs = new List<IDocumentOutput<string>>();
            foreach (string outputExtension in outputExtensions)
            {
                IDocumentOutput<string> output = outputCatalog.BestMatch(outputExtension);

                if (output == null)
                {
                    System.Console.WriteLine($"Warning: \".{outputExtension}\" extension not find in output format catalog !");
                    continue;
                }

                outputs.Add(output);
            }

            IDocumentNode document = input.Convert(File.ReadAllText(file.FullName));
            foreach (IDocumentOutput<string> output in outputs)
            {
                System.Console.WriteLine($"Converting to {output.DisplayName}...");
                File.WriteAllText(Path.Combine(workingDirectory, Path.ChangeExtension(file.Name, output.FileExtension.Main)), output.Convert(document));
            }

            return true;
        }
    }
}

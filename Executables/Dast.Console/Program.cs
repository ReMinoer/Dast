using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Reflection;
using System.Runtime.Loader;
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
            }
        }
#endif

        static void Main(string[] args)
        {
#if DEBUG
            if (args.Length == 0)
                args = new []{ "../../../../../README.dh", "md" };
#endif

            if (Process(args))
                return;

            System.Console.WriteLine("Press any key to quit...");
            System.Console.ReadKey();
        }

        static bool Process(string[] args)
        {
            if (args.Length <= 1)
            {
                System.Console.WriteLine("Error: Not enough arguments !");
                System.Console.WriteLine("Usage: Dast.Console.exe <filePath> <outputExtension>+");
                return false;
            }

            string executableLocation = Assembly.GetEntryAssembly().Location;

            string filePath = Path.Combine(Path.GetDirectoryName(executableLocation), args[0]);
            var file = new FileInfo(filePath);
            if (!file.Exists)
            {
                System.Console.WriteLine($"Error: {file.FullName} not found !");
                return false;
            }

#if !DEBUG
            string pluginsPath = Path.Combine(Path.GetDirectoryName(executableLocation), PluginsDirectory);
            Directory.CreateDirectory(pluginsPath);

            Assembly[] assemblies = Directory.GetFiles(pluginsPath, "*.dll", SearchOption.AllDirectories).Select(AssemblyLoadContext.Default.LoadFromAssemblyPath).ToArray();
#else
            var assemblies = new List<Assembly>();
            foreach (string debugPlugin in DebugPlugins)
            {
                string pluginsPath = Path.Combine(Path.GetDirectoryName(executableLocation), debugPlugin);
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

            var conventionBuilder = new ConventionBuilder();
            conventionBuilder.ForTypesDerivedFrom<IDocumentInput<string>>().Export<IDocumentInput<string>>();
            conventionBuilder.ForTypesDerivedFrom<IDocumentOutput<string>>().Export<IDocumentOutput<string>>();

            ContainerConfiguration containerConfiguration = new ContainerConfiguration().WithAssemblies(assemblies, conventionBuilder);

            var inputCatalog = new DocumentFormatCatalog<IDocumentInput<string>>();
            var outputCatalog = new DocumentFormatCatalog<IDocumentOutput<string>>();

            using (CompositionHost container = containerConfiguration.CreateContainer())
            {
                IDocumentInput<string>[] documentInputs = container.GetExports<IDocumentInput<string>>().ToArray();
                if (documentInputs.Length == 0)
                {
                    System.Console.WriteLine("Error: No input format available !");
                    return false;
                }

                IDocumentOutput<string>[] documentOutputs = container.GetExports<IDocumentOutput<string>>().ToArray();
                if (documentOutputs.Length == 0)
                {
                    System.Console.WriteLine("Error: No output format available !");
                    return false;
                }

                inputCatalog.AddRange(documentInputs);
                outputCatalog.AddRange(documentOutputs);
            }

            IDocumentInput<string> input = inputCatalog.BestMatch(file.Extension.TrimStart('.'));

            if (input == null)
            {
                System.Console.WriteLine("Error: No input format can handle that file extension !");
                return false;
            }

            var outputs = new List<IDocumentOutput<string>>();
            for (int i = 1; i < args.Length; i++)
            {
                IDocumentOutput<string> output = outputCatalog.BestMatch(args[i]);

                if (output == null)
                {
                    System.Console.WriteLine($"Warning: \".{args[i]}\" extension not find in output format catalog !");
                    continue;
                }

                outputs.Add(output);
            }

            IDocumentNode document = input.Convert(File.ReadAllText(file.FullName));
            foreach (IDocumentOutput<string> output in outputs)
            {
                System.Console.WriteLine($"Converting to {output.DisplayName}...");
                File.WriteAllText(Path.ChangeExtension(file.FullName, output.FileExtension.Main), output.Convert(document));
            }

            return true;
        }
    }
}

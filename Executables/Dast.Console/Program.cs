using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Dast.Extensibility;

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
                            assemblies.Add(AssemblyLoadContext.Default.LoadFromAssemblyPath(dllPath));
                        }
                        catch (FileLoadException) { }
                }
                }
            }
#endif
            var converter = new ExtensibleDastConverter<string, string>();
            ExtensionsLoader.FromAssemblies(assemblies, converter);

            string inputExtension = file.Extension.TrimStart('.');
            string content = File.ReadAllText(file.FullName);
            
            foreach ((FileExtension extension, string result) output in converter.Convert(inputExtension, content, outputExtensions))
            {
                string outputFile = Path.Combine(workingDirectory, Path.ChangeExtension(file.Name, output.extension.Main));
                File.WriteAllText(outputFile, output.result);
            }

            return true;
        }
    }
}

using System.Collections.Generic;
using System.Diagnostics;
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
                args = new[] { "../../../../../External/DashCSharp/External/Dash/doc.dh", "md", "dh", "html" };
            }
#endif

            if (args.Length <= 1)
            {
                System.Console.WriteLine("Error: Not enough arguments !");
                System.Console.WriteLine("Usage: Dast.Console.exe <filePath> <outputExtension>+");
                goto PressAnyKey;
            }

            string filePath = args[0];
            string[] outputExtensions = args.Skip(1).ToArray();

            if (Process(filePath, outputExtensions))
                return;

        PressAnyKey:
            System.Console.WriteLine("Press any key to quit...");
            System.Console.ReadKey();
        }

        static private bool Process(string filePath, string[] outputExtensions)
        {
            string workingDirectory = Directory.GetCurrentDirectory();

            string rootedFilePath = Path.IsPathRooted(filePath) ? filePath : Path.Combine(workingDirectory, filePath);
            var file = new FileInfo(rootedFilePath);
            if (!file.Exists)
            {
                System.Console.WriteLine($"Error: {file.FullName} not found !");
                return false;
            }

            string inputExtension = file.Extension.TrimStart('.');

            var converter = new DastTextConverter();
            ExtensionsLoader.FromAssemblies(GetAssemblies(), converter);
            
            Stopwatch stopWatch = Stopwatch.StartNew();
            
            using (FileStream inputStream = file.OpenRead())
            {
                Dictionary<string, Stream> outputStreams = outputExtensions
                    .ToDictionary<string, string, Stream>(x => x, x => SafeFileCreate(Path.Combine(workingDirectory, Path.ChangeExtension(file.Name, x))));

                converter.Convert(inputExtension, inputStream, outputStreams);

                foreach (Stream outputStream in outputStreams.Values)
                    outputStream.Dispose();
            }

            //foreach ((string extension, string result) output in converter.Convert(inputExtension, File.ReadAllText(rootedFilePath), outputExtensions).Zip(outputExtensions, (o, e) => (e, o.result)))
            //    File.WriteAllText(Path.Combine(workingDirectory, Path.ChangeExtension(file.Name, output.extension)), output.result);

            stopWatch.Stop();
            System.Console.WriteLine(stopWatch.Elapsed.TotalSeconds);

            return true;
        }

        static private FileStream SafeFileCreate(string path)
        {
            try
            {
                return File.Create(path);
            }
            catch (IOException)
            {
                return File.Create(path);
            }
        }

        static private IEnumerable<Assembly> GetAssemblies()
        {
            string executableDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

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
            return assemblies;
        }
    }
}

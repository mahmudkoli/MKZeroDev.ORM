﻿using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace MKZeroDev.ORM.CLI
{
    internal class Program
    {
        private static string _projDir = Directory.GetCurrentDirectory();
        private static string _outputDir = @"bin\Debug";
        private static bool _buildAssembly = false;
        private static string _connStr = default!;

        static void Main(string[] args)
        {
            try
            {
                //Test();

                if (args == null || args.Length == 0)
                {
                    ShowGeneralHelp();
                    return;
                }

                if (args[0] == "--help" || args[0] == "-h")
                {
                    ShowCommandHelp();
                    return;
                }

                if (args[0] == "--info" || args[0] == "-i")
                {
                    ShowMessage("Information...");
                    return;
                }

                if (args[0] != "orm")
                {
                    ShowInvalidCommandError();
                    return;
                }

                if (args.Length == 1 && args[0] == "orm")
                {
                    ShowCommandHelp();
                    return;
                }

                if (args.Length > 1 && (args[1] == "--help" || args[1] == "-h"))
                {
                    ShowCommandHelp();
                    return;
                }

                if (args.Length > 1 && (args[1] == "--version" || args[1] == "-v"))
                {
                    ShowMessage("Version...");
                    return;
                }

                if (args.Length > 1 && (args[1] == "database-update"))
                {
                    var connStrIn = args.ToList().IndexOf("--connStr");
                    if (connStrIn > -1 && args.Length > (connStrIn + 1) && !string.IsNullOrEmpty(args[connStrIn + 1]))
                    {
                        _connStr = args[connStrIn + 1];
                    }

                    var projDirIn = args.ToList().IndexOf("--projDir");
                    if (projDirIn > -1 && args.Length > (projDirIn + 1) && !string.IsNullOrEmpty(args[projDirIn + 1]))
                    {
                        _projDir = args[projDirIn + 1];
                    }

                    var outputDirIn = args.ToList().IndexOf("--outDir");
                    if (outputDirIn > -1 && args.Length > (outputDirIn + 1) && !string.IsNullOrEmpty(args[outputDirIn + 1]))
                    {
                        _outputDir = args[outputDirIn + 1];
                    }

                    var buildAsm = args.ToList().IndexOf("--build");
                    if (buildAsm > -1)
                    {
                        _buildAssembly = true;
                    }

                    if (!string.IsNullOrEmpty(_connStr) && !string.IsNullOrEmpty(_projDir) && !string.IsNullOrEmpty(_outputDir))
                    {
                        DatabaseUpdate();
                        return;
                    }
                }

                ShowInvalidCommandError();
                return;
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message;

                // can't catch internal type
                if (ex.StackTrace?.Contains("ThrowOperationCanceledException") ?? false)
                    return;

                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }

                ShowError(errorMsg, ex.StackTrace, ex.Source);
            }
        }

        static void Test()
        {
            _projDir = @"D:\Practice\MKZeroDev\MKZeroDev.ORM\MKZeroDev.ORMTest";
            _outputDir = @"bin\Debug";
            _connStr = @"Server=.\SQLEXPRESS;Database=ORMCore;Trusted_Connection=true";
            _buildAssembly = true;
            DatabaseUpdate();
        }

        static void ShowError(string message, string? stackTrace = null, string? source = null)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("Something went wrong!!!");
            stringBuilder.AppendLine(message);
            stringBuilder.AppendLine();

            Console.WriteLine(stringBuilder.ToString());
        }

        static void ShowGeneralHelp()
        {
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine();
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var ver = version?.Major + "." + version?.Minor +
                      (version?.Build > 0 ? "." + version?.Build : string.Empty);

            Console.WriteLine($"Greetings from MAHMUD KOLI");
            Console.WriteLine($"MKZeroDev CLI Version {ver}");
            Console.WriteLine();

            Console.ResetColor();

            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("Usage: mkzerodev [options]");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Options:");
            stringBuilder.AppendLine("\t-h|--help\t\tDisplay help.");
            stringBuilder.AppendLine("\t--info\t\tDisplay MKZeroDev information.");

            Console.WriteLine(stringBuilder.ToString());
        }

        static void ShowCommandHelp()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("Usage: mkzerodev orm [options] [command]");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Options:");
            stringBuilder.AppendLine("\t--version\t\tShow version information");
            stringBuilder.AppendLine("\t-h|--help\t\tShow help information");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Commands:");
            stringBuilder.AppendLine("\tdatabase-update\t\tCommands to update the database.");

            Console.WriteLine(stringBuilder.ToString());
        }

        static void ShowInvalidCommandError()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("Could not execute because the specified command or option was not found.");
            stringBuilder.AppendLine("Possible reasons for this include:");
            stringBuilder.AppendLine("\t*You misspelled a built-in mkzerodev command.");
            stringBuilder.AppendLine("\t*You intended to execute a MKZeroDev program, but mkzerodev-orm does not exist.");
            stringBuilder.AppendLine("\t*You intended to run a global tool, but a mkzerodev-prefixed executable with this name could not be found on the PATH.");

            Console.WriteLine(stringBuilder.ToString());
        }

        static void ShowMessage(string msg)
        {
            Console.WriteLine(msg);
        }

        static void DatabaseUpdate()
        {
            var assemblies = BuildProjectAssembly(_projDir);
            var ormDatabaseInheritedTypes = GetInheritedTypesFromAssemblies<ORMDatabase>(assemblies);

            foreach (Type type in ormDatabaseInheritedTypes)
            {
                Console.WriteLine($"Database {type.ToString()} updating...");
                var obj = (ORMDatabase?)Activator.CreateInstance(type, new object[] { _connStr });
                obj?.DatabaseUpdate();
                Console.WriteLine($"Database {obj?.ToString()} has been updated successfully.");
            }

            Console.WriteLine("Database update completed.");
        }

        #region Build assembly project
        static IList<Type> GetInheritedTypesFromAssemblies<T>(IList<Assembly> assemblies)
        {
            var inheritedTypes = new List<Type>();

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(T))).ToList();
                inheritedTypes.AddRange(types);
            }

            return inheritedTypes;
        }

        static IList<Assembly> BuildProjectAssembly(string projDir)
        {
            var assemblies = new List<Assembly>();
            var projFiles = Directory.GetFiles(projDir, "*.csproj", SearchOption.TopDirectoryOnly);

            if (_buildAssembly)
            {
                foreach (var csprojFile in projFiles)
                {
                    Console.WriteLine($"Project {Path.GetFileName(csprojFile)} building...");
                    BuildAssemblyMSBuildWithRegister(csprojFile, _outputDir);
                    Console.WriteLine($"Project {Path.GetFileName(csprojFile)} has been built successfully.");
                }

                Console.WriteLine("Project build completed.");
            }

            var dllFilesName = projFiles.Select(csp => Path.GetFileNameWithoutExtension(csp) + ".dll").ToList();

            var dllFiles = new List<string>();
            var dllDir = Path.Combine(projDir, _outputDir);

            foreach (var dllFileName in dllFilesName)
            {
                var dfs = Directory.GetFiles(dllDir, dllFileName, SearchOption.AllDirectories);
                dllFiles.AddRange(dfs);
            }

            foreach (var dllFile in dllFiles)
            {
                Assembly assembly = Assembly.LoadFile(dllFile);

                if (!IsAssemblyDebugBuild(assembly)) continue;

                assemblies.Add(assembly);
            }

            return assemblies;
        }

        static bool IsAssemblyDebugBuild(Assembly assembly)
        {
            return assembly.GetCustomAttributes(false).OfType<DebuggableAttribute>().Any(da => da.IsJITTrackingEnabled);
        }

        //https://learn.microsoft.com/en-us/visualstudio/msbuild/updating-an-existing-application?view=vs-2022#use-microsoftbuildlocator
        static void BuildAssemblyMSBuildWithRegister(string projFile, string outputDir)
        {
            MSBuildLocator.RegisterDefaults();
            DirectlyBuildAssemblyMSBuild(projFile, outputDir);
        }

        static void DirectlyBuildAssemblyMSBuild(string projFile, string outputDir)
        {
            var outputPath = Path.Combine(Path.GetDirectoryName(projFile) ?? string.Empty, outputDir);
            Project project = new Project(projFile);

            project.SetGlobalProperty("Configuration", "Debug");
            project.SetGlobalProperty("Platform", "Any CPU");
            project.SetGlobalProperty("OutputPath", outputPath);

            project.Build();
        }
        #endregion
    }
}
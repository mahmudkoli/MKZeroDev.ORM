﻿using Microsoft.VisualBasic;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

namespace MKZeroDev.ORM.CLI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //DatabaseUpdate();
            //Console.ReadLine();
            try
            {
                Console.WriteLine(string.Join("  000  ", args));

                var version = Assembly.GetExecutingAssembly().GetName().Version;
                var ver = version.Major + "." + version.Minor +
                          (version.Build > 0 ? "." + version.Build : string.Empty);

                Console.WriteLine(ver);

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

                if (args[0] == "--info")
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

                if (args.Length > 1 && (args[1] != "database-update"))
                {
                    ShowInvalidCommandError();
                    return;
                }

                if (args.Length > 1 && (args[1] == "database-update"))
                {
                    DatabaseUpdate();
                    return;
                }
            }
            catch (Exception ex)
            {
                // can't catch internal type
                if (ex.StackTrace.Contains("ThrowOperationCanceledException"))
                    return;

                ShowError(ex.Message, ex.StackTrace, ex.Source);
            }
        }

        static void ShowError(string message, string stackTrace = null, string source = null)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("Something went wrong!");
            stringBuilder.AppendLine();

            if (!string.IsNullOrEmpty(stackTrace))
            {
                stringBuilder.AppendLine("----- Error Info -----");
                stringBuilder.AppendLine(stackTrace);
                stringBuilder.AppendLine(source);
                stringBuilder.AppendLine("----------------------");
            }

            Console.WriteLine(stringBuilder.ToString());
        }

        static void ShowGeneralHelp()
        {
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
            string className = "Program";

            var classes = Assembly.GetExecutingAssembly().GetTypes().Where(t =>  t.IsClass && t.Name == className);

            foreach (var item in classes)
            {
                Console.WriteLine(item.FullName);
            }

            Console.WriteLine("Database has been updated successfully.");

            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

            loadedAssemblies.SelectMany(x => x.GetReferencedAssemblies())
                            .Distinct()
                            .Where(y => loadedAssemblies.Any((a) => a.FullName == y.FullName) == false)
                            .ToList()
                            .ForEach(x => loadedAssemblies.Add(AppDomain.CurrentDomain.Load(x)));

            foreach (var item in loadedAssemblies.SelectMany(x => x.GetTypes()).Where(t => t.IsClass && t.Name == className))
            {
                Console.WriteLine(item.FullName);
            }

            Console.WriteLine("Database has been updated successfully.");
        }
    }
}
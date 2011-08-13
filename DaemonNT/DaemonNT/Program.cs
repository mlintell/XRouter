﻿namespace DaemonNT
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DaemonNT.Installation;

    /// <summary>
    /// Provides a console program to run various DaemonNT commands.
    /// </summary>
    /// <remarks>
    /// The program can be ran with the following available arguments:
    /// <code>
    /// DaemonNT.exe [options] {command} [service-name]
    /// {command} ::= debug | run | install | uninstall | start | stop
    ///     | restart | status | list
    /// </code>
    /// </remarks>
    internal class Program
    {
        public static void Main(string[] args)
        {
            #region Parse command-line arguments

            // TODO: Lepe poresit parametry prikazove radky (az bude definitivne jiste,
            // co vsechno bude v sobe DaemonNT obsahovat - pokud budeme napr. implementovat
            // watchdog ci konfiguracni GUI)

            // TODO:
            // - if the argument parsing gets too complex better use a specialized library

            List<string> arguments = new List<string>();
            arguments.AddRange(args);

            if ((arguments.Count > 0) && (arguments[0] == "DaemonNT"))
            {
                arguments.RemoveAt(0);
            }

            if ((arguments.Count < 1))
            {
                PrintUsage();
                return;
            }

            ServiceCommands commands = new ServiceCommands();

            // non-mandatory option parameters
            while ((arguments.Count > 0) && arguments[0].StartsWith("-"))
            {
                if (arguments[0].StartsWith("--config-file="))
                {
                    commands.ConfigFile = arguments[0].Split(new[] { '=' }, 2)[1];
                }
                arguments.RemoveAt(0);
            }

            if (arguments.Count < 1)
            {
                PrintUsage();
                return;
            }

            string command = arguments[0];
            string serviceName = string.Empty;
            if (command != "list")
            {
                if (arguments.Count == 2)
                {
                    serviceName = arguments[1];
                }
                else
                {
                    PrintUsage();
                    return;
                }
            }

            #endregion

            #region Execute the requested command

            try
            {
                switch (command)
                {
                    case "run":
                        commands.Run(serviceName);
                        break;
                    case "debug":
                        commands.DebugStart(serviceName);
                        break;
                    case "install":
                        commands.Install(serviceName);
                        break;
                    case "uninstall":
                        commands.Uninstall(serviceName);
                        break;
                    case "start":
                        commands.Start(serviceName);
                        break;
                    case "stop":
                        commands.Stop(serviceName);
                        break;
                    case "restart":
                        commands.Restart(serviceName);
                        break;
                    case "status":
                        CheckStatus(commands, serviceName);
                        break;
                    case "list":
                        ListServices(commands);
                        break;
                    default:
                        PrintUsage();
                        return;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error: {0}", ex.Message);
                Environment.Exit(-1);
            }

            #endregion
        }

        private static void CheckStatus(ServiceCommands commands, string serviceName)
        {
            bool isInstalled = InstallerServices.IsInstalled(serviceName);
            if (isInstalled)
            {
                string status = commands.GetStatus(serviceName);
                Console.WriteLine("Status of service '{0}': {1}.", serviceName, status);
            }
            else
            {
                Console.WriteLine("Service '{0}' is not installed.", serviceName);
            }
        }

        private static void ListServices(ServiceCommands commands)
        {
            List<string> services = commands.ListServices().ToList();
            services.Sort();
            var installedServiceNames = Installation.InstallerServices.GetInstalledServices().Select((service) => service.ServiceName);
            Console.WriteLine("Configured services:");
            foreach (var service in services)
            {
                bool installed = installedServiceNames.Contains(service);
                Console.WriteLine("{0}{1}", service, installed ? " - installed" : "");
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: DaemonNT.exe [options] COMMAND [SERVICE_NAME]");
            Console.WriteLine("Available commands: debug, run, install, uninstall, start, stop, restart, status, list.");
            Console.WriteLine("The 'run' command is indended only for the OS's service runner.");
            Console.WriteLine("SERVICE_NAME is needed for all commands except 'list'.");
            Console.WriteLine("Options:");
            Console.WriteLine("  --config-file=CONFIG_FILE - path to configuration file");
        }
    }
}
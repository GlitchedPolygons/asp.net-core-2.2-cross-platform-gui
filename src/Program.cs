// Copyright (c) 2019, Raphael Beck. All rights reserved.
// Use of this source code is governed by the BSD 3-Clause license that can be found in the repository root directory's LICENSE file.

#define OPEN_IN_BROWSER
// Comment away the above line of code if you want to run the app in the background
// instead of opening it up in the browser on startup (useful if you're bridging with Electron or so).

// #define ALLOW_MULTIPLE_INSTANCES
// Uncomment the above line to allow >1 instances of this application to be run simultaneously.

using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using CrossPlatformGUI.Utilities;
using CrossPlatformGUI.Extensions;

namespace CrossPlatformGUI
{
    /// <summary>
    /// The application's entry point class.<para> </para>
    /// This is where the main method resides.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Root directory (absolute path to where the running application's executable resides).
        /// </summary>
        public static readonly string ROOT_DIRECTORY = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);

        /// <summary>
        /// wwwroot folder path.
        /// </summary>
        public static readonly string WWWROOT_DIRECTORY = Path.Combine(ROOT_DIRECTORY, "wwwroot");

        /// <summary>
        /// HTTPS SSL Certificate file path.
        /// </summary>
        public static readonly string CERTIFICATE_FILE_PATH = Path.Combine(WWWROOT_DIRECTORY, "ssl", "127.0.0.1.p12");

        /// <summary>
        /// The client version number.
        /// </summary>
        public static string Version => Assembly.GetEntryAssembly()?.GetName().Version.ToString();

        /// <summary>
        /// The port number on which the application is hosted.
        /// </summary>
        public static int Port { get; private set; } = -1;

        /// <summary>
        /// The URL where you can reach the application inside your browser.
        /// </summary>
        public static string Url { get; private set; }

#if !ALLOW_MULTIPLE_INSTANCES
        /// <summary>
        /// <see cref="Mutex"/> responsible for ensuring that
        /// there is always only exactly one instance of the app running.
        /// </summary>
        private static Mutex mutex;
#endif

        public static async Task Main(string[] args)
        {
            Console.WriteLine();
            Console.OutputEncoding = System.Text.Encoding.UTF8;

#if !ALLOW_MULTIPLE_INSTANCES
            mutex = new Mutex(true, $"CrossPlatformGUI_Desktop_{Version}", out bool newInstance);

            if (!newInstance)
            {
                PrintSingleInstanceError();
                return;
            }
#endif

            Port = ConfigurePort();
            Url = $"https://127.0.0.1:{Port}";

            try
            {
                var webHost = CreateWebHostBuilder(Port).UseUrls(Url + ';').Build();
                webHost.Start();

#if OPEN_IN_BROWSER
                Url.OpenUrlInBrowser();
#endif
                Console.WriteLine($"PORT={Port}");

                await webHost.WaitForShutdownAsync();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.ToString());
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Creates the <see cref="IWebHostBuilder"/> needed for starting the web app inside <see cref="Main"/>.
        /// </summary>
        /// <param name="port">localhost port where the app will be hosted.</param>
        /// <returns><see cref="IWebHostBuilder"/></returns>
        public static IWebHostBuilder CreateWebHostBuilder(int port)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true, true)
                .Build();

            var certificate = ConfigureSSL();

            var builder = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    options.AddServerHeader = false;
                    options.Listen(IPAddress.Loopback, port, listenOptions => { listenOptions.UseHttps(certificate); });
                })
                .UseConfiguration(config)
                .UseContentRoot(Directory.GetCurrentDirectory());

            return builder.UseStartup<Startup>();
        }

        private static int ConfigurePort()
        {
            string portPath = Path.Combine(ROOT_DIRECTORY, "Port");

            if (!File.Exists(portPath))
            {
                File.WriteAllText(portPath, "19962");
            }

            if (!int.TryParse(File.ReadAllText(portPath), out int port) || port < 5000)
            {
                port = 19962;
                File.WriteAllText(portPath, port.ToString());
            }

            if (!PortUtility.IsPortAvailable(port))
            {
                port = PortUtility.GetFirstAvailablePort(5000);
            }

            return port;
        }

        private static X509Certificate2 ConfigureSSL()
        {
            if (File.Exists(CERTIFICATE_FILE_PATH))
            {
                return new X509Certificate2(CERTIFICATE_FILE_PATH, "changeit");
            }

            string dir = Path.GetDirectoryName(CERTIFICATE_FILE_PATH);
            Directory.CreateDirectory(dir);

            var processStartInfo = new ProcessStartInfo
            {
                WorkingDirectory = dir,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Console.WriteLine();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string mkcert = "mkcert-v1.3.0-windows-amd64.exe";
                processStartInfo.FileName = "cmd.exe";
                processStartInfo.Arguments = $"/c {mkcert} -install && {mkcert} -pkcs12 127.0.0.1";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string mkcert = "mkcert-v1.3.0-linux-amd64";
                processStartInfo.FileName = "/bin/bash";
                processStartInfo.Arguments = $"-c \"sudo chmod a+x {mkcert} && sudo ./{mkcert} -install && sudo ./{mkcert} -pkcs12 127.0.0.1\"";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                string mkcert = "mkcert-v1.3.0-darwin-amd64";
                processStartInfo.FileName = "/bin/bash";
                processStartInfo.Arguments = $"-c \"sudo chmod a+x {mkcert} && sudo ./{mkcert} -install && sudo ./{mkcert} -pkcs12 127.0.0.1\"";
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                throw new PlatformNotSupportedException();
            }

            Console.WriteLine("\nSSL Certificate generation started using mkcert - timeout set to 120s\n");

            var process = new Process {StartInfo = processStartInfo};
            process.Start();

            if (!process.WaitForExit(120000))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                throw new ApplicationException("SSL Certificate generation using mkcert timed out after 2 minutes of unresponsiveness...");
            }

            if (process.ExitCode != 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                throw new ApplicationException("SSL Certificate generation failed - check the logs");
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nSSL Certificate generated successfully: {CERTIFICATE_FILE_PATH}\n");
            Console.ResetColor();

            return new X509Certificate2(CERTIFICATE_FILE_PATH, "changeit");
        }

        private static void PrintSingleInstanceError()
        {
            Console.WriteLine(File.ReadAllText("wwwroot/imgs/ascii/error"));
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n  ERROR: There is already one instance of this application running!\n");
            Console.ResetColor();
            Console.WriteLine("\n  Press any button to exit...");
            Console.ReadKey(true);
        }
    }
}
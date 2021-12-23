using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace NewzNabAggregator.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var cla = new CommandLineApplication(throwOnUnexpectedArg: true);
            var configFileOption = cla.Option("--config <configuration_filename>", "File name of configuration", CommandOptionType.SingleValue);
            cla.Execute(args);

            CreateHostBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    if (configFileOption.HasValue())
                    {
                        config.AddJsonFile(configFileOption.Value());
                    }
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.AddFilter((provider, category, logLevel) => provider.StartsWith("Microsoft.AspNetCore") && logLevel >= LogLevel.Warning);
                })
                .Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var builder = new ConfigurationBuilder().AddCommandLine(args).AddEnvironmentVariables(delegate (EnvironmentVariablesConfigurationSource s)
            {
                s.Prefix = "NNA_";
            });
            var startupConfig = builder.Build();
            var baseDir = startupConfig.GetValue("baseDir", $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}{Path.DirectorySeparatorChar}NewzNabAggregator{Path.DirectorySeparatorChar}");
            Environment.SetEnvironmentVariable("NNA_baseDir", baseDir);
            builder = new ConfigurationBuilder().SetBasePath(baseDir).AddJsonFile("config.json", optional: true, reloadOnChange: false).AddEnvironmentVariables(delegate (EnvironmentVariablesConfigurationSource s)
            {
                s.Prefix = "NNA_";
            });
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (env != null)
            {
                builder.AddJsonFile("config." + env + ".json", optional: true, reloadOnChange: false);
            }
            return Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(delegate (IWebHostBuilder webBuilder)
            {
                webBuilder.UseConfiguration(builder.Build()).UseStartup<Startup>();
            });
        }
    }
}

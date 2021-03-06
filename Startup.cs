﻿using System;
using System.IO;
using System.Threading.Tasks;
using AshBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AshBot
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(string[] args)
        {
            string path = AppContext.BaseDirectory; 
            //string newPath = Path.GetFullPath(Path.Combine(path, @"..\"));

            var builder = new ConfigurationBuilder()         // Specify the default location for the config file
                .AddJsonFile("/botbuild/Release/config.json");                // Add this file to the configuration
            Configuration = builder.Build();                // Build the configuration
        }

        public static async Task RunAsync(string[] args)
        {
            var startup = new Startup(args);
            await startup.RunAsync();
        }

        public async Task RunAsync()
        {
            var services = new ServiceCollection();             // Create a new instance of a service collection
            ConfigureServices(services);

            var provider = services.BuildServiceProvider();     // Build the service provider
            provider.GetRequiredService<LoggingService>();      // Start the logging service
            provider.GetRequiredService<CommandHandler>(); 		// Start the command handler service
            provider.GetRequiredService<JudgementService>();    // Start the judgement service
            provider.GetRequiredService<AudioService>();

            await provider.GetRequiredService<StartupService>().StartAsync();       // Start the startup service
            await Task.Delay(-1);                               // Keep the program alive
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {                                       // Add discord to the collection
                LogLevel = LogSeverity.Verbose,     // Tell the logger to give Verbose amount of info
                MessageCacheSize = 1000             // Cache 1,000 messages per channel
            }))
            .AddSingleton(new CommandService(new CommandServiceConfig
            {                                       // Add the command service to the collection
                LogLevel = LogSeverity.Verbose,     // Tell the logger to give Verbose amount of info
                DefaultRunMode = RunMode.Async,     // Force all commands to run async by default
            }))
            .AddSingleton<CommandHandler>()         // Add the command handler to the collection
            .AddSingleton<StartupService>()         // Add startupservice to the collection
            .AddSingleton<LoggingService>()         // Add loggingservice to the collection
            .AddSingleton<JudgementService>()       //Add judgement service to the collection
            .AddSingleton<AudioService>()
            .AddSingleton<Random>()                 // Add random to the collection
            .AddSingleton(Configuration);           // Add the configuration to the collection
        }
    }
}

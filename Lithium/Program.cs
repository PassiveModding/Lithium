﻿using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Lithium.Discord.Contexts;
using Lithium.Handlers;
using Lithium.Models;
using Lithium.Services;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Serilog;
using EventHandler = Lithium.Handlers.EventHandler;

namespace Lithium
{
    public class Program
    {
        private EventHandler _handler;
        public DiscordSocketClient Client;

        public static void Main(string[] args)
        {
            new Program().Start().GetAwaiter().GetResult();
        }

        public async Task Start()
        {
            Console.Title = "Lithium Discord Bot by Passive";

            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "setup/")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "setup/"));
            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "setup/backups")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "setup/backups"));
            Config.CheckExistence();
            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                MessageCacheSize = 50
            });


            try
            {
                await Client.LoginAsync(TokenType.Bot, Config.Load().BotToken);
                await Client.StartAsync();
            }
            catch (Exception e)
            {
                Log.Information("------------------------------------\n" +
                                $"{e}\n" +
                                "------------------------------------\n" +
                                "Token was rejected by Discord (Invalid Token or Connection Error)\n" +
                                "------------------------------------");
            }


            var serviceProvider = ConfigureServices();
            _handler = new EventHandler(serviceProvider);
            await _handler.ConfigureAsync();
            Client.Log += Client_Log;
            await Task.Delay(-1);
        }

        private static Task Client_Log(LogMessage arg)
        {
            Logger.LogInfo(arg.Message);
            return Task.CompletedTask;
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection()
                .AddSingleton(Client)
                .AddSingleton(new DocumentStore
                {
                    Database = DatabaseHandler.DBName,
                    Urls = new[] {DatabaseHandler.ServerURL}
                }.Initialize())
                .AddSingleton(new DatabaseHandler())
                .AddSingleton(new TimerService(Client))
                .AddSingleton(new InteractiveService(Client))
                .AddSingleton(new CommandService(
                    new CommandServiceConfig
                    {
                        CaseSensitiveCommands = false,
                        ThrowOnError = false,
                        DefaultRunMode = RunMode.Async
                    }));

            return services.BuildServiceProvider();
        }
    }
}
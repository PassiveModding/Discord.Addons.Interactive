namespace SampleBot
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Addons.Interactive;
    using Discord.Commands;
    using Discord.WebSocket;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// The program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The client.
        /// </summary>
        private static DiscordSocketClient client;

        /// <summary>
        /// The handler.
        /// </summary>
        private static CommandHandler handler;

        public static async Task Main(string[] args)
        {
            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "setup")))
            {
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "setup"));
            }

            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info
            });

            var token = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "setup", "token.txt"));

            try
            {
                await client.LoginAsync(TokenType.Bot, token);
                await client.StartAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine("Discord Token Rejected\n" +
                                $"{e}");
            }

            var serviceProvider = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(new InteractiveService(client))
                .AddSingleton(new CommandService(
                    new CommandServiceConfig { CaseSensitiveCommands = false, ThrowOnError = false })).BuildServiceProvider();
            handler = new CommandHandler(serviceProvider);
            await handler.ConfigureAsync();

            client.Log += Client_LogAsync;
            await Task.Delay(-1);
        }

        private static Task Client_LogAsync(LogMessage arg)
        {
            Console.WriteLine(arg.Message);
            return Task.CompletedTask;
        }
    }
}
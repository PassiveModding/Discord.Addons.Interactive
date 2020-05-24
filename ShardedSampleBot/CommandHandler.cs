using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace ShardedSampleBot
{
    /// <summary>
    /// The command handler.
    /// </summary>
    public class CommandHandler
    {
        /// <summary>
        /// The discord client.
        /// </summary>
        private readonly DiscordShardedClient client;

        /// <summary>
        /// The command service
        /// </summary>
        private readonly CommandService commandService;

        /// <summary>
        /// The service provider.
        /// </summary>
        private readonly IServiceProvider provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandler"/> class.
        /// </summary>
        /// <param name="provider">
        /// The provider.
        /// </param>
        public CommandHandler(IServiceProvider provider)
        {
            this.provider = provider;
            client = this.provider.GetService<DiscordShardedClient>();
            commandService = new CommandService();

            client.MessageReceived += Client_MessageReceivedAsync;
            client.ShardReady += Client_ReadyAsync;
        }
        
        /// <summary>
        /// Initializes all bot modules
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public Task ConfigureAsync()
        {
            return commandService.AddModulesAsync(Assembly.GetEntryAssembly(), provider);
        }

        /// <summary>
        /// The ready event
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task Client_ReadyAsync(DiscordSocketClient shard)
        {
            var application = await client.GetApplicationInfoAsync();
            Console.WriteLine($"Invite: https://discordapp.com/oauth2/authorize?client_id={application.Id}&scope=bot&permissions=2146958591");
        }

        /// <summary>
        /// The _client_ message received async.
        /// </summary>
        /// <param name="socketMessage">
        /// The socket message.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task Client_MessageReceivedAsync(SocketMessage socketMessage)
        {
            // Check to ensure that we are only receiving valid messages from users only!
            if (!(socketMessage is SocketUserMessage message))
            {
                return;
            }

            var context = new ShardedCommandContext(client, message);
            if (context.User.IsBot)
            {
                return;
            }

            var argPos = 0;

            // Ensure that we filter out all messages that do not start with the bot prefix
            if (!(message.HasMentionPrefix(client.CurrentUser, ref argPos) || message.HasStringPrefix(".", ref argPos)))
            {
                return;
            }

            var result = await commandService.ExecuteAsync(context, argPos, provider);
            if (result.IsSuccess)
            {
                Console.WriteLine("Command Success");
            }
            else
            {
                try
                {
                    await context.Channel.SendMessageAsync(string.Empty, false, new EmbedBuilder
                    {
                        Title = $"{context.User.Username.ToUpper()} ERROR",
                        Description = result.ErrorReason
                    }.Build());
                }
                catch
                {
                    // Ignore errors
                }
            }
        }
    }
}
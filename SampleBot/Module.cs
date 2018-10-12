// ReSharper disable All

namespace SampleBot
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Addons.Interactive;
    using Discord.Commands;

    public class Module : InteractiveBase
    {
        // DeleteAfterAsync will send a message and asynchronously delete it after the timeout has popped
        // This method will not block.
        [Command("delete")]
        public async Task<RuntimeResult> Test_DeleteAfterAsync()
        {
            await ReplyAndDeleteAsync("this message will delete in 10 seconds", timeout: TimeSpan.FromSeconds(10));
            return Ok();
        }

        // NextMessageAsync will wait for the next message to come in over the gateway, given certain criteria
        // By default, this will be limited to messages from the source user in the source channel
        // This method will block the gateway, so it should be ran in async mode.
        [Command("next", RunMode = RunMode.Async)]
        public async Task Test_NextMessageAsync()
        {
            await ReplyAsync("What is 2+2?");
            var response = await NextMessageAsync();
            if (response != null)
                await ReplyAsync($"You replied: {response.Content}");
            else
                await ReplyAsync("You did not reply before the timeout");
        }

        // PagedReplyAsync will send a paginated message to the channel
        // You can customize the paginator by creating a PaginatedMessage object
        // You can customize the criteria for the paginator as well, which defaults to restricting to the source user
        // This method will not block.
        [Command("paginator")]
        public async Task Test_Paginator()
        {
            var pages = new[]
            {
                new PaginatedMessage.Page
                {
                    Description = "Page1"
                },
                new PaginatedMessage.Page
                {
                    Author = new EmbedAuthorBuilder
                    {
                        IconUrl = Context.User.GetAvatarUrl(),
                        Name = Context.User.ToString(),
                        Url = Context.User.GetAvatarUrl()
                    },
                    Description = "Page 2 Description",
                    Title = "Page 2 Title",
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder
                        {
                            Name = "Field 1",
                            Value = "Field 1 Description"
                        }
                    },
                    ImageUrl = "https://discordapp.com/assets/9c38ca7c8efaed0c58149217515ea19f.png",
                    Color = Color.DarkMagenta,
                    FooterOverride = new EmbedFooterBuilder
                    {
                        IconUrl = Context.User.GetAvatarUrl(),
                        Text = Context.User.ToString()
                    },
                    ThumbnailUrl = Context.User.GetAvatarUrl(),
                    TimeStamp = DateTimeOffset.UtcNow,
                    Url = Context.User.GetAvatarUrl()
                },
                new PaginatedMessage.Page
                {
                    Author = new EmbedAuthorBuilder
                    {
                        IconUrl = Context.User.GetAvatarUrl(),
                        Name = Context.User.ToString(),
                        Url = Context.User.GetAvatarUrl()
                    },
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder
                        {
                            Name = "Field 1",
                            Value = "Field 1 Description"
                        }
                    },
                    Color = Color.DarkMagenta,
                    ThumbnailUrl = Context.User.GetAvatarUrl(),
                    TimeStamp = DateTimeOffset.UtcNow,
                    Url = Context.User.GetAvatarUrl()
                }
            };

            var pager = new PaginatedMessage
            {
                Pages = pages,
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                    Name = Context.Client.CurrentUser.ToString(),
                    Url = Context.Client.CurrentUser.GetAvatarUrl()
                },
                Color = Color.DarkGreen,
                Content = "Default Message Content",
                Description = "Default Embed Description",
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Default Field 1",
                        Value = "Default Field Desc 1"
                    }
                },
                FooterOverride = null,
                ImageUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                Options = PaginatedAppearanceOptions.Default,
                TimeStamp = DateTimeOffset.UtcNow
            };

            await PagedReplyAsync(pager, new ReactionList
            {
                Forward = true,
                Backward = true,
                Jump = true,
                Trash = true
            });
        }

        // InlineReactionReplyAsync will send a message and adds reactions on it.
        // Once an user adds a reaction, the callback is fired.
        // If callback was successfull next callback is not handled
        // Unsuccessful callback is a reaction that did not have a callback.
        [Command("reaction")]
        public async Task Test_ReactionReply()
        {
            //This message does not expire after a single
            //it will not allow a user to react more than once
            //it allows more than one user to react
            //it will expire after 60 seconds
            await InlineReactionReplyAsync(new ReactionCallbackData("text", null, false, true, TimeSpan.FromSeconds(60), c => c.Channel.SendMessageAsync("Timed Out!"))
                .WithCallback(new Emoji("👍"), (c, r) => c.Channel.SendMessageAsync($"{r.User.Value.Mention} replied with 👍"))
                .WithCallback(new Emoji("👎"), (c, r) => c.Channel.SendMessageAsync($"{r.User.Value.Mention} replied with 👎"))
            , false);
        }

        [Command("embedreaction")]
        public async Task Test_EmedReactionReply(bool expiresafteruse, bool singleuseperuser, bool sourceuser)
        {
            var one = new Emoji("1⃣");
            var two = new Emoji("2⃣");

            var embed = new EmbedBuilder()
                .WithTitle("Choose one")
                .AddField(one.Name, "Beer", true)
                .AddField(two.Name, "Drink", true)
                .Build();

            await InlineReactionReplyAsync(new ReactionCallbackData("text", embed, expiresafteruse, singleuseperuser, TimeSpan.FromSeconds(20), (c) => c.Channel.SendMessageAsync("Timed Out!"))
                .WithCallback(one, (c, r) => c.Channel.SendMessageAsync($"{r.User.Value.Mention} Here you go :beer:"))
                .WithCallback(two, (c, r) => c.Channel.SendMessageAsync($"{r.User.Value.Mention} Here you go :tropical_drink:")), sourceuser
            );
        }
    }
}
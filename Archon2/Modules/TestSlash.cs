using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;

namespace Archon2.Modules
{
    public class TestSlash : ApplicationCommandModule
    {
        [SlashCommand("ping", "Pings.")]
        public async Task Ping(InteractionContext c)
        {
            var options = new List<DiscordSelectComponentOption>()
            {
                new DiscordSelectComponentOption("Then learn how to!", "learn"),
                new DiscordSelectComponentOption("That's understandable.", "understand"),
            };

            var dropdown = new DiscordSelectComponent("dropdown", null, options);

            var embed = Utils.Embed().WithDescription("")
                .WithTitle("Pong!")
                .WithDescription("I have no clue how to get client latency yet.");

            await c.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .AddEmbed(embed.Build()).AddComponents(dropdown));

            var intr = c.Client.GetInteractivity();
            var r = await intr.WaitForSelectAsync(await c.GetOriginalResponseAsync(), "dropdown");
           
            switch (r.Result.Values[0])
            {
                case "learn":
                    var embed2 = Utils.Embed()
                        .WithTitle("Learn?")
                        .WithDescription("I'm lazy.");
                    await c.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()).AddEmbed(embed2));
                    break;
                case "understand":
                    embed2 = Utils.Embed()
                        .WithTitle("Thank you!")
                        .WithDescription(":D");
                    await c.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()).AddEmbed(embed2));
                    break;
            }

        }
    }
}

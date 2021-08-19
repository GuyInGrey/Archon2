using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
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

            //var intr = c.Client.GetInteractivity();
            //var r = await intr.WaitForSelectAsync(null, "dropdown");

            c.Client.ComponentInteractionCreated += async (s, e) =>
            {
                Console.WriteLine("A");
            };

            await c.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.CornflowerBlue)
                    .WithTitle("Pong!")
                    .WithDescription("I have no clue how to get client latency yet.")
                    .Build()).AddComponents(dropdown));
        }
    }
}

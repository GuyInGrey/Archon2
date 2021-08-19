using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace Archon2.Modules
{
    public class RandomMessage : ApplicationCommandModule
    {
        List<(ulong, DateTime, string)> Memes;
        Random rand = new();

        public RandomMessage()
        {
            Memes = File.ReadAllText("memes.txt").Split("\n").Select(line =>
            {
                var i = line.IndexOf(",");
                var id = ulong.Parse(line.Substring(0, i));
                var i2 = line.IndexOf(",", i + 1);
                var timestamp = long.Parse(line.Substring(i + 1, i2 - i - 1));

                var url = line.Substring(i2 + 1, line.Length - i2 - 1);
                return (id, timestamp.ToUnixTimestamp(), url);
            }).ToList();
        }

        [SlashCommand("meme", "Gets a random attachment from LDSG's #gifs-and-memes.")]
        public async Task Meme(InteractionContext c)
        {
            var sel = Memes[rand.Next(0, Memes.Count)];

            var e = Utils.Embed()
                .WithImageUrl(sel.Item3)
                .WithDescription("From <@" + sel.Item1 + ">")
                .WithTimestamp(sel.Item2);

            var builder = new DiscordInteractionResponseBuilder().AddEmbed(e);
            await c.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
        }
    }
}

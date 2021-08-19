using System;

using DSharpPlus.Entities;

namespace Archon2
{
    public static class Utils
    {
        public static DiscordEmbedBuilder Embed()
        {
            return new DiscordEmbedBuilder()
                .WithTimestamp(DateTime.Now)
                .WithColor(DiscordColor.CornflowerBlue);
        }
    }
}

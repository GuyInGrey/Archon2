using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace Archon2.Modules
{
    public class Puzzle : ApplicationCommandModule
    {
        static DiscordChannel ImageChannel;
        static Random rand = new();

        [SlashCommand("puzzle", "Starts a sliding puzzle")]
        public async Task PuzzleCmd(InteractionContext c, 
            [Option("size", "The number of pieces on the x and y axis. An input of 4 will result in 15 pieces with one space.")]long size,
            [Choice("Discord", "discord")]
            [Choice("White", "white")]
            [Choice("Black", "black")]
            [Choice("Transparent", "transparent")]
            [Option("color", "The background color for the image.")]string color = "discord")
        {
            var builder = new DiscordInteractionResponseBuilder().AddEmbed(Utils.Embed()
                .WithTitle("Please upload an image.")
                .WithDescription("Please make sure the image is square, and the dimensions are divisible by `" + size + "`.\nThe image must also be below 4 MB and <= 1024x1024.")
                .WithAuthor(c.Member.DisplayName, iconUrl: c.Member.GetAvatarUrl(ImageFormat.Png, 256))
                .WithFooter("This will time out in one minute.")
                .Build());
            await c.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);

            var intr = c.Client.GetInteractivity();
            var r = await intr.WaitForMessageAsync(m =>
            {
                return m.Attachments.Count > 0 &&
                    (m.Attachments[0].Url.EndsWith(".png") || m.Attachments[0].Url.EndsWith(".jpg")) &&
                    m.Author.Id == c.Member.Id;
            }, new TimeSpan(0, 1, 0));

            if (r.TimedOut)
            {
                await c.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(Utils.Embed()
                    .WithDescription("Timed out.")
                    .WithAuthor(c.Member.DisplayName, iconUrl: c.Member.GetAvatarUrl(ImageFormat.Png, 256))
                    .Build()));
                return;
            }
            else if (r.Result.Attachments[0].FileSize >= 4000000)
            {
                builder.AddEmbed(Utils.Embed()
                    .WithDescription("Failed. Please use a file that's smaller than 4 MB.")
                    .WithAuthor(c.Member.DisplayName, iconUrl: c.Member.GetAvatarUrl(ImageFormat.Png, 256))
                    .Build());
                await c.EditResponseAsync(new DiscordWebhookBuilder().AddEmbeds(builder.Embeds));
                return;
            }

            ImageChannel = await c.Client.GetChannelAsync(854456461676642304);

            var url = r.Result.Attachments[0].Url;

            await c.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(Utils.Embed()
                .WithDescription("Loading...")
                .WithAuthor(c.Member.DisplayName, iconUrl: c.Member.GetAvatarUrl(ImageFormat.Png, 256))));

            var image = await UrlToImage(url);
            await r.Result.DeleteAsync();

            if (image.Width != image.Height || image.Width % size != 0 || image.Height % size != 0 || image.Width > 1024)
            {
                builder.AddEmbed(Utils.Embed()
                    .WithTitle("Invalid dimensions.")
                    .WithDescription("Please make sure the image is square, and the dimensions are divisible by `" + size + "`.\nThe image must also be <= 1024x1024.")
                    .WithColor(DiscordColor.Red)
                    .WithAuthor(c.Member.DisplayName, iconUrl: c.Member.GetAvatarUrl(ImageFormat.Png, 256))
                    .Build());
                await c.EditResponseAsync(new DiscordWebhookBuilder().AddEmbeds(builder.Embeds));
                return;
            }

            var parts = PuzzleHelpers.Generate(image, (int)size, 
                color == "discord" ? new Color(new Argb32(54, 57, 63)) :
                color == "black" ? new Color(new Argb32(0, 0, 0)) :
                color == "white" ? new Color(new Argb32(255, 255, 255)) :
                color == "transparent" ? new Color(new Argb32(0, 0, 0, 0)) :
                Color.Magenta);
            image = parts.Item1;
            var u = await ImageToUrl(image);
            await Run(c, u, (int)size, parts.x, parts.y);
        }

        public async Task Run(InteractionContext c, string url, int size, int x, int y)
        {
            var buttons = new List<DiscordActionRowComponent>()
            {
                new DiscordActionRowComponent(new List<DiscordComponent>()
                {
                    new DiscordButtonComponent(ButtonStyle.Primary, "left", "", emoji: new DiscordComponentEmoji("◀️")),
                    new DiscordButtonComponent(ButtonStyle.Primary, "right", "", emoji: new DiscordComponentEmoji("▶️")),
                    new DiscordButtonComponent(ButtonStyle.Primary, "up", "", emoji: new DiscordComponentEmoji("🔼")),
                    new DiscordButtonComponent(ButtonStyle.Primary, "down", "", emoji: new DiscordComponentEmoji("🔽")),
                }),
                new DiscordActionRowComponent(new List<DiscordComponent>()
                {
                    new DiscordButtonComponent(ButtonStyle.Secondary, "shuffle", "Shuffle", emoji: new DiscordComponentEmoji("♻️")),
                    new DiscordButtonComponent(ButtonStyle.Danger, "end", "End"),
                }),
            };

            await c.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(Utils.Embed()
                .WithTitle("Have fun!")
                .WithImageUrl(url)
                .WithAuthor(c.Member.DisplayName, iconUrl: c.Member.GetAvatarUrl(ImageFormat.Png, 256))
                .WithFooter("This puzzle will end after 5 minutes of inactivity.")
                .WithColor(DiscordColor.Green))
                .AddComponents(buttons));

            var r = await c.Client.GetInteractivity().WaitForButtonAsync(await c.GetOriginalResponseAsync(), new TimeSpan(0, 5, 0));
            await r.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (r.TimedOut || r.Result.Id == "end")
            {
                await c.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(Utils.Embed()
                    .WithColor(DiscordColor.Red)
                    .WithTitle("Puzzle ended.")
                    .WithAuthor(c.Member.DisplayName, iconUrl: c.Member.GetAvatarUrl(ImageFormat.Png, 256))));
                return;
            }

            var image = await UrlToImage(url);

            switch (r.Result.Id)
            {
                case "shuffle":
                    await c.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(Utils.Embed()
                        .WithTitle("Shuffling, please wait...")
                        .WithAuthor(c.Member.DisplayName, iconUrl: c.Member.GetAvatarUrl(ImageFormat.Png, 256))));
                    for (var i = 0; i < 100; i++)
                    {
                        (image, x, y) = PuzzleHelpers.Swap(image, x, y, size, rand.Next(0, 4));
                    }
                    break;
                case "up":
                    (image, x, y) = PuzzleHelpers.Swap(image, x, y, size, 0);
                    break;
                case "down":
                    (image, x, y) = PuzzleHelpers.Swap(image, x, y, size, 2);
                    break;
                case "left":
                    (image, x, y) = PuzzleHelpers.Swap(image, x, y, size, 3);
                    break;
                case "right":
                    (image, x, y) = PuzzleHelpers.Swap(image, x, y, size, 1);
                    break;
            }

            await Run(c, await ImageToUrl(image), size, x, y);
        }
        
        private static async Task<string> ImageToUrl(Image i)
        {
            i.SaveAsPng("temp.png");
            using var f = File.OpenRead("temp.png");

            var m2 = await ImageChannel.SendMessageAsync(new DiscordMessageBuilder()
                .WithFile("temp.png", f));
            var url = m2.Attachments[0].Url;
            await m2.DeleteAsync();
            return url;
        }

        public static async Task<Image> UrlToImage(string url)
        {
            var client = new HttpClient();
            using var bytes = await client.GetStreamAsync(url);
            return Image.Load(bytes, new PngDecoder());
        }
    }
}

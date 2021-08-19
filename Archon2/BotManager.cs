using System.Threading.Tasks;

using Archon2.Modules;

using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;

using Newtonsoft.Json.Linq;

namespace Archon2
{
    public class BotManager
    {

        private readonly DiscordClient Client;
        private readonly SlashCommandsExtension Slash;
        private readonly InteractivityExtension Interactivity;

        public BotManager()
        {
            Client = new DiscordClient(new DiscordConfiguration()
            {
                Token = Program.Config["token"].Value<string>(),
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All,
            });

            Slash = Client.UseSlashCommands();
            Interactivity = Client.UseInteractivity();
            Slash.RegisterCommands<TestSlash>(769057370646511628);
        }

        public async Task Run()
        {
            await Client.ConnectAsync();

            await Task.Delay(-1);
        }
    }
}

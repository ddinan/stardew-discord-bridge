using Discord;
using Discord.WebSocket;
using DiscordBridge;
using Newtonsoft.Json;
using StardewModdingAPI;
using System.IO;
using System.Threading.Tasks;

public class DiscordBot
{
    public static Task Start() => new DiscordBot().MainAsync();

    private static DiscordSocketClient client;

    public class DiscordConfig
    {
        public string token;
        public ulong channelID;
    }

    public async Task MainAsync()
    {
        if (!File.Exists(Path.Combine(ModEntry.path, "config.json")))
        {
            ModEntry.ModMonitor.Log("The file 'config.json' was not found.", LogLevel.Error);
            return;
        }

        var token = JsonConvert.DeserializeObject<DiscordConfig>(File.ReadAllText(Path.Combine(ModEntry.path, "config.json"))).token;

        //ModEntry.SendChatMessage(token + " token");

        var config = new DiscordSocketConfig()
        {
            // We need to allow Discord intents to read member messages
            GatewayIntents = GatewayIntents.All
        };

        client = new DiscordSocketClient(config);

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

        // Add handlers
        client.Log += Log;
        client.MessageReceived += MessageReceived;

        client.Ready += () =>
        {
            ModEntry.ModMonitor.Log("Discord bot is now running.", LogLevel.Info);
            return Task.CompletedTask;
        };

        await Task.Delay(-1);
    }

    private static async Task MessageReceived(SocketMessage message)
    {
        var channel = message.Channel as SocketGuildChannel;
        if (channel == null) return;

        if (message.Author.IsBot) return; // Don't relay bot messages

        if (!File.Exists(Path.Combine(ModEntry.path, "config.json")))
        {
            ModEntry.ModMonitor.Log("The file 'config.json' was not found.", LogLevel.Error);
            return;
        }

        var channelID = JsonConvert.DeserializeObject<DiscordConfig>(File.ReadAllText(Path.Combine(ModEntry.path, "config.json"))).channelID;

        if (channel.Id != channelID) return; // Only send in specific channel

        if (message.Content.ToLower().StartsWith(".who"))
        {
            // TODO: Show players online
            return;
        }

        ModEntry.SendChatMessage(message.Author.ToString() + ": " + message.Content);
    }

    public static async Task SendChannelMessageAsync(string message)
    {
        if (!File.Exists(Path.Combine(ModEntry.path, "config.json")))
        {
            ModEntry.ModMonitor.Log("The file 'config.json' was not found.", LogLevel.Error);
            return;
        }

        ulong channelID = JsonConvert.DeserializeObject<DiscordConfig>(File.ReadAllText(Path.Combine(ModEntry.path, "config.json"))).channelID;

        var channel = await client.GetChannelAsync(channelID) as IMessageChannel;
        if (channel == null) return;

        await channel!.SendMessageAsync(message);
    }

    private static Task Log(LogMessage msg)
    {
        return Task.CompletedTask;
    }
}
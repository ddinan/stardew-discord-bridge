using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace DiscordBridge
{
    /// <summary>The mod entry point.</summary>
    class ModEntry : Mod
    {
        /// <summary>Provides method to log to the console.</summary>
        public static IMonitor ModMonitor;

        /// <summary>
        /// The path of the mod folder.
        /// </summary>
        public static string path = "";

        /// <summary>The mod entry point</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory as well as the modding API.</param>
        public override void Entry(IModHelper helper)
        {
            // Update the path
            path = Helper.DirectoryPath;

            // Create a new Harmony instance for patching the source code
            var harmony = new Harmony(ModManifest.UniqueID);

            // Override the default receiveChatMessage() behaviour
            harmony.Patch(
               original: AccessTools.Method(typeof(ChatBox), nameof(ChatBox.receiveChatMessage)),
               postfix: new HarmonyMethod(AccessTools.Method(typeof(ModEntry), nameof(ModEntry.ReceiveChatMessageHandler)))
            );

            ModMonitor = Monitor;

            DiscordBot.Start(); // Start the Discord bot
        }

        public static void SendChatMessage(string message)
        {
            Game1.chatBox.addMessage("Discord: " + message, new Microsoft.Xna.Framework.Color(128, 0, 128)); // RGB 128, 0, 128 = purple
        }

        /// <summary>Custom handler to retrieve messages sent to chat.</summary>
        /// <param name="sourceFarmer">The player that sent the message.</param>
        /// <param name="message">The message content.</param>
        private static async void ReceiveChatMessageHandler(long sourceFarmer, int chatKind, LocalizedContentManager.LanguageCode language, string message)
        {
            string playerName = "";

            // Find the player's name from the ID
            if (sourceFarmer == Game1.player.UniqueMultiplayerID)
            {
                playerName = ChatBox.formattedUserName(Game1.player);
            }

            else if (Game1.otherFarmers.ContainsKey(sourceFarmer))
            {
                playerName = ChatBox.formattedUserName(Game1.otherFarmers[sourceFarmer]);
            }

            // Only show messages sent by players
            if (playerName != "")
            {
                await DiscordBot.SendChannelMessageAsync("**" + playerName + "**: " + message); // Send the message on Discord
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DOB_AutoRole.Core
{
    internal class BotCore
    {
        internal DiscordSocketClient Client { get; }
        private CommandService Commands { get; }
        private DependencyMap Map { get; }

        private static BotCore BotInstance { get; set; }
        private static object Locker { get; } = new object();

        private BotCore()
        {
            Client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Debug
            });

            Commands = new CommandService();
            Map = new DependencyMap();
        }

        internal static BotCore Instance
        {
            get
            {
                if (BotInstance != null)
                    return BotInstance;

                lock (Locker)
                {
                    if (BotInstance != null)
                        return BotInstance;

                    BotInstance = new BotCore();
                }
                return BotInstance;
            }
        }

        private async Task InstallCommandsAsync()
        {
            Client.MessageReceived += Client_MessageReceived;

            await Commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task Client_MessageReceived(SocketMessage msg)
        {
            var message = msg as SocketUserMessage;
            if (message == null)
                return;

            var argPos = 0;

            Console.WriteLine($"DEBUG CHANNEL TYPE: {message.Channel.GetType()}");

            if (!message.HasCharPrefix('!', ref argPos))
                return;

            var context = new CommandContext(Client, message);

            await message.Channel.TriggerTypingAsync();

            var result = await Commands.ExecuteAsync(context, argPos, Map);

            if (!result.IsSuccess && !result.ErrorReason.ToLower().Contains("unknown command"))  // avoid logging unknown command for now...
                await message.Channel.SendMessageAsync(result.ErrorReason);
        }

        internal async void LaunchAsync(string token)
        {
            Client.Log += (message) =>
             {
                 Console.WriteLine($"{DateTime.Now}: ({message.Severity}) {message.Message}");
                 return Task.CompletedTask;
             };

            await InstallCommandsAsync();

            await Client.LoginAsync(TokenType.Bot, token);

            await Client.ConnectAsync();
        }

        internal async void DisconnectAsync()
        {
            await Client.DisconnectAsync();
            await Client.LogoutAsync();
        }
    }
}

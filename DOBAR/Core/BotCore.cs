using System;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LiteDB;
using static DOBAR.Helper.Helper;
using static DOBAR.Helper.Logger;

namespace DOBAR.Core
{
    internal class BotCore
    {
        internal DiscordSocketClient Client { get; }
        internal LiteDatabase Database { get; }
        internal CommandService Commands { get; }
        internal Configuration Configuration { get; private set; }
        private DependencyMap Map { get; }

        private static BotCore BotInstance { get; set; }
        private static object Locker { get; } = new object();

        private BotCore()
        {
            Info("Creating bot core...");
            Database = new LiteDatabase(WorkingDirectory + "config.db");
            Info("Database loaded.");

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
                Debug("Accessing bot instance.");
                if (BotInstance != null)
                    return BotInstance;

                Debug("Locking bot instance.");
                lock (Locker)
                {
                    if (BotInstance != null)
                        return BotInstance;

                    Debug("Creating new bot instance.");
                    BotInstance = new BotCore();
                }
                return BotInstance;
            }
        }

        private async Task InstallCommandsAsync()
        {
            Client.MessageReceived += Client_MessageReceivedAsync;

            await Commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task Client_MessageReceivedAsync(SocketMessage msg)
        {
            var message = msg as SocketUserMessage;
            if (message == null)
                return;

            var argPos = 0;

            Console.WriteLine($"DEBUG CHANNEL TYPE: {message.Channel.GetType()}");

            if (!message.HasCharPrefix('!', ref argPos))
                return;

            var context = new CommandContext(Client, message);

            var result = await Commands.ExecuteAsync(context, argPos, Map);

            if (!result.IsSuccess && !result.ErrorReason.ToLower().Contains("unknown command"))  // avoid logging unknown command for now...
                await message.Channel.SendMessageAsync(result.ErrorReason);
        }

        internal async void LaunchAsync(Configuration config)
        {
            Info("Launching...");
            Configuration = config;

            Client.Log += (message) =>
            {
                Log(message.Severity, message.Message);
                return Task.CompletedTask;
            };

            Client.UserJoined += async (user) =>
            {
                Info($"User {user.Nickname} joined.");
                var memberRole = from r in user.Guild.Roles where r.Name.ToLower() == "member" select r;
                await user.AddRolesAsync(memberRole);

                var setting = new UserSetting()
                {
                    Id = user.Id
                };

                await setting.NotifyFFAUser();

                var db = Database.GetCollection<UserSetting>("users");
                db.Insert(setting);
            };

            Client.UserLeft += (user) =>
            {
                Info($"User {user.Nickname} left.");
                var db = Database.GetCollection<UserSetting>("users");
                db.Delete(x => x.Id == user.Id);

                return Task.CompletedTask;
            };

            Client.Ready += () =>
            {
                Task.Run(() =>
                {
                    UserLoop();
                });

                return Task.CompletedTask;
            };

            await InstallCommandsAsync();

            await Client.LoginAsync(TokenType.Bot, Configuration.Token);

            await Client.StartAsync();
        }

        private async void UserLoop()
        {
            while (true)
            {
                Info("Restarting users loop...");
                var db = Database.GetCollection<UserSetting>("users");
                var users = db.FindAll();

                Info($"We found {users.Count()} users.");

                var guild = (from g in Instance.Client.Guilds where g.Name == "DOB - Dah Old Bot" select g).FirstOrDefault();

                //still not everything loaded.
                if (guild == null)
                    continue;

                //await guild.DownloadUsersAsync();

                foreach (var user in guild.Users)
                {
                    if (!db.Exists(x => x.Id == user.Id))
                    {
                        var setting = new UserSetting()
                        {
                            Id = user.Id
                        };

                        Warn($"Not existing user found: {user.Nickname}, joined: {user.JoinedAt}");
                        db.Insert(setting);
                    }
                }

#if (!DEBUG)
                for (var i = 0; i < users.Count(); i++)
                {
                    var user = users.ElementAt(i);

                    await user.UpdateUserRole();
                    await user.CheckInformUser();

                    db.Update(user);

                    await Task.Delay(5 * 1000); //avoid v5 server flooding
                }
#endif
            }
        }

        internal async Task DisconnectAsync()
        {
            await Client.StopAsync();
            await Client.LogoutAsync();
        }
    }
}
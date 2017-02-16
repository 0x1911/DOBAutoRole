using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using static DOB_AutoRole.Helper.Logger;

namespace DOB_AutoRole.Core
{
    internal class UserSettingsHelper
    {
        internal static Color[] LicenseColors = new Color[] { new Color(163, 163, 163), new Color(221, 247, 0), new Color(94, 94, 94), new Color(255, 173, 0) };
        internal static string[] Licenses => new string[] { "Platin", "Gold", "Silver", "Beta Tester" };
        internal static string[] Roles => new string[] { "platin", "gold", "silver", "tester" };
        internal static string[] ExcludeRoles => new string[] { "staff", "mod", "vip", "bots" };
    }

    public class UserSetting
    {
        public ulong Id { get; set; }
        public string Token { get; set; }
        public int LastChecked { get; set; }
        public string License { get; set; }
        public string Username { get; set; }
        public DateTime LastInformed { get; set; } = DateTime.MinValue;

        public async Task UpdateUserRole(string token, bool updateToken = true)
        {
            Info($"Start updating user {Id}.");
            Debug($"Token: {token} (current: {Token}");

            if (updateToken)
                Token = token;

            try
            {
                var hc = new HttpClient();
                var answer = await hc.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"http://[2a01:4f8:172:201b::10]/discord.php?apiKey={BotCore.Instance.Configuration.V5ApiKey}&authKey={token}"));
                var html = await answer.Content.ReadAsStringAsync();

                dynamic userInfo = JObject.Parse(html);

                var oldLicense = License;

                Debug(html);

                if ((bool)userInfo.result)
                {
                    License = userInfo.type;
                    Username = userInfo.public_name;
                    LastChecked = userInfo.timestamp;
                }
                else
                {
                    LastChecked = -1;
                    License = "";
                    Username = "";
                }

                Info($"Old license: {oldLicense} new license: {License}");

                var guild = (from g in BotCore.Instance.Client.Guilds where g.Name == "DOB Darkorbit Bot" select g).FirstOrDefault();
                var user = guild.GetUser(Id);
                var removeRoles = from r in guild.Roles where UserSettingsHelper.Roles.Contains(r.Name) select r;


                var addRoleName = UserSettingsHelper.Licenses.Contains(License) ? UserSettingsHelper.Roles[Array.IndexOf(UserSettingsHelper.Licenses, License)] : null;
                var addRoles = from r in guild.Roles where r.Name == addRoleName select r;

                await user.ChangeRolesAsync(addRoles, removeRoles);
            }
            catch(Exception ex)
            {
                Error(ex);
            }

            Info("Finished updating user.");
        }

        public async Task UpdateUserRole()
        {
            await UpdateUserRole(Token);
        }

        public async Task CheckInformUser()
        {
            Info($"Starting inform check for user {Id}");
            Debug($"Last informed: {LastInformed}");
            Debug($"License: {License}");

            // stop if license exists.
            if (UserSettingsHelper.Licenses.Contains(License))
                return;

            // only every 3 days
            if ((DateTime.Now - LastInformed).TotalDays < 3)
                return;

            var guild = (from g in BotCore.Instance.Client.Guilds where g.Name == "DOB Darkorbit Bot" select g).FirstOrDefault();
            var user = guild.GetUser(Id);
            var excludeRoles = from r in guild.Roles where UserSettingsHelper.ExcludeRoles.Contains(r.Name) select r;

            foreach (var er in excludeRoles)
                if (user.RoleIds.Contains(er.Id))
                    return; // Do not disturb 

            Info($"Remembering user {Id} to set license.");
            var channel = await user.CreateDMChannelAsync();
            await channel.SendMessageAsync($"Hi. Your account is currently not assigned to a bot license. Please write `!role <your auth key>` to assign your license in this chat. **Please do not post in public server!**");

            LastInformed = DateTime.Now;
        }
    }
}

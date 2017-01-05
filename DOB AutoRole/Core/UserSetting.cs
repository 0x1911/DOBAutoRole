using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json.Linq;

namespace DOB_AutoRole.Core
{
    public class UserSetting
    {
        internal static string[] Licenses => new string[] { "Platin", "Gold", "Silver", "Beta Tester" };
        internal static string[] Roles => new string[] { "platin", "gold", "silver", "tester" };
        internal static string[] ExcludeRoles => new string[] { "staff", "mod", "vip", "bots" };
        public ulong Id { get; set; }
        public string Token { get; set; }
        public int LastChecked { get; set; }
        public string License { get; set; }
        public string Username { get; set; }
        public DateTime LastInformed { get; set; } = DateTime.MinValue;

        public async Task UpdateUserRole(string token, bool updateToken = true)
        {
            if (updateToken)
                Token = token;

            var hc = new HttpClient();
            var answer = await hc.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"http://[2a01:4f8:172:201b::10]/discord.php?apiKey={BotCore.Instance.Configuration.V5ApiKey}&authKey={token}"));
            var html = await answer.Content.ReadAsStringAsync();

            dynamic userInfo = JObject.Parse(html);

            var oldLicense = License;

            if (userInfo.result)
            {
                License = userInfo.type;
                Username = userInfo.real_name;
                LastChecked = userInfo.timestamp;
            }
            else
            {
                LastChecked = -1;
                License = "";
                Username = "";
            }

            if (oldLicense != License)
            {
                await RemoveRoles();
                await AddRole(Roles[Array.IndexOf(Licenses, License)]);
            }
        }

        public async Task AddRole(string role)
        {
            var guild = (from g in BotCore.Instance.Client.Guilds where g.Name == "DOB Darkorbit Bot" select g).FirstOrDefault();
            var user = guild.GetUser(Id);
            var roles = from r in guild.Roles where r.Name == role select r;

            await user.AddRolesAsync(roles);
        }

        public async Task RemoveRoles()
        {
            var guild = (from g in BotCore.Instance.Client.Guilds where g.Name == "DOB Darkorbit Bot" select g).FirstOrDefault();
            var user = guild.GetUser(Id);
            var roles = from r in guild.Roles where Roles.Contains(r.Name) select r;

            await user.RemoveRolesAsync(roles);
        }

        public async Task UpdateUserRole()
        {
            await UpdateUserRole(Token);
        }

        public async Task CheckInformUser()
        {
            // wenn eine Lizenz existiert dann aufhören
            if (Licenses.Contains(License))
                return;

            // maximal alle 3 Tage
            if ((DateTime.Now - LastInformed).TotalDays < 3)
                return;

            var guild = (from g in BotCore.Instance.Client.Guilds where g.Name == "DOB Darkorbit Bot" select g).FirstOrDefault();
            var user = guild.GetUser(Id);
            var excludeRoles = from r in guild.Roles where ExcludeRoles.Contains(r.Name) select r;

            foreach (var er in excludeRoles)
                if (user.RoleIds.Contains(er.Id))
                    return; // Die wollen wir nicht weiter belästigen


            var channel = await user.CreateDMChannelAsync();
            await channel.SendMessageAsync($"Hi. Your account is currently not assigned to a bot license. Please write `!role <your auth key>` to assign your license in this chat. **Please do not post in public server!**");

            LastInformed = DateTime.Now;
        }
    }
}

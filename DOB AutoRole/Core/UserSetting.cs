using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json.Linq;
using static DOB_AutoRole.Helper.Logger;

namespace DOB_AutoRole.Core
{
    internal class UserSettingsHelper
    {
        internal static Color[] LicenseColors = new Color[] { new Color(163, 163, 163), new Color(221, 247, 0), new Color(94, 94, 94), new Color(255, 173, 0) };
        internal static string[] Licenses => new string[] { "Platin", "Gold", "Silver", "Beta Tester" };
        internal static string[] Roles => new string[] { "platin", "gold", "silver", "tester" };
        internal static string[] ExcludeRoles => new string[] { "staff", "developer", "sales manager", "community manager", "super moderator", "moderator", "vip", "bots" };
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
                //get the user info object from the web APi
                var tmpLicenseApi = new Helper.v5API.Licenses();
                var rawUserInfo = await tmpLicenseApi.GetLicenseInfo(BotCore.Instance.Configuration.V5ApiKey, token);
                //and parse as json object
                dynamic userInfo = JObject.Parse(rawUserInfo);
                //hold obsolete data for tmp info log output
                var tmpOldLicense = License;

                Debug(rawUserInfo);
                //did we get valid data back from the http api?
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

                Info($"Changed old license {tmpOldLicense} to {License}.");

                var guild = (from g in BotCore.Instance.Client.Guilds where g.Name == "DOB Darkorbit Bot" select g).FirstOrDefault();
                var user = guild.GetUser(Id);
                var removeRoles = from r in guild.Roles where UserSettingsHelper.Roles.Contains(r.Name) select r;


                var addRoleName = UserSettingsHelper.Licenses.Contains(License) ? UserSettingsHelper.Roles[Array.IndexOf(UserSettingsHelper.Licenses, License)] : null;
                var addRoles = from r in guild.Roles where r.Name == addRoleName select r;

                await user.RemoveRolesAsync(removeRoles);
                await user.AddRolesAsync(addRoles);
            }
            catch (Exception ex)
            {
                Error(ex.Message.ToString());
            }

            Info("Finished updating user.");
        }

        public async Task UpdateUserRole()
        {
            await UpdateUserRole(Token);
        }

        /// <summary>
        /// Encourage users without a bound discord account to do so via a personal message\r
        /// </summary>
        public async Task NotifyFFAUser()
        {
            Info($"Starting info check for user {Id}");
            Debug($"Last informed: {LastInformed}");
            Debug($"License: {License}");

            // stop if license exists
            if (UserSettingsHelper.Licenses.Contains(License))
                return;

            // only every 3 days
            if ((DateTime.Now - LastInformed).TotalDays < 3)
                return;

            var guild = (from g in BotCore.Instance.Client.Guilds where g.Name == "DOB Darkorbit Bot" select g).FirstOrDefault();
            var user = guild.GetUser(Id);
            var excludeRoles = from r in guild.Roles where UserSettingsHelper.ExcludeRoles.Contains(r.Name) select r;

            if (excludeRoles.Any(er => user.Roles.Select(role => role.Id).Contains(er.Id)))
            {
                return; // Do not disturb 
            }

            Info($"Remembering user {Id} to bind a license to his discord account.");
            var channel = await user.CreateDMChannelAsync();
            await channel.SendMessageAsync($"Hi there. Your account is currently not bound to a DOB user/auth key. To change that reply in here with `!role <your auth key>` to assign your license in this chat. **Note: Do not post your auth key in a public channel!**");

            LastInformed = DateTime.Now;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord.API;
using Discord;
using Discord.Commands;
using DOB_AutoRole.Core;

namespace DOB_AutoRole.Modules
{
    [Group("role")]
    public class RoleModule : ModuleBase
    {
        [Command, Alias("link"), Summary("links your discord account to a forum account")]
        public async Task Link([Summary("your auth key")] string authKey)
        {
            var db = BotCore.Instance.Database.GetCollection<UserSetting>("users");
            var user = db.FindOne(x => x.Id == Context.User.Id);

            user.Token = authKey;
            await user.UpdateUserRole();

            if (UserSetting.Licenses.Contains(user.License))
                await ReplyAsync($"Your {user.License} license has been saved.");
            else
                throw new Exception("No license for your auth key was found.");
        }
    }
}

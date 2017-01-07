﻿using System;
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
        public async Task LinkAsync([Summary("your auth key")] string authKey)
        {
            if (Context.Guild != null)
                await Context.Message.DeleteAsync();

            var db = BotCore.Instance.Database.GetCollection<UserSetting>("users");
            var user = db.Find(x => x.Id == Context.User.Id).FirstOrDefault();

            user.Token = authKey;
            await user.UpdateUserRole();

            if (UserSettingsHelper.Licenses.Contains(user.License))
                await ReplyAsync($"Your {user.License} license has been saved.");
            else
                throw new Exception("No license for your auth key was found.");
        }
    }
}

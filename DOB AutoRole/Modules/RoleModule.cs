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
using static DOB_AutoRole.Helper.Logger;

namespace DOB_AutoRole.Modules
{
    [Group("role")]
    public class RoleModule : ModuleBase
    {
        [Command, Alias("link"), Summary("links your discord account to a forum account")]
        public async Task Link([Summary("your auth key")] string authKey)
        {
            Debug($"Received auth key {authKey}");
            if (Context.Guild != null)
                await Context.Message.DeleteAsync();

            authKey = authKey.Replace("<", "").Replace(">", "");

            var db = BotCore.Instance.Database.GetCollection<UserSetting>("users");
            var user = db.Find(x => x.Id == Context.User.Id).FirstOrDefault();

            user.Token = authKey;
            await user.UpdateUserRole();

            db.Update(user);

            if (UserSettingsHelper.Licenses.Contains(user.License))
                await ReplyAsync($"Your {user.License} license has been saved.");
            else
                throw new Exception("No license for your auth key was found.");
        }

        [Command("info"), Alias("i"), Summary("displays information about a users license")]
        public async Task Info([Summary("the user mentions whose information should be displayd")] string userMention = null)
        {
            var db = BotCore.Instance.Database.GetCollection<UserSetting>("users");

            var roles = Context.Guild.Roles;


            List<UserSetting> users = new List<UserSetting>();

            foreach (var id in Context.Message.MentionedUserIds)
                users.Add(db.Find(x => x.Id == id).FirstOrDefault());

            if (users.Count == 0)
                users.Add(db.Find(x => x.Id == Context.User.Id).FirstOrDefault());

            foreach (var u in users)
            {
                await Context.Channel.TriggerTypingAsync();

                var du = await Context.Guild.GetUserAsync(u.Id);
                var c = new Color(102, 153, 204);

                foreach (var r in roles)
                {
                    if (du.RoleIds.Contains(r.Id))
                    {
                        c = r.Color;
                        break;
                    }
                }

                var eb = new EmbedBuilder()
                {
                    Color = c
                };
                
                eb.AddField((efb) =>
                {
                    efb.Name = du.Nickname;
                    efb.Value = $"license:\t{u.License}\nforum name:\t{u.Username}\njoined discord:\t{du.JoinedAt}";
                });

                await Context.Channel.SendMessageAsync("", false, eb);
            }
        }
    }
}

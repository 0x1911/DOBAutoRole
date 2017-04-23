using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DOB_AutoRole.Core;
using static DOB_AutoRole.Helper.Logger;

namespace DOB_AutoRole.Modules.ModModule
{
    [Group("mod")]
    public class ModModule : ModuleBase
    {
        private bool HasModAccess(IGuild guild, IGuildUser user)
        {
            var validRole = false;

            var db = BotCore.Instance.Database.GetCollection<ModConfiguration>("modconfiguration");
            var config = db.Find(x => x.Id == Context.Guild.Id).FirstOrDefault();
            if (config == null)
            {
                return false;
            }

            var validRoleId = config.ModRole;

            var roles = guild.Roles.OrderBy(r => r.Position);

            foreach (var role in roles)
            {
                if (role.Id == validRoleId)
                    validRole = true;

                if (validRole && user.RoleIds.Contains(role.Id))
                    return true;
            }

            return false;
        }

        [Command("check"), Summary("Checks if a user has mod access.")]
        public async Task Check(string mention = null)
        {
            List<IGuildUser> users = new List<IGuildUser>();

            foreach (var id in Context.Message.MentionedUserIds)
                users.Add(await Context.Guild.GetUserAsync(id));

            if (users.Count == 0)
                users.Add(await Context.Guild.GetUserAsync(Context.User.Id));

            foreach (var user in users)
            {
                await ReplyAsync($"{user.Username} access: {HasModAccess(Context.Guild, user)}");
            }
        }

        [Command("setup"), Summary("Setup the moderating role")]
        public async Task Setup([Summary("Minimum required role for accessing the mod commands")] string roleName = null)
        {
            Debug($"Mod setup issued by {Context.User.Username}");

            if (Context.Guild.OwnerId != Context.User.Id)
            {
                Debug("Mod setup was triggered by not an owner. Stopping...");
                await ReplyAsync("This command is only available for the guild owner.");
                return;
            }

            if (roleName == null)
            {
                var db = BotCore.Instance.Database.GetCollection<ModConfiguration>("modconfiguration");
                var config = db.Find(x => x.Id == Context.Guild.Id).FirstOrDefault();
                if (config == null)
                {
                    await ReplyAsync("No moderation role has been set.");
                }
                else
                {
                    await ReplyAsync($"Mod role: {Context.Guild.GetRole(config.ModRole)?.Name ?? "Invalid Id"}");
                }

                return;
            }

            foreach (var r in Context.Guild.Roles)
            {
                if (r.Name.ToUpper() == roleName.ToUpper())
                {
                    var db = BotCore.Instance.Database.GetCollection<ModConfiguration>("modconfiguration");
                    var config = db.Find(x => x.Id == Context.Guild.Id).FirstOrDefault() ?? new ModConfiguration()
                    {
                        Id = Context.Guild.Id
                    };
                    config.ModRole = r.Id;

                    if (!db.Update(config))
                        db.Insert(config);

                    await ReplyAsync($"Mod role has been updated to {r.Mention}");
                    return;
                }
            }

            await ReplyAsync("This role was not found.");
        }
    }
}

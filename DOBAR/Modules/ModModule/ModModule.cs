﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DOBAR.Core;
using static DOBAR.Helper.Logger;

namespace DOBAR.Modules.ModModule
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

        private async Task<IMessageChannel> GetModLog(IGuild guild)
        {
            var db = BotCore.Instance.Database.GetCollection<ModConfiguration>("modconfiguration");
            var config = db.Find(x => x.Id == Context.Guild.Id).FirstOrDefault();

            if (config == null || config.ModLog == 0)
                return null;

            return (await guild.GetTextChannelAsync(config.ModLog));
        }

        [Command("checkaccess"), Summary("Checks if a user can access the mod commands.")]
        public async Task CheckAccess(string mention = null)
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

        [Command("setlog"), Summary("Sets a mod log channel.")]
        public async Task SetLogChannel([Summary("Channel mention.")] string channel)
        {
            if (Context.Guild.OwnerId != Context.User.Id)
            {
                Debug("Mod setup was triggered, but not by an owner. Stopping...");
                await ReplyAsync("This command is only available for the guild owner.");
                return;
            }

            if (Context.Message.MentionedChannelIds.Count != 1)
            {
                await ReplyAsync("Provide exactly one channel by mentioning it!");
                return;
            }

            var db = BotCore.Instance.Database.GetCollection<ModConfiguration>("modconfiguration");
            var config = db.Find(x => x.Id == Context.Guild.Id).FirstOrDefault();
            if (config == null)
            {
                await ReplyAsync("No moderation log channel has been set.");
            }
            else
            {
                config.ModLog = Context.Message.MentionedChannelIds.First();
                db.Update(config);
                await ReplyAsync($"Mod log channel has been set to <#{config.ModLog}>");
                await (await Context.Guild.GetTextChannelAsync(config.ModLog)).SendMessageAsync(
                    "This channel is now the moderation log channel.");
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

        [Command("warn"), Summary("Warns a user.")]
        public async Task Warn([Summary("User mention")] string user, [Summary("How many points is this warning?")]int warningPoints = 1, [Summary("Days until this is removed")]int days = 30, [Summary("Reason for warning this user")]string reason = "")
        {
            await Context.Message.DeleteAsync();

            if (!HasModAccess(Context.Guild, await Context.Guild.GetUserAsync(Context.User.Id)))
            {
                await ReplyAsync("You are not allowed to use this!");
                return;
            }

            if (Context.Message.MentionedUserIds.Count != 1)
            {
                await ReplyAsync("You can only warn one user at a time!");
                return;
            }

            var realUser = await Context.Guild.GetUserAsync(Context.Message.MentionedUserIds.First());

            var warning = new UserStats()
            {
                UserId = realUser.Id,
                ExpirationDate = DateTime.Now.AddDays(days),
                WarningPoints = warningPoints
            };

            var db = BotCore.Instance.Database.GetCollection<UserStats>("mod");
            db.Insert(warning);


            //244 66 66
            var eb = new EmbedBuilder()
            {
                Color = warning.WarningPoints > 0 ? new Color(255, 100, 0) : new Color(70, 245, 65),
                ImageUrl = realUser.GetAvatarUrl(),
                ThumbnailUrl = Context.User.GetAvatarUrl(ImageFormat.Png),
                Title = $"{realUser.Username} (ID: {realUser.Id})"
            };

            eb.AddField((efb) =>
            {
                efb.Name = "Channel";
                efb.Value = $"{Context.Channel}";
                efb.IsInline = true;
            });

            eb.AddField((efb) =>
            {
                efb.Name = "Warned by";
                efb.Value = Context.User.Username;
                efb.IsInline = true;
            });

            eb.AddField((efb) =>
            {
                efb.Name = "Points";
                efb.Value = $"{warning.WarningPoints}";
                efb.IsInline = true;
            });

            eb.AddField((efb) =>
            {
                efb.Name = "Expiration date";
                efb.Value = $"{warning.ExpirationDate}";
                efb.IsInline = true;
            });


            if (!string.IsNullOrEmpty(reason))
                eb.AddField((efb) =>
                {
                    efb.Name = "Reason";
                    efb.Value = $"{reason}";
                });

            var log = await GetModLog(Context.Guild);
            if (log != null)
            {
                await log.SendMessageAsync("A user has been warned", false, eb);
            }
            await (await realUser.CreateDMChannelAsync()).SendMessageAsync("You received a warning", false, eb);
        }

        [Command("check"), Summary("Checks a users warning points.")]
        public async Task CheckUser(string mention = null)
        {
            await Context.Channel.TriggerTypingAsync();

            var users = new List<ulong>();

            if (Context.Message.MentionedUserIds.Count > 0 &&
                HasModAccess(Context.Guild, await Context.Guild.GetUserAsync(Context.User.Id)))
            {
                users.AddRange(Context.Message.MentionedUserIds);
            }
            else
            {
                users.Add(Context.User.Id);
            }

            var roles = Context.Guild.Roles.OrderBy((r) => -r.Position);
            var db = BotCore.Instance.Database.GetCollection<UserStats>("mod");

            foreach (var id in users)
            {
                var du = await Context.Guild.GetUserAsync(id);
                var c = new Color(102, 153, 204);

                foreach (var r in roles)
                {
                    if (du.RoleIds.Contains(r.Id))
                    {
                        c = r.Color;
                        break;
                    }
                }

                var warnings = db.Find(x => x.UserId == id).Sum(warning => warning.WarningPoints);
                var maxDate = db.Find(x => x.UserId == id).Max(warning => warning.ExpirationDate);

                var eb = new EmbedBuilder()
                {
                    Color = c,
                    ThumbnailUrl = du.GetAvatarUrl(ImageFormat.Png),
                    Title = $"{du.Username} (ID: {du.Id})"
                };

                eb.AddField((efb) =>
                {
                    efb.Name = "Active warning points";
                    efb.Value = $"{warnings} points";
                    efb.IsInline = true;
                });

                eb.AddField((efb) =>
                {
                    efb.Name = "Due date";
                    efb.Value = $"{maxDate}";
                    efb.IsInline = true;
                });

                await Context.Channel.SendMessageAsync("", false, eb);
            }
        }
    }
}

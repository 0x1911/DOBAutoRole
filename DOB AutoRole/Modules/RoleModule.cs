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
        public async Task Link([Summary("your profile link")] string link)
        {
            if (link == null || link == string.Empty)
                throw new ArgumentNullException("Please enter a link to your forum profile");

            if (!link.Contains("https://v5dev.xyz/forum/index.php?action=profile;u="))
                throw new ArgumentException("Please enter a valid profile url!");

            var hc = new HttpClient();
            var answer = await hc.SendAsync(new HttpRequestMessage(HttpMethod.Get, link));
            var html = await answer.Content.ReadAsStringAsync();

            var roles = new string[] { "silver", "gold", "platin", "tester" };

            foreach (var role in roles)
                if (html.ToLower().Contains(role.ToLower()))
                {
                    var memberRole = from r in Context.Guild.Roles where r.Name.ToLower() == role.ToLower() select r;
                    await (await Context.Guild.GetUserAsync(Context.User.Id)).AddRolesAsync(memberRole);
                    await ReplyAsync($"{Context.User.Username} has been added to **{memberRole.FirstOrDefault().Name}**.");
                    return;
                }

            throw new Exception("Unable to find a role matching your profile.");
        }
    }
}

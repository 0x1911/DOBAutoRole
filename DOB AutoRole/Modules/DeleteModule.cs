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
    public class DeleteModule : ModuleBase
    {
        [Command("delete"), Summary("Just deletes the message")]
        public async Task Delete()
        {
            await Context.Message.DeleteAsync();
        }
    }
}

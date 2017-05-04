using System.Threading.Tasks;
using Discord.Commands;

namespace DOB_AutoRole.Modules
{
    public  class DeleteModule:ModuleBase
    {
        [Command("delete"), Summary("Just deletes the message")]
        public async Task Delete()
        {
            await Context.Message.DeleteAsync();
        }
    }
}

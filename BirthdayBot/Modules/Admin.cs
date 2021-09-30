using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayBot.Modules
{
    public class Admin : ModuleBase
    {
        /*[Command("purge")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Purge(int amt)
        {
            var deletedTime = 3000;

            var msgs = await Context.Channel.GetMessagesAsync(amt + 1).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(msgs);

            var msg = await Context.Channel.SendMessageAsync($"Deleted {msgs.Count()} messages. This message will delete in {deletedTime / 1000} seconds");
            await Task.Delay(deletedTime);
            await msg.DeleteAsync();
        }*/
    }
}

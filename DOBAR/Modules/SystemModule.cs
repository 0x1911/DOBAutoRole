using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DOBAR.Core;

namespace DOBAR.Modules
{
    public class SystemModule : ModuleBase
    {
        [Command("about"), Summary("Displays an about message.")]
        public async Task AboutAsync()
        {
            await ReplyAsync("Copyright © 2017 by Serraniel.\nMade using brain.exe, C#, Discord.Net and LiteDB.");
        }

        [Command("help"), Alias("h"), Summary("Displays the help.")]
        public async Task HelpAsync([Summary("Command which help should be displayed.")] string cmdName = null)
        {
            var eb = new EmbedBuilder()
            {
                Color = new Color(255, 140, 20)
            };

            if (cmdName == null)
            {
                eb.Title = "AutoRole help:";

                foreach (var c in BotCore.Instance.Commands.Commands)
                {
                    eb.AddField((efb) =>
                    {
                        efb.Name = c.Module.Aliases.FirstOrDefault() + " " + c.Name;
                        efb.Value = c.Summary ?? "any description specified";
                    });
                }
            }
            else
            {
                eb.Title = $"Help for \"{cmdName}\"";
                var found = false;
                foreach (var c in BotCore.Instance.Commands.Commands)
                {
                    if (!c.Aliases.Contains(cmdName))
                        continue;

                    eb.AddField((efb) =>
                    {
                        efb.Name = c.Parameters.Aggregate(c.Name + "\n", (current, cmd) => $"{current} {(cmd.IsOptional ? $"[<{cmd.Name}>]" : $"<{cmd.Name}>")}");
                        efb.Value = c.Parameters.Aggregate(c.Summary + "\n\n" + c.Aliases.Aggregate("**Aliases**", (current, alias) => $"{current}{(c.Aliases.ElementAt(0) == alias ? string.Empty : ", ")} {alias}") + "\n\n**Parameters**", (current, cmd) => $"{current}\n\t{cmd.Name} {(cmd.IsOptional ? "(optional)" : "")}: {cmd.Summary}");
                    });

                    found = true;
                }

                if (!found)
                    throw new ArgumentException($"Command \"{cmdName}\" not found.");
            }

            await Context.Channel.SendMessageAsync("", false, eb);
        }

        [Command("system"), Alias("sys"), Summary("Returns some system information.")]
        public async Task SystemAsync()
        {
            var proc = System.Diagnostics.Process.GetCurrentProcess();

            Func<double, double> formatRamValue = d =>
            {
                while (d > 1024)
                    d /= 1024;

                return d;
            };

            Func<long, string> formatRamUnit = d =>
            {
                var units = new string[] { "B", "kB", "mB", "gB" };
                var unitCount = 0;
                while (d > 1024)
                {
                    d /= 1024;
                    unitCount++;
                }

                return units[unitCount];
            };

            var eb = new EmbedBuilder()
            {
                Color = new Color(4, 97, 247)
            };

            eb.AddField((efb) =>
            {
                efb.Name = "System";
                efb.IsInline = true;
                efb.Value = $"os version:\t{System.Runtime.InteropServices.RuntimeInformation.OSDescription}\narchitecture:\t{System.Runtime.InteropServices.RuntimeInformation.OSArchitecture}\nframework:\t{System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}";
            });

            eb.AddField((efb) =>
            {
                efb.Name = "Bot";
                efb.IsInline = true;
                efb.Value = $"architecture:\t{System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}\nup time:\t{(DateTime.Now - proc.StartTime).ToString(@"d'd 'hh\:mm\:ss")}\nmemory:\t{formatRamValue(proc.PagedMemorySize64).ToString("f2")} {formatRamUnit(proc.PagedMemorySize64)}\nprocessor time:\t{proc.TotalProcessorTime.ToString(@"d'd 'hh\:mm\:ss")}";
            });

            eb.AddField((efb) =>
            {
                efb.Name = "Discord";
                efb.IsInline = true;

                var channelCount = 0;
                var userCount = 0;

                foreach (var g in BotCore.Instance.Client.Guilds)
                {
                    channelCount += g.Channels.Count;
                    userCount += g.Users.Count;
                }

                efb.Value = $"state:\t{BotCore.Instance.Client.ConnectionState}\nguilds:\t{BotCore.Instance.Client.Guilds.Count}\nchannels:\t{channelCount}\nusers:\t{userCount}\nping:\t{BotCore.Instance.Client.Latency} ms";
            });

            await Context.Channel.SendMessageAsync("", false, eb);
        }

        [Command("ping"), Summary("Returns the ping from the server to discord's servers.")]
        public async Task PingAsync()
        {
            await ReplyAsync($":ping_pong: pong {BotCore.Instance.Client.Latency} ms");
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Neuromatrix.Core.Moderation
{
    public class Invite : ModuleBase<SocketCommandContext>
    {
        [Command("invite"), Summary("Создает инвайт для сервера.")]
        public async Task InviteModule(ulong GuildId)
        {
            if (!(Context.User.Id == 316272461291192322))
            {
                await Context.Channel.SendMessageAsync(":x: Вы не модератор Нейроматрицы!");
                return;
            }

            if (Context.Client.Guilds.Where(x => x.Id == GuildId).Count() < 1)
            {
                await Context.Channel.SendMessageAsync(":x: Я не состою в гильдии id=" + GuildId);
                return;
            }

            SocketGuild Guild = Context.Client.Guilds.Where(x => x.Id == GuildId).FirstOrDefault();
            var Invites = await Guild.GetInvitesAsync();
            if (Invites.Count() < 1)
            {
                try
                {
                    await Guild.TextChannels.First().CreateInviteAsync();
                }
                catch (Exception ex)
                {
                    await Context.Channel.SendMessageAsync($":x: Создание инвайта для гильдии {Guild.Name} пошло не так, ошибка: ``{ex.Message}``");
                    return;
                }
            }

            Invites = null;
            Invites = await Guild.GetInvitesAsync();
            EmbedBuilder Embed = new EmbedBuilder();
            Embed.WithAuthor($"Инвайт для гильдии {Guild.Name}", Guild.IconUrl);
            Embed.WithColor(40, 155, 200);
            foreach (var Current in Invites)
                Embed.AddInlineField("Инвайт:", $"[Инвайт]({Current.Url})");

            await Context.Channel.SendMessageAsync("", false, Embed.Build());
        }
    }
}

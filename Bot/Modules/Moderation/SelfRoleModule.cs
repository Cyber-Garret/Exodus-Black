using Bot.Models;
using Bot.Preconditions;
using Bot.Services.Data;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Modules
{
	[RequireUserPermission(GuildPermission.Administrator,
			ErrorMessage = ":x: | Прошу прощения страж, но эта команда доступна только капитану и его избранным стражам.",
			NotAGuildErrorMessage = GlobalVariables.NotInGuildText)]
	[Cooldown(5)]
	public class SelfRoleModule : InteractiveBase
	{
		GuildDataService guildData;

		public SelfRoleModule(IServiceProvider service)
		{
			guildData = service.GetRequiredService<GuildDataService>();
		}

		[Command("ДобавитьРоль"), Alias("др")]
		[Summary("Роль должна быть с возможностью @упоминания. Эмодзи можно использовать только серверные.")]
		[Remarks("Синтаксис: !др <@роль> <Эмодзи сервера>")]
		public async Task SaveGuildRole(SocketRole role = null, [Remainder] string text = null)
		{
			Emote emote;
			try
			{
				emote = Emote.Parse(text);
			}
			catch
			{
				await ReplyAndDeleteAsync("Капитан, я не смогла проиндексировать эмодзи. Ты уверен что используешь серверный эмодзи?");
				return;
			}

			if (role == null || emote == null)
			{
				await ReplyAndDeleteAsync("Капитан, ты не указал роль и\\или эмодзи.");
				return;
			}
			var guild = guildData.GetGuildAccount(Context.Guild);

			if (!guild.GuildSelfRoles.Any(r => r.RoleID == role.Id || r.EmoteID == emote.Id))
			{
				var selfrole = new GuildSelfRole
				{
					EmoteID = emote.Id,
					RoleID = role.Id
				};
				guild.GuildSelfRoles.Add(selfrole);
				guildData.SaveAccounts(Context.Guild);

				await ReplyAndDeleteAsync($"Успех! Капитан я сохранила данную связку роли {role.Mention} и {emote} в своей базе данных.", timeout: TimeSpan.FromSeconds(30));
			}
			else
				await ReplyAndDeleteAsync("Капитан, роль и\\или эмодзи что ты указал уже используються в системе авторолей.");

		}

		[Command("УдалитьРоли"), Alias("ур")]
		[Summary("Очищает список ролей для использования в сообщении автороли.")]
		public async Task ClearGuildSelfRoles()
		{
			//Clear SelfRole Message ID and save default value
			var guild = guildData.GetGuildAccount(Context.Guild);

			guild.SelfRoleMessageId = 0;
			guild.GuildSelfRoles.Clear();

			//Clear all self role associated with guild and save
			guildData.SaveAccounts(Context.Guild);

			await ReplyAndDeleteAsync("Капитан, я удалила все записанные в моей базе роли для твоего корабля.");
		}

		[Command("СписокРолей"), Alias("ср")]
		[Summary("Отображает список авторолей и привязанные к ним эмодзи.")]
		public async Task ListGuildRole()
		{
			var roles = await DatabaseHelper.GetGuildAllSelfRolesAsync(Context.Guild.Id);
			if (roles.Count > 0)
			{
				var message = "В моей базе записанны такие автороли и эмодзи привязанные к ним:\n";
				foreach (var item in roles)
				{
					var emote = await Context.Guild.GetEmoteAsync(item.EmoteID);
					var role = Context.Guild.GetRole(item.RoleID);
					message += $"{emote} - {role.Mention}\n";
				}
				await ReplyAndDeleteAsync(message, timeout: TimeSpan.FromMinutes(1));
			}
			await ReplyAndDeleteAsync("Капитан, в мой базе не записанно ни одной автороли.");
		}

		[Command("РазместитьРоли"), Alias("рр")]
		[Summary("Размещает сообщение с доступными авторолями, автоматическим заголовком сервера и любым сообщением с поддержкой discord синтаксиса(можно даже ссылки)")]
		[Remarks("Синтаксис: !рр <сообщение>")]
		[RequireBotPermission(ChannelPermission.AddReactions | ChannelPermission.ReadMessageHistory | ChannelPermission.ManageRoles,
			ErrorMessage = "Капитан, у меня нет прав на [Добавлять реакции]и\\или[Читать историю сообщений]и\\или[Управлять ролями]")]
		public async Task DeploySelfRoleMessage([Remainder]string text = null)
		{
			if (string.IsNullOrWhiteSpace(text)) return;

			var guild = await DatabaseHelper.GetGuildAccountAsync(Context.Guild.Id);
			var guildRoles = await DatabaseHelper.GetGuildAllSelfRolesAsync(Context.Guild.Id);
			if (guildRoles.Count() < 2)
			{
				await ReplyAndDeleteAsync($"Капитан, добавль больше авторолей чтобы я могла разместить сообщение.");
				return;
			}

			var message = await ReplyAsync(embed: await EmbedsHelper.SelfRoleMessageAsync(Context, guildRoles, text));

			guild.SelfRoleMessageId = message.Id;
			await DatabaseHelper.SaveGuildAccountAsync(guild);

			foreach (var role in guildRoles)
			{
				var emote = await Context.Guild.GetEmoteAsync(role.EmoteID);
				await message.AddReactionAsync(emote);
			}
		}
	}
}

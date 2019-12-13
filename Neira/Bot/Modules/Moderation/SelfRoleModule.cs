using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Neira.Bot.Preconditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neira.Bot.Helpers;
using Neira.Database;

namespace Neira.Bot.Modules
{
	[RequireUserPermission(GuildPermission.Administrator,
			ErrorMessage = ":x: | Прошу прощения страж, но эта команда доступна только капитану корабля и его избранным стражам.",
			NotAGuildErrorMessage = GlobalVariables.NotInGuildText)]
	[Cooldown(5)]
	public class SelfRoleModule : BaseModule
	{
		private readonly DiscordSocketClient _discord;
		public SelfRoleModule(IServiceProvider service)
		{
			_discord = service.GetRequiredService<DiscordSocketClient>();
		}

		[Command("др")]
		public async Task SaveGuildRole(SocketRole role = null, Emote emote = null)
		{
			if (role == null && emote == null) return;

			if (!DatabaseHelper.GetGuildSelfRole(Context.Guild.Id, role.Id, emote.Id))
			{
				var selfrole = new GuildSelfRole
				{
					GuildId = Context.Guild.Id,
					EmoteId = emote.Id,
					RoleId = role.Id
				};
				await DatabaseHelper.SaveGuildSelfRoleAsync(selfrole);

				await ReplyAndDeleteAsync("роль и смайлик сохранены");
			}
			await ReplyAndDeleteAsync("роль или смайлик не сохранены");
		}

		[Command("ор")]
		public async Task RemoveGuildRole()
		{
			await DatabaseHelper.ClearGuildSelfRoleAsync(Context.Guild.Id);
			await ReplyAndDeleteAsync("роли удалены");
		}

		[Command("cр")]
		public async Task ListGuildRole()
		{
			var roles = DatabaseHelper.GetGuildAllSelfRoles(Context.Guild.Id);
			await ReplyAndDeleteAsync($"ролей {roles.Count()}");
		}

		[Command("рс")]
		public async Task DeploySelfRoleMessage(string text = null)
		{
			var guild = await DatabaseHelper.GetGuildAccountAsync(Context.Guild.Id);
			if (guild.GuildSelfRoles.Count() < 2)
			{
				await ReplyAndDeleteAsync($"всего {guild.GuildSelfRoles.Count()} ролей.");
				return;
			}
			var message = await ReplyAsync(text);

			guild.SelfRoleMessageId = message.Id;
			await DatabaseHelper.SaveGuildAccountAsync(guild);

			foreach (var role in guild.GuildSelfRoles)
			{
				var emote = await _discord.GetGuild(Context.Guild.Id).GetEmoteAsync(role.EmoteId);
				await message.AddReactionAsync(emote);
			}
		}
	}
}

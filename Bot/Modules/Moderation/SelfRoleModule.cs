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
			NotAGuildErrorMessage = NotInGuildText)]
	[Cooldown(5)]
	public class SelfRoleModule : BaseModule
	{
		private readonly GuildDataService guildData;

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
				await ReplyAndDeleteAsync(@"Капитан, ты не указал роль и\\или эмодзи.");
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
				await ReplyAndDeleteAsync(@"Капитан, роль и\\или эмодзи что ты указал уже используються в системе авторолей.");

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
			var guild = guildData.GetGuildAccount(Context.Guild);
			if (guild.GuildSelfRoles.Count > 0)
			{
				var message = "В моей базе записанны такие автороли и эмодзи привязанные к ним:\n";
				foreach (var item in guild.GuildSelfRoles)
				{
					var emote = await Context.Guild.GetEmoteAsync(item.EmoteID);
					var role = Context.Guild.GetRole(item.RoleID);
					message += $"{emote} - {role.Mention}\n";
				}
				await ReplyAndDeleteAsync(message, timeout: TimeSpan.FromMinutes(1));
			}
			else
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

			var guild = guildData.GetGuildAccount(Context.Guild);
			if (guild.GuildSelfRoles.Count < 2)
			{
				await ReplyAndDeleteAsync($"Капитан, добавь больше авторолей чтобы я могла разместить сообщение.");
				return;
			}

			var message = await ReplyAsync(embed: await SelfRoleMessageAsync(Context, guild.GuildSelfRoles, text));

			guild.SelfRoleMessageId = message.Id;
			guildData.SaveAccounts(Context.Guild);

			foreach (var role in guild.GuildSelfRoles)
			{
				var emote = await Context.Guild.GetEmoteAsync(role.EmoteID);
				await message.AddReactionAsync(emote);
			}
		}

		#region Methods
		private static async Task<Embed> SelfRoleMessageAsync(SocketCommandContext Context, List<GuildSelfRole> GuildRoles, string text)
		{
			//Initial Embed
			var embed = new EmbedBuilder
			{
				Color = Color.Gold,
				Description = text
			};
			//Add guild as Author
			embed.WithAuthor(Context.Guild.Name, Context.Guild.IconUrl);
			//Create field with roles and associated emotes
			var embedField = new EmbedFieldBuilder
			{
				Name = InvisibleString
			};
			foreach (var item in GuildRoles)
			{
				var emote = await Context.Guild.GetEmoteAsync(item.EmoteID);
				var role = Context.Guild.GetRole(item.RoleID);
				embedField.Value += $"Нажми на {emote} что бы получить роль {role.Mention}\n";
			}
			embed.AddField(embedField);

			return embed.Build();
		}
		#endregion

	}
}

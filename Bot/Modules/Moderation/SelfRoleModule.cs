using Bot.Core.Data;
using Bot.Models;
using Bot.Preconditions;
using Bot.Properties;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.Modules
{
	[RequireUserPermission(GuildPermission.Administrator,
			ErrorMessage = ":x: | Прошу прощения страж, но эта команда доступна только капитану и его избранным стражам.",
			NotAGuildErrorMessage = NotInGuildText)]
	[Cooldown(5)]
	public class SelfRoleModule : BaseModule
	{

		[Command("ДобавитьРоль"), Alias("др")]
		[Summary("Роль должна быть с возможностью @упоминания. Эмодзи можно использовать только серверные.")]
		public async Task SaveGuildRole(SocketRole role = null, [Remainder] string text = null)
		{
			if (role == null || text == null)
			{
				await ReplyAndDeleteAsync(Resources.SlfRolRoleOrEmojIsNull);
				return;
			}

			Emote emote;
			try
			{
				emote = Emote.Parse(text);
			}
			catch
			{
				await ReplyAndDeleteAsync(Resources.SlfRolErrorEmoji);
				return;
			}

			var guild = GuildData.GetGuildAccount(Context.Guild);

			if (!guild.GuildSelfRoles.Any(r => r.RoleID == role.Id || r.EmoteID == emote.Id))
			{
				var selfrole = new GuildSelfRole
				{
					EmoteID = emote.Id,
					RoleID = role.Id
				};
				guild.GuildSelfRoles.Add(selfrole);
				GuildData.SaveAccounts(Context.Guild);

				await ReplyAndDeleteAsync(string.Format(Resources.SlfRolSucAdd, role.Mention, emote), timeout: TimeSpan.FromSeconds(30));
			}
			else
				await ReplyAndDeleteAsync(Resources.SlfRolRoleOrEmojiExist);

		}

		[Command("УдалитьРоли"), Alias("ур")]
		[Summary("Очищает список ролей для использования в сообщении автороли.")]
		public async Task ClearGuildSelfRoles()
		{
			var guild = GuildData.GetGuildAccount(Context.Guild);

			//Clear all self role associated with guild and save
			guild.SelfRoleMessageId = 0;
			guild.GuildSelfRoles.Clear();
			GuildData.SaveAccounts(Context.Guild);

			await ReplyAndDeleteAsync(Resources.SlfRolClear);
		}

		[Command("СписокРолей"), Alias("ср")]
		[Summary("Отображает список авторолей и привязанные к ним эмодзи.")]
		public async Task ListGuildRole()
		{
			var guild = GuildData.GetGuildAccount(Context.Guild);
			if (guild.GuildSelfRoles.Count > 0)
			{
				var message = Resources.SlfRolList;
				foreach (var item in guild.GuildSelfRoles)
				{
					var emote = await Context.Guild.GetEmoteAsync(item.EmoteID);
					var role = Context.Guild.GetRole(item.RoleID);
					message += $"{emote} - {role.Mention}\n";
				}
				await ReplyAndDeleteAsync(message, timeout: TimeSpan.FromMinutes(1));
			}
			else
				await ReplyAndDeleteAsync(Resources.SlfRolEmpty);
		}

		[Command("РазместитьРоли"), Alias("рр")]
		[Summary("Размещает сообщение с доступными авторолями, автоматическим заголовком сервера и любым сообщением с поддержкой discord синтаксиса(можно даже ссылки)")]
		[RequireBotPermission(ChannelPermission.AddReactions | ChannelPermission.ReadMessageHistory | ChannelPermission.ManageRoles,
			ErrorMessage = "Капитан, у меня нет прав на [Добавлять реакции]и\\или[Читать историю сообщений]и\\или[Управлять ролями]")]
		public async Task DeploySelfRoleMessage([Remainder]string text = null)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				await ReplyAndDeleteAsync(Resources.SlfRolNoText);
				return;
			}

			var guild = GuildData.GetGuildAccount(Context.Guild);
			if (guild.GuildSelfRoles.Count > 0)
			{
				var message = await ReplyAsync(embed: await SelfRoleMessageAsync(Context, guild.GuildSelfRoles, text));

				guild.SelfRoleMessageId = message.Id;
				GuildData.SaveAccounts(Context.Guild);

				foreach (var role in guild.GuildSelfRoles)
				{
					var emote = await Context.Guild.GetEmoteAsync(role.EmoteID);
					await message.AddReactionAsync(emote);
				}
			}
			else
				await ReplyAndDeleteAsync(Resources.SlfRolEmpty);


		}

		#region Methods
		private static async Task<Embed> SelfRoleMessageAsync(SocketCommandContext Context, List<GuildSelfRole> GuildRoles, string text)
		{
			//Initial Embed
			var embed = new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					IconUrl = Context.Guild.IconUrl,
					Name = Context.Guild.Name
				},
				Color = Color.Gold,
				Description = text
			};

			//Create field with roles and associated emotes
			var embedField = new EmbedFieldBuilder
			{
				Name = "\u200b"
			};
			foreach (var item in GuildRoles)
			{
				var emote = await Context.Guild.GetEmoteAsync(item.EmoteID);
				var role = Context.Guild.GetRole(item.RoleID);
				embedField.Value += string.Format(Resources.SlfRolEmbDescField, emote, role.Mention);
			}
			embed.AddField(embedField);

			return embed.Build();
		}
		#endregion

	}
}

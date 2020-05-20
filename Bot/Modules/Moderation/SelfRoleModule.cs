using Bot.Core.Data;
using Bot.Models;
using Bot.Preconditions;
using Bot.Properties;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.Modules
{
	[RequireUserPermission(GuildPermission.Administrator)]
	[Cooldown(5)]
	public class SelfRoleModule : BaseModule
	{
		private readonly ILogger<SelfRoleModule> logger;

		public SelfRoleModule(ILogger<SelfRoleModule> logger)
		{
			this.logger = logger;
		}

		[Command("addrole"), Alias("ДобавитьРоль")]
		public async Task SaveGuildRole(SocketRole role = null, [Remainder] string text = null)
		{
			try
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
			catch (Exception ex)
			{
				await ReplyAsync($"Error: {ex.Message}");
				logger.LogError(ex, "add role");
			}
		}

		[Command("clearroles"), Alias("УдалитьРоли")]
		public async Task ClearGuildSelfRoles()
		{
			try
			{
				var guild = GuildData.GetGuildAccount(Context.Guild);

				//Clear all self role associated with guild and save
				guild.SelfRoleMessageId = 0;
				guild.GuildSelfRoles.Clear();
				GuildData.SaveAccounts(Context.Guild);

				await ReplyAndDeleteAsync(Resources.SlfRolClear);
			}
			catch (Exception ex)
			{
				await ReplyAsync($"Error: {ex.Message}");
				logger.LogError(ex, "Remove role");
			}
		}

		[Command("rolelist"), Alias("СписокРолей")]
		public async Task ListGuildRole()
		{
			try
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
			catch (Exception ex)
			{
				await ReplyAsync($"Ошибка: {ex.Message}");
				logger.LogError(ex, "Role list error");
			}
		}

		[Command("placeroles"), Alias("РазместитьРоли")]
		[RequireBotPermission(ChannelPermission.AddReactions | ChannelPermission.ReadMessageHistory | ChannelPermission.ManageRoles)]
		public async Task DeploySelfRoleMessage([Remainder] string text = null)
		{
			try
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
			catch (Exception ex)
			{
				await ReplyAsync($"Error: {ex.Message}");
				logger.LogError(ex, "Place role error");
			}
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
				Description = text,
				Footer = new EmbedFooterBuilder
				{
					IconUrl = Resources.NeiraFooterIcon,
					Text = Resources.MyAd
				}
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

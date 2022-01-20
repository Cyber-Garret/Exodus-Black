using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Failsafe.Core.Data;
using Failsafe.Models;
using Failsafe.Preconditions;
using Failsafe.Properties;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Failsafe.Modules.Moderation
{
	[RequireContext(ContextType.Guild), Cooldown(5), RequireUserPermission(GuildPermission.Administrator)]
	public class SelfRoleModule : RootModule
	{
		private readonly ILogger<SelfRoleModule> logger;

		public SelfRoleModule(ILogger<SelfRoleModule> logger)
		{
			this.logger = logger;
		}

		[Command("AddRole"), Alias("ДобавитьРоль", "ДодатиРоль")]
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

				if (!guild.GuildSelfRoles.Any(r => r.RoleId == role.Id || r.EmoteId == emote.Id))
				{
					var selfrole = new GuildSelfRole
					{
						EmoteId = emote.Id,
						RoleId = role.Id
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

		[Command("ClearRoles"), Alias("УдалитьРоли", "ВидалитиРолі")]
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

		[Command("RoleList"), Alias("СписокРолей")]
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
						var emote = await Context.Guild.GetEmoteAsync(item.EmoteId);
						var role = Context.Guild.GetRole(item.RoleId);
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

		[Command("PlaceRoles"), Alias("РазместитьРоли", "РозташуватиРолі")]
		[RequireBotPermission(GuildPermission.AddReactions | GuildPermission.ManageRoles)]
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
						var emote = await Context.Guild.GetEmoteAsync(role.EmoteId);
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
				Description = text
			};

			//Create field with roles and associated emotes
			var embedField = new EmbedFieldBuilder
			{
				Name = "\u200b"
			};
			foreach (var item in GuildRoles)
			{
				var emote = await Context.Guild.GetEmoteAsync(item.EmoteId);
				var role = Context.Guild.GetRole(item.RoleId);
				embedField.Value += string.Format(Resources.SlfRolEmbDescField, emote, role.Mention) + Environment.NewLine;
			}
			embed.AddField(embedField);

			return embed.Build();
		}
		#endregion

	}
}

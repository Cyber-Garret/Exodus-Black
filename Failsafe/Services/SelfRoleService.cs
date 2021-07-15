using Discord;
using Discord.WebSocket;

using Failsafe.Core.Data;

using Microsoft.Extensions.Logging;

using System;
using System.Threading.Tasks;

namespace Failsafe.Services
{
    public class SelfRoleService
    {
        private readonly ILogger<SelfRoleService> _logger;
        private readonly DiscordSocketClient _discord;

        public SelfRoleService(ILogger<SelfRoleService> logger, DiscordSocketClient discord)
        {
            _logger = logger;
            _discord = discord;
        }

        public async Task SelfRoleReactionAdded(Cacheable<IUserMessage, ulong> cache, SocketReaction reaction)
        {
            try
            {
                var msg = await cache.GetOrDownloadAsync();

                var guild = GuildData.FindGuildBySelfRoleMessage(msg.Id);

                if (guild == null || guild.GuildSelfRoles.Count < 0) return;

                foreach (var selfRole in guild.GuildSelfRoles)
                {
                    var emote = await _discord.GetGuild(guild.Id).GetEmoteAsync(selfRole.EmoteId);
                    if (reaction.Emote.Equals(emote))
                    {
                        var user = _discord.GetGuild(guild.Id).GetUser(reaction.UserId);
                        var role = _discord.GetGuild(guild.Id).GetRole(selfRole.RoleId);
                        await user.AddRoleAsync(role);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SelfRoleReactionAdded");
            }
        }
        public async Task SelfRoleReactionRemoved(Cacheable<IUserMessage, ulong> cache, SocketReaction reaction)
        {
            try
            {
                var msg = await cache.GetOrDownloadAsync();

                var guild = GuildData.FindGuildBySelfRoleMessage(msg.Id);

                if (guild == null || guild.GuildSelfRoles.Count < 0) return;

                foreach (var selfRole in guild.GuildSelfRoles)
                {
                    var emote = await _discord.GetGuild(guild.Id).GetEmoteAsync(selfRole.EmoteId);
                    if (reaction.Emote.Equals(emote))
                    {
                        var user = _discord.GetGuild(guild.Id).GetUser(reaction.UserId);
                        var role = _discord.GetGuild(guild.Id).GetRole(selfRole.RoleId);
                        await user.RemoveRoleAsync(role);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SelfRoleReactionRemoved");
            }
        }
    }
}

using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Neuromatrix.Data;
using Neuromatrix.Modules.Administration;

namespace Neuromatrix.Services
{
    public class DiscordEventHandlerService
    {
        #region Private Fields
        private readonly DiscordShardedClient _client;
        private readonly CommandHandlingService _commandHandlingService;
        private readonly ServerActivityLogger _serverActivityLogger;
        #endregion


        public DiscordEventHandlerService(DiscordShardedClient client, CommandHandlingService command, ServerActivityLogger serverActivityLogger)
        {
            _client = client;
            _commandHandlingService = command;
            _serverActivityLogger = serverActivityLogger;
        }

        public void Configure()
        {
            _client.JoinedGuild += _client_JoinedGuild;
            _client.ChannelCreated += _client_ChannelCreatedAsync;
            _client.ChannelDestroyed += _client_ChannelDestroyedAsync;
            _client.GuildMemberUpdated += _client_GuildMemberUpdatedAsync;
            _client.MessageDeleted += _client_MessageDeletedAsync;
            _client.MessageReceived += _client_MessageReceived;
            _client.MessageUpdated += _client_MessageUpdatedAsync;
            _client.RoleCreated += _client_RoleCreatedAsync;
            _client.RoleDeleted += _client_RoleDeletedAsync;
            _client.UserLeft += _client_UserLeftAsync;
        }

        private Task _client_JoinedGuild(SocketGuild guild)
        {
            Database.CreateGuildAccount(guild);
            return Task.CompletedTask;
        }

        private async Task _client_ChannelCreatedAsync(SocketChannel arg)
        {
            await _serverActivityLogger.ChannelCreated(arg);
        }

        private async Task _client_ChannelDestroyedAsync(SocketChannel arg)
        {
            await _serverActivityLogger.ChannelDestroyed(arg);
        }

        private async Task _client_GuildMemberUpdatedAsync(SocketGuildUser userBefore, SocketGuildUser userAfter)
        {
            await _serverActivityLogger.GuildMemberUpdated(userBefore, userAfter);
        }

        private async Task _client_MessageDeletedAsync(Cacheable<IMessage, ulong> cacheMessage, ISocketMessageChannel channel)
        {
            if (!cacheMessage.HasValue)
                return;
            if (cacheMessage.Value.Author.IsBot)
                return;
            await _serverActivityLogger.MessageDeleted(cacheMessage, channel);
        }

        private async Task _client_MessageReceived(SocketMessage message)
        {
            if (message.Author.IsBot)
                return;
            await _commandHandlingService.HandleCommandAsync(message);
            await _serverActivityLogger.MessageReceived(message);
        }

        private async Task _client_MessageUpdatedAsync(Cacheable<IMessage, ulong> cacheMessageBefore, SocketMessage messageAfter, ISocketMessageChannel channel)
        {
            if (!cacheMessageBefore.HasValue)
                return;
            if (cacheMessageBefore.Value.Author.IsBot)
                return;
            await _serverActivityLogger.MessageUpdated(cacheMessageBefore, messageAfter, channel);
        }

        private async Task _client_RoleCreatedAsync(SocketRole arg)
        {
            await _serverActivityLogger.RoleCreated(arg);
        }

        private async Task _client_RoleDeletedAsync(SocketRole arg)
        {
            await _serverActivityLogger.RoleDeleted(arg);
        }

        private async Task _client_UserLeftAsync(SocketGuildUser arg)
        {
            await _serverActivityLogger.UserLeft(arg);
        }
    }
}

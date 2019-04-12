using System;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Neuromatrix.Data;
using Neuromatrix.Helpers;
using Neuromatrix.Modules.Administration;

namespace Neuromatrix.Services
{
    public class DiscordEventHandlerService
    {
        #region Private Fields
        private readonly DiscordShardedClient _client = Program._client;
        private readonly CommandHandlerService _commandHandlingService;
        private readonly ServerActivityLogger _serverActivityLogger;
        #endregion


        public DiscordEventHandlerService(CommandHandlerService command, ServerActivityLogger serverActivityLogger)
        {
            _commandHandlingService = command;
            _serverActivityLogger = serverActivityLogger;
        }

        public void Configure()
        {
            //_client.ShardConnected += _client_ShardConnected;
            _client.ShardDisconnected += _client_ShardDisconnected;
            _client.JoinedGuild += _client_JoinedGuild;
            _client.ChannelCreated += _client_ChannelCreatedAsync;
            _client.ChannelDestroyed += _client_ChannelDestroyedAsync;
            _client.GuildMemberUpdated += _client_GuildMemberUpdatedAsync;
            _client.MessageDeleted += _client_MessageDeletedAsync;
            _client.MessageReceived += _client_MessageReceived;
            _client.MessageUpdated += _client_MessageUpdatedAsync;
            _client.RoleCreated += _client_RoleCreatedAsync;
            _client.RoleDeleted += _client_RoleDeletedAsync;
            _client.UserJoined += _client_UserJoinedAsync;
            _client.UserLeft += _client_UserLeftAsync;
        }
        #region Events
        private Task _client_ShardDisconnected(Exception arg1, DiscordSocketClient arg2)
        {
            Console.WriteLine($"Shard {arg2.ShardId} Disconnected");
            return Task.CompletedTask;
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

        private async Task _client_UserJoinedAsync(SocketGuildUser arg)
        {
            await UserJoined(arg);
        }

        private async Task _client_UserLeftAsync(SocketGuildUser arg)
        {
            await _serverActivityLogger.UserLeft(arg);
        }
        #endregion

        #region Methods
        private async Task UserJoined(SocketGuildUser user)
        {
            try
            {
                #region Checks
                if (user == null || user.IsBot) return;

                var guild = Database.GetGuildAccount(user.Guild);
                if (string.IsNullOrWhiteSpace(guild.WelcomeMessage)) return;
                #endregion

                IDMChannel dM = await user.GetOrCreateDMChannelAsync();

                await dM.SendMessageAsync(null, false, MiscHelpers.WelcomeEmbed(user).Build());
            }
            catch
            {
                //TODO text log
            }
            
        }
        #endregion

    }
}

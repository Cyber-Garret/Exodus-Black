using Bot.Services;

using Discord.Commands;
using Discord.WebSocket;

using System.Threading.Tasks;

namespace Bot.Modules
{
	public class MusicModule : BotModuleBase
	{
		private readonly MusicService musicService;
		public MusicModule(MusicService service)
		{
			musicService = service;
		}

		[Command("Включить")]
		public async Task MusicJoin()
			=> await ReplyAsync(null, false, await musicService.JoinOrPlayAsync((SocketGuildUser)Context.User, Context.Channel, Context.Guild.Id));

		[Command("Добавить")]
		public async Task MusicPlay([Remainder]string search)
			=> await ReplyAsync(null, false, await musicService.JoinOrPlayAsync((SocketGuildUser)Context.User, Context.Channel, Context.Guild.Id, search));

		[Command("Выключить")]
		public async Task MusicLeave()
			=> await ReplyAsync(null, false, await musicService.LeaveAsync((SocketGuildUser)Context.User, Context.Guild.Id));

		[Command("Очередь")]
		public async Task MusicQueue()
			=> await ReplyAsync(null, false, await musicService.ListAsync(Context.Guild.Id));

		[Command("Пропустить")]
		public async Task SkipTrack()
			=> await ReplyAsync(null, false, await musicService.SkipTrackAsync(Context.Guild.Id));

		[Command("Громкость")]
		public async Task Volume(int volume)
			=> await ReplyAsync(null, false, await musicService.VolumeAsync(Context.Guild.Id, volume));

		[Command("Пауза")]
		public async Task Pause()
		   => await ReplyAsync(null, false, await musicService.Pause(Context.Guild.Id));

		[Command("Продолжить")]
		public async Task Resume()
			=> await ReplyAsync(null, false, await musicService.Pause(Context.Guild.Id));
	}
}

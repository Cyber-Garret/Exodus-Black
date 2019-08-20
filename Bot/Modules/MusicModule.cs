using Bot.Services;

using Discord.Commands;
using Discord.WebSocket;

using System.Threading.Tasks;

namespace Bot.Modules
{
	[RequireContext(ContextType.Guild)]
	public class MusicModule : BotModuleBase
	{
		private readonly MusicService musicService;
		public MusicModule(MusicService service)
		{
			musicService = service;
		}

		[Command("включить")]
		[Alias("вкл")]
		public async Task MusicJoin()
			=> await ReplyAsync(null, false, await musicService.JoinOrPlayAsync(Context.User as SocketGuildUser, Context.Channel as SocketTextChannel, Context.Guild.Id));

		[Command("добавить")]
		public async Task MusicPlay([Remainder]string search)
			=> await ReplyAsync(null, false, await musicService.JoinOrPlayAsync(Context.User as SocketGuildUser, Context.Channel as SocketTextChannel, Context.Guild.Id, search));

		[Command("выключить")]
		[Alias("выкл")]
		public async Task MusicLeave()
			=> await ReplyAsync(null, false, await musicService.LeaveAsync(Context.User as SocketGuildUser, Context.Guild.Id));

		[Command("очередь")]
		public async Task MusicQueue()
			=> await ReplyAsync(null, false, await musicService.ListAsync(Context.Guild.Id));

		[Command("пропустить")]
		public async Task SkipTrack()
			=> await ReplyAsync(null, false, await musicService.SkipTrackAsync(Context.Guild.Id));

		[Command("громкость")]
		public async Task Volume(int volume)
			=> await ReplyAsync(null, false, await musicService.VolumeAsync(Context.Guild.Id, volume));

		[Command("пауза")]
		public async Task Pause()
		   => await ReplyAsync(null, false, await musicService.Pause(Context.Guild.Id));

		[Command("продолжить")]
		public async Task Resume()
			=> await ReplyAsync(null, false, await musicService.Pause(Context.Guild.Id));

		[Command("текст")]
		public async Task Lyrics()
		{
			var lyrics = await musicService.GetLyricsAsync(Context.Guild.Id);
			string part1 = null, part2 = null;
			if (lyrics.Length >= 2000)
			{
				part1 = lyrics.Remove(2000);
				part2 = lyrics.Substring(2000);
			}

			await ReplyAsync(part1);
			await ReplyAsync(part2);
		}
	}
}

using Neira.Bot.Services;

using Discord.Commands;
using Discord.WebSocket;

using System.Threading.Tasks;

namespace Neira.Bot.Modules
{
	[RequireContext(ContextType.Guild, ErrorMessage = "Прошу прощения страж, но музыка работает только на серверах Discord.")]
	public class MusicModule : BotModuleBase
	{
		private readonly MusicService musicService;
		public MusicModule(MusicService service)
		{
			musicService = service;
		}

		[Command("играть")]
		[Summary("Производит поиск по заданной фразе в YouTube, если что-то было найдено, то добавляется первый трек в очередь.")]
		[Remarks("Например: **!поиск get lucky**")]
		public async Task MusicPlay([Remainder]string search = null)
			=> await ReplyAsync(embed: await musicService.PlayAsync(Context.User as SocketGuildUser, Context.Channel as SocketTextChannel, Context.Guild.Id, search));

		[Command("выключить")]
		[Alias("выкл")]
		[Summary("Отключает меня от аудиоканала, в котором я нахожусь.")]
		public async Task MusicLeave()
			=> await ReplyAsync(embed: await musicService.LeaveAsync(Context.User as SocketGuildUser, Context.Guild.Id));

		[Command("очередь")]
		[Summary("Выводит список треков в очереди.")]
		public async Task MusicQueue()
			=> await ReplyAsync(embed: await musicService.ListAsync(Context.Guild.Id));

		[Command("микс")]
		[Summary("Перемешивает плейлист")]
		public async Task ShuffleQueue()
			=> await ReplyAsync(embed: await musicService.ShuffleQueueAsync(Context.Guild.Id));

		[Command("очистить")]
		[Summary("Очищает плейлист от всех треков.")]
		public async Task ClearQueue()
			=> await ReplyAsync(embed: await musicService.ClearQueueAsync(Context.Guild.Id));

		[Command("трек")]
		[Summary("Отображает информацию о текущем треке")]
		public async Task GetTrackInfo()
			=> await ReplyAsync(embed: await musicService.GetCurrentTrackAsync(Context.Guild.Id));

		[Command("след")]
		[Alias("пропустить", "дальше")]
		[Summary("Пропускает текущий трек.")]
		public async Task SkipTrack()
			=> await ReplyAsync(embed: await musicService.SkipTrackAsync(Context.Guild.Id));

		[Command("громкость")]
		[Summary("Позволяет установить мою громкость в диапазоне от 1 до 149.")]
		[Remarks("Например: **!громкость 50**")]
		public async Task Volume(int volume)
			=> await ReplyAsync(embed: await musicService.VolumeAsync(Context.Guild.Id, volume));

		[Command("пауза")]
		[Summary("Ставит на паузу или снимает, все зависит в каком состоянии форсирующая частота.")]
		public async Task Pause()
		   => await ReplyAsync(embed: await musicService.Pause(Context.Guild.Id));
	}
}

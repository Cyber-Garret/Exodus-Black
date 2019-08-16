using Bot.Helpers;
using Bot.Models;

using Discord;
using Discord.WebSocket;

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Victoria;
using Victoria.Entities;

namespace Bot.Services
{
	public class MusicService
	{
		private readonly LavaSocketClient lavaSocket;
		private readonly LavaRestClient lavaRest;
		private LavaPlayer lavaPlayer;

		public MusicService(LavaRestClient lavaRestClient, LavaSocketClient lavaSocketClient)
		{
			lavaSocket = lavaSocketClient;
			lavaRest = lavaRestClient;
		}

		private readonly Lazy<ConcurrentDictionary<ulong, AudioOptions>> LazyOptions = new Lazy<ConcurrentDictionary<ulong, AudioOptions>>();

		private ConcurrentDictionary<ulong, AudioOptions> Options => LazyOptions.Value;

		public async Task<Embed> JoinOrPlayAsync(SocketGuildUser user, IMessageChannel textChannel, ulong guildId, string query = null)
		{
			if (user.VoiceChannel == null)
				return await MusicEmbedHelper.CreateErrorEmbed($"{Program.config.RadioModuleName}, Войти/Играть", "Страж, для начала зайди в аудиоканал доступный мне! Бип... ");

			if (Options.TryGetValue(user.Guild.Id, out var options) && options.Master.Id != user.Id)
				return await MusicEmbedHelper.CreateErrorEmbed($"{Program.config.RadioModuleName}, Войти/Играть", $"Я не могу сменить канал, пока {options.Master} не отключит меня. ");
			try
			{
				lavaPlayer = lavaSocket.GetPlayer(guildId);
				if (lavaPlayer == null)
				{

					await lavaSocket.ConnectAsync(user.VoiceChannel);
					Options.TryAdd(user.Guild.Id, new AudioOptions
					{
						Master = user
					});
					lavaPlayer = lavaSocket.GetPlayer(guildId);
				}

				if (query == null)
					return await MusicEmbedHelper.CreateErrorEmbed($"{Program.config.RadioModuleName} - Поиск", "Пожалуйста, укажи название трека или ссылку на него, чтобы я могла добавить его в плейлист.");

				LavaTrack track;
				var search = await lavaRest.SearchYouTubeAsync(query);

				if (search.LoadType == LoadType.NoMatches && query != null)
					return await MusicEmbedHelper.CreateErrorEmbed(Program.config.RadioModuleName, $"Извини, но по запросу {query}, я ничего не нашла.");
				if (search.LoadType == LoadType.LoadFailed && query != null)
					return await MusicEmbedHelper.CreateErrorEmbed(Program.config.RadioModuleName, $"Мне не удалось загрузить {query}.");

				track = search.Tracks.FirstOrDefault();

				if (lavaPlayer.CurrentTrack != null && lavaPlayer.IsPlaying || lavaPlayer.IsPaused)
				{
					lavaPlayer.Queue.Enqueue(track);
					return await MusicEmbedHelper.CreateBasicEmbed(Program.config.RadioModuleName, $"{track.Title} успешно добавлен в очередь.");
				}
				await lavaPlayer.PlayAsync(track);
				return await MusicEmbedHelper.CreateMusicEmbed(Program.config.RadioModuleName, $"Сейчас играет: [{track.Title}]({track.Uri})");
			}
			catch (Exception e)
			{
				return await MusicEmbedHelper.CreateErrorEmbed($"{Program.config.RadioModuleName}, Войти/Играть", e.Message);
			}
		}

		public async Task<Embed> LeaveAsync(SocketGuildUser user, ulong guildId)
		{
			try
			{
				var player = lavaSocket.GetPlayer(guildId);

				if (player.IsPlaying)
					await player.StopAsync();

				var channelName = player.VoiceChannel.Name;
				await lavaSocket.DisconnectAsync(user.VoiceChannel);
				return await MusicEmbedHelper.CreateBasicEmbed(Program.config.RadioModuleName, $"Отключаюсь от {channelName}.");
			}

			catch (InvalidOperationException e)
			{
				return await MusicEmbedHelper.CreateErrorEmbed("Покидаю аудио канал", e.Message);
			}
		}

		public async Task<Embed> ListAsync(ulong guildId)
		{
			//var config = FailsafeDbOperations.GetGuildAccountAsync(guildId);
			//var cmdPrefix = config.CommandPrefix;
			try
			{
				var descriptionBuilder = new StringBuilder();

				var player = lavaSocket.GetPlayer(guildId);
				if (player == null)
					return await MusicEmbedHelper.CreateErrorEmbed($"{Program.config.RadioModuleName} - Плей-лист", $"Не удалось подключить аудио модуль.\nСтраж, ты уверен, что используешь аудио модуль правильно? Если не уверен, советую посмотреть [справку](http://neira.link/Command) во вкладке **Аудио**. ");

				if (player.IsPlaying)
				{

					if (player.Queue.Count < 1 && player.CurrentTrack != null)
					{
						return await MusicEmbedHelper.CreateBasicEmbed($"Сейчас играет: {player.CurrentTrack.Title}", "В очереди нет других треков.");
					}
					else
					{
						var trackNum = 2;
						foreach (LavaTrack track in player.Queue.Items)
						{
							if (trackNum == 2) { descriptionBuilder.Append($"Следующий: [{track.Title}]({track.Uri})\n"); trackNum++; }
							else { descriptionBuilder.Append($"#{trackNum}: [{track.Title}]({track.Uri})\n"); trackNum++; }
						}
						return await MusicEmbedHelper.CreateBasicEmbed($"{Program.config.RadioModuleName} - Плей-лист", $"Сейчас играет: [{player.CurrentTrack.Title}]({player.CurrentTrack.Uri})\n{descriptionBuilder.ToString()}");
					}
				}
				else
				{
					return await MusicEmbedHelper.CreateErrorEmbed($"{Program.config.RadioModuleName} - Плей-лист", "Похоже, что аудио модуль сейчас ничего не проигрывает. Если это ошибка, пожалуйста, сообщите моему создателю.");
				}
			}
			catch (Exception ex)
			{
				return await MusicEmbedHelper.CreateErrorEmbed($"{Program.config.RadioModuleName} - Плей-лист", ex.Message);
			}

		}

		public async Task<Embed> SkipTrackAsync(ulong guildId)
		{
			//var config = await FailsafeDbOperations.GetGuildAccountAsync(guildId);
			//var cmdPrefix = config.CommandPrefix;

			try
			{
				var player = lavaSocket.GetPlayer(guildId);
				if (player == null)
					return await MusicEmbedHelper.CreateErrorEmbed($"{Program.config.RadioModuleName} - Плей-лист", $"Не удалось подключить аудио модуль.\nСтраж, ты уверен, что используешь аудио модуль правильно? Если не уверен, советую посмотреть [справку](http://neira.link/Command) во вкладке **Аудио**.");
				//if (player.Queue.Count == 1)
				//{
				//	await player.StopAsync();
				//	return await MusicEmbedHelper.CreateMusicEmbed($"{Program.config.RadioModuleName} - Пропуск трека", "Это был последний трек в очереди и потому я остановлю воспроизведение.");
				//}

				if (player.Queue.Count == 0)
					return await MusicEmbedHelper.CreateErrorEmbed($"{Program.config.RadioModuleName} - Пропуск трека", "В плейлисте пусто, чтобы что-то пропускать!");
				else
				{
					try
					{
						var currentTrack = player.CurrentTrack;
						await player.SkipAsync();
						return await MusicEmbedHelper.CreateBasicEmbed($"{Program.config.RadioModuleName} - Пропуск трека", $"Пропускаю {currentTrack.Title}");
					}
					catch (Exception ex)
					{
						return await MusicEmbedHelper.CreateErrorEmbed($"{Program.config.RadioModuleName} - Ошибка пропуска трека", ex.ToString());
					}

				}
			}
			catch (Exception ex)
			{
				return await MusicEmbedHelper.CreateErrorEmbed($"{Program.config.RadioModuleName} - Пропуск трека", ex.ToString());
			}
		}

		public async Task<Embed> VolumeAsync(ulong guildId, int volume)
		{
			if (volume >= 150 || volume <= 0)
			{
				return await MusicEmbedHelper.CreateErrorEmbed($"{Program.config.RadioModuleName} - Громкость", "Громкость должна быть в пределе от 1 до 149.");
			}
			try
			{
				var player = lavaSocket.GetPlayer(guildId);
				await player.SetVolumeAsync(volume);
				return await MusicEmbedHelper.CreateBasicEmbed($"{Program.config.RadioModuleName} - 🔊Громкость", $"Громкость установлена на уровень {volume}.");
			}
			catch (InvalidOperationException ex)
			{
				return await MusicEmbedHelper.CreateErrorEmbed($"{Program.config.RadioModuleName} - Громкость", $"{ex.Message}", "Пожалуйста, сообщите моему создателю если эта ошибка часто повторяется. ");
			}
		}

		public async Task<Embed> Pause(ulong guildId)
		{
			try
			{
				var player = lavaSocket.GetPlayer(guildId);
				if (player.IsPaused)
				{
					await player.ResumeAsync();
					return await MusicEmbedHelper.CreateMusicEmbed($"{Program.config.RadioModuleName} - ▶️", $"**Воспроизведение возобновлено:** Сейчас играет {player.CurrentTrack.Title}");
				}
				else
				{
					await player.PauseAsync();
					return await MusicEmbedHelper.CreateMusicEmbed($"{Program.config.RadioModuleName} - ⏸️", $"**Воспроизведение приостановлено:** {player.CurrentTrack.Title}");
				}
			}
			catch (InvalidOperationException e)
			{
				return await MusicEmbedHelper.CreateErrorEmbed($"{Program.config.RadioModuleName} Воспроизведение/Пауза", e.Message);
			}
		}

		public async Task OnTrackFinished(LavaPlayer player, LavaTrack track, TrackEndReason reason)
		{
			if (!reason.ShouldPlayNext())
				return;

			if (!player.Queue.TryDequeue(out var item) || !(item is LavaTrack nextTrack))
			{
				await player.TextChannel?.SendMessageAsync($"В плей-листе больше нет треков.");
				return;
			}

			await player.PlayAsync(nextTrack);

			EmbedBuilder embed = new EmbedBuilder();
			embed.WithDescription($"**Финальный трек: `{track.Title}`\nСейчас играет: `{nextTrack.Title}`**");
			//embed.WithColor();
			await player.TextChannel.SendMessageAsync(null, false, embed.Build());
			await player.TextChannel.SendMessageAsync(player.ToString());
		}

		public Task LogAsync(LogMessage logMessage)
		{
			Console.WriteLine(logMessage.Message);
			return Task.CompletedTask;
		}
	}
}

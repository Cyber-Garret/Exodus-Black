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
using Victoria.Helpers;

namespace Bot.Services
{
	public class MusicService
	{
		private readonly LavaSocketClient lavaSocket;
		private readonly LavaRestClient lavaRest;
		private LavaPlayer lavaPlayer;
		private readonly string MusicModuleName;

		public MusicService(LavaRestClient lavaRestClient, LavaSocketClient lavaSocketClient)
		{
			lavaSocket = lavaSocketClient;
			lavaRest = lavaRestClient;
			MusicModuleName = Program.config.RadioModuleName;
		}

		//private readonly Lazy<ConcurrentDictionary<ulong, AudioOptions>> LazyOptions = new Lazy<ConcurrentDictionary<ulong, AudioOptions>>();

		//private ConcurrentDictionary<ulong, AudioOptions> Options => LazyOptions.Value;

		public async Task<Embed> PlayAsync(SocketGuildUser user, ITextChannel textChannel, ulong guildId, string query = null)
		{
			if (user.VoiceChannel == null)
				return await MusicEmbedHelper.CreateErrorEmbed(MusicModuleName, "Страж, для начала зайди в аудиоканал доступный мне! Бип... ");

			//if (Options.TryGetValue(user.Guild.Id, out var options) && options.Master.Id != user.Id)
			//	return await MusicEmbedHelper.CreateErrorEmbed($"{MusicModuleName}, Войти/Играть", $"Я не могу сменить канал, пока {options.Master} не отключит меня. ");

			if (query == null)
			{
				lavaPlayer = lavaSocket.GetPlayer(guildId);
				if (lavaPlayer == null)
				{
					await lavaSocket.ConnectAsync(user.VoiceChannel, textChannel);
					//Options.TryAdd(user.Guild.Id, new AudioOptions
					//{
					//	Master = user
					//});
					await Logger.Log(new LogMessage(LogSeverity.Info, MusicModuleName, $"Модуль поключен к **{user.VoiceChannel.Name}** и привязан к **{textChannel.Name}**."));
					return await MusicEmbedHelper.CreateBasicEmbed(MusicModuleName, $"Модуль поключен к **{user.VoiceChannel.Name}** и привязан к **{textChannel.Name}**. Форсирующая частота подключена...");
				}
			}
			try
			{
				lavaPlayer = lavaSocket.GetPlayer(guildId);
				if (lavaPlayer == null)
				{
					await lavaSocket.ConnectAsync(user.VoiceChannel, textChannel);
					//Options.TryAdd(user.Guild.Id, new AudioOptions
					//{
					//	Master = user
					//});
					lavaPlayer = lavaSocket.GetPlayer(guildId);
				}

				var search = await lavaRest.SearchYouTubeAsync(query);

				//If we couldn't find anything, tell the user.
				if (search.LoadType == LoadType.NoMatches)
					return await MusicEmbedHelper.CreateErrorEmbed(MusicModuleName, $"Извини, но по запросу {query}, я ничего не нашла.");
				//If we can't load by user query, tell the user.
				if (search.LoadType == LoadType.LoadFailed)
					return await MusicEmbedHelper.CreateErrorEmbed(MusicModuleName, $"Мне не удалось загрузить {query}.");

				//TODO: Add a 1-5 list for the user to pick from. (Like Fredboat)
				var track = search.Tracks.FirstOrDefault();


				if (lavaPlayer.CurrentTrack != null && lavaPlayer.IsPlaying || lavaPlayer.IsPaused)
				{
					lavaPlayer.Queue.Enqueue(track);
					return await MusicEmbedHelper.CreateBasicEmbed(MusicModuleName, $"{track?.Title} успешно добавлен в очередь.");
				}

				await lavaPlayer.PlayAsync(track);
				return await MusicEmbedHelper.CreateMusicEmbed(MusicModuleName, $"Начинаю воспроизведение - [{track?.Title}]({track?.Uri})");
			}
			catch (Exception e)
			{
				return await MusicEmbedHelper.CreateErrorEmbed($"{MusicModuleName}, неизвестная ошибка во время действия Войти/Добавить", e.Message);
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

				return await MusicEmbedHelper.CreateBasicEmbed(MusicModuleName, $"Отключаю форсирующую частоту от {channelName}.");
			}

			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Critical, "Leave in music service", ex.Message, ex));
				return await MusicEmbedHelper.CreateErrorEmbed($"{MusicModuleName} ошибка отключения", ex.Message);
			}
		}

		public async Task<Embed> ListAsync(ulong guildId)
		{
			var config = await FailsafeDbOperations.GetGuildAccountAsync(guildId);
			var cmdPrefix = config.CommandPrefix ?? "!";
			try
			{
				var descriptionBuilder = new StringBuilder();

				var player = lavaSocket.GetPlayer(guildId);
				if (player == null)
					return await MusicEmbedHelper.CreateErrorEmbed($"{MusicModuleName} - Плей-лист", $"Не удалось подключить аудио модуль.\nСтраж, ты уверен, что используешь аудио модуль правильно? Если не уверен, советую посмотреть справку выполнив команду {cmdPrefix}справка во вкладке **{MusicModuleName}**. ");

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
							descriptionBuilder.Append($"#{trackNum}: [{track.Title}]({track.Uri})\n");
							trackNum++;
							//if (trackNum == 2) { descriptionBuilder.Append($"Следующий: [{track.Title}]({track.Uri})\n"); trackNum++; }
							//else { descriptionBuilder.Append($"#{trackNum}: [{track.Title}]({track.Uri})\n"); trackNum++; }
						}
						return await MusicEmbedHelper.CreateBasicEmbed($"{MusicModuleName} - Плей-лист", $"Сейчас играет: [{player.CurrentTrack.Title}]({player.CurrentTrack.Uri})\n{descriptionBuilder.ToString()}");
					}
				}
				else
				{
					return await MusicEmbedHelper.CreateErrorEmbed($"{MusicModuleName} - Плей-лист", "Похоже, что аудио модуль сейчас ничего не проигрывает. Если это ошибка, пожалуйста, сообщите моему создателю.");
				}
			}
			catch (Exception ex)
			{
				return await MusicEmbedHelper.CreateErrorEmbed($"{MusicModuleName} - Плей-лист", ex.Message);
			}

		}

		public async Task<Embed> ShuffleQueueAsync(ulong guildId)
		{
			var config = await FailsafeDbOperations.GetGuildAccountAsync(guildId);
			var cmdPrefix = config.CommandPrefix ?? "!";
			try
			{
				var descriptionBuilder = new StringBuilder();

				var player = lavaSocket.GetPlayer(guildId);
				if (player == null)
					return await MusicEmbedHelper.CreateErrorEmbed($"{MusicModuleName} - Плей-лист", $"Не удалось подключить аудио модуль.\nСтраж, ты уверен, что используешь аудио модуль правильно? Если не уверен, советую посмотреть справку выполнив команду {cmdPrefix}справка во вкладке **{MusicModuleName}**. ");

				if (player.IsPlaying)
				{

					if (player.Queue.Count <= 2 && player.CurrentTrack != null)
					{
						return await MusicEmbedHelper.CreateBasicEmbed($"Сейчас играет: {player.CurrentTrack.Title}", "В очереди мало треков для микса.");
					}
					else
					{
						//await player.StopAsync();
						player.Queue.Shuffle();

						var trackNum = 2;
						foreach (LavaTrack track in player.Queue.Items)
						{
							descriptionBuilder.Append($"#{trackNum}: [{track.Title}]({track.Uri})\n");
							trackNum++;
							//if (trackNum == 2) { descriptionBuilder.Append($"Следующий: [{track.Title}]({track.Uri})\n"); trackNum++; }
							//else { descriptionBuilder.Append($"#{trackNum}: [{track.Title}]({track.Uri})\n"); trackNum++; }
						}
						return await MusicEmbedHelper.CreateBasicEmbed($"{MusicModuleName} - Плей-лист", $"Сейчас играет: [{player.CurrentTrack.Title}]({player.CurrentTrack.Uri})\n{descriptionBuilder.ToString()}");
					}
				}
				else
				{
					return await MusicEmbedHelper.CreateErrorEmbed($"{MusicModuleName} - Плей-лист", "Похоже, что аудио модуль сейчас ничего не проигрывает. Если это ошибка, пожалуйста, сообщите моему создателю.");
				}
			}
			catch (Exception ex)
			{
				return await MusicEmbedHelper.CreateErrorEmbed($"{MusicModuleName} - Плей-лист", ex.Message);
			}
		}

		public async Task<Embed> ClearQueueAsync(ulong guildId)
		{
			var config = await FailsafeDbOperations.GetGuildAccountAsync(guildId);
			var cmdPrefix = config.CommandPrefix ?? "!";
			try
			{
				var descriptionBuilder = new StringBuilder();

				var player = lavaSocket.GetPlayer(guildId);
				if (player == null)
					return await MusicEmbedHelper.CreateErrorEmbed($"{MusicModuleName} - Плей-лист", $"Не удалось подключить аудио модуль.\nСтраж, ты уверен, что используешь аудио модуль правильно? Если не уверен, советую посмотреть справку выполнив команду {cmdPrefix}справка во вкладке **{MusicModuleName}**. ");

				if (player.IsPlaying)
					await player.StopAsync();

				player.Queue.Clear();
				return await MusicEmbedHelper.CreateMusicEmbed($"{MusicModuleName} - Плей-лист", "Плейлист был полностью очищен.");
			}
			catch (Exception ex)
			{
				return await MusicEmbedHelper.CreateErrorEmbed($"{MusicModuleName} - Плей-лист", ex.Message);
			}
		}

		public async Task<Embed> GetCurrentTrackAsync(ulong guildId)
		{
			var config = await FailsafeDbOperations.GetGuildAccountAsync(guildId);
			var cmdPrefix = config.CommandPrefix ?? "!";
			try
			{
				var descriptionBuilder = new StringBuilder();

				var player = lavaSocket.GetPlayer(guildId);
				if (player == null)
					return await MusicEmbedHelper.CreateErrorEmbed(MusicModuleName, $"Не удалось подключить аудио модуль.\nСтраж, ты уверен, что используешь аудио модуль правильно? Если не уверен, советую посмотреть справку выполнив команду {cmdPrefix}справка во вкладке **{MusicModuleName}**. ");

				if (player.IsPlaying)
					return await MusicEmbedHelper.CreateMusicEmbed(MusicModuleName,
						$"- Автор: **`{player.CurrentTrack.Author}`**\n" +
						$"- Трек: **`{player.CurrentTrack.Title}`** [Ссылка на трек]({player.CurrentTrack.Uri})\n" +
						$"- Позиция: **{player.CurrentTrack.Position.ToString(@"hh\:mm\:ss")}** \\ **{player.CurrentTrack.Length}**\n" +
						$"- Источник: **{player.CurrentTrack.Provider}**", thumbnailUrl: await player.CurrentTrack.FetchThumbnailAsync());


				return await MusicEmbedHelper.CreateMusicEmbed($"{MusicModuleName} - Плей-лист", "Плейлист был полностью очищен.");
			}
			catch (Exception ex)
			{
				return await MusicEmbedHelper.CreateErrorEmbed($"{MusicModuleName} - Плей-лист", ex.Message);
			}
		}

		public async Task<Embed> ToggleRepeatAsync
		public async Task<Embed> SkipTrackAsync(ulong guildId)
		{
			var config = await FailsafeDbOperations.GetGuildAccountAsync(guildId);
			var cmdPrefix = config.CommandPrefix;

			try
			{
				var player = lavaSocket.GetPlayer(guildId);
				if (player == null)
					return await MusicEmbedHelper.CreateErrorEmbed($"{MusicModuleName} - Плей-лист", $"Не удалось подключить аудио модуль.\nСтраж, ты уверен, что используешь аудио модуль правильно? Если не уверен, советую посмотреть справку выполнив команду {cmdPrefix}справка во вкладке **{MusicModuleName}**. ");

				if (player.Queue.Count < 1)
					return await MusicEmbedHelper.CreateBasicEmbed($"{MusicModuleName} - Пропуск трека", "Невозможно пропустить, так как в данный момент воспроизводится последний трек или в плейлисте пусто." +
						$"\n\nВозможно ты имел ввиду **{cmdPrefix}выключить**?");
				else
				{
					try
					{
						//save current track for embed message
						var currentTrack = player.CurrentTrack;
						await player.SkipAsync();
						return await MusicEmbedHelper.CreateBasicEmbed($"{MusicModuleName} - Пропуск трека", $"Пропускаю {currentTrack.Title}");
					}
					catch (Exception ex)
					{
						return await MusicEmbedHelper.CreateErrorEmbed($"{MusicModuleName} - Ошибка пропуска трека", ex.ToString());
					}

				}
			}
			catch (Exception ex)
			{
				return await MusicEmbedHelper.CreateErrorEmbed($"{MusicModuleName} - Ошибка пропуска трека", ex.ToString());
			}
		}

		public async Task<Embed> VolumeAsync(ulong guildId, int volume)
		{
			if (volume >= 150 || volume <= 0)
			{
				return await MusicEmbedHelper.CreateErrorEmbed($"{MusicModuleName} - 🔊Громкость", "Громкость должна быть в пределе от 1 до 149.");
			}
			try
			{
				var player = lavaSocket.GetPlayer(guildId);
				await player.SetVolumeAsync(volume);
				return await MusicEmbedHelper.CreateBasicEmbed($"{MusicModuleName} - 🔊Громкость", $"Громкость установлена на уровень {volume}.");
			}
			catch (InvalidOperationException ex)
			{
				return await MusicEmbedHelper.CreateErrorEmbed($"{MusicModuleName} - Громкость", $"{ex.Message}", "Пожалуйста, сообщите моему создателю если эта ошибка часто повторяется. ");
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
					return await MusicEmbedHelper.CreateMusicEmbed($"{MusicModuleName} - ▶️", $"**Воспроизведение возобновлено:** Сейчас играет {player.CurrentTrack.Title}");
				}
				else
				{
					await player.PauseAsync();
					return await MusicEmbedHelper.CreateMusicEmbed($"{MusicModuleName} - ⏸️", $"**Воспроизведение приостановлено:** {player.CurrentTrack.Title}");
				}
			}
			catch (InvalidOperationException e)
			{
				return await MusicEmbedHelper.CreateErrorEmbed($"{MusicModuleName} - Ошибка Возобновления/Паузы", e.Message);
			}
		}

		public async Task OnTrackFinished(LavaPlayer player, LavaTrack track, TrackEndReason reason)
		{
			if (!reason.ShouldPlayNext())
				return;

			if (!player.Queue.TryDequeue(out var item) || !(item is LavaTrack nextTrack))
			{
				await player.TextChannel.SendMessageAsync($":frowning: В плей-листе больше нет треков.");
				return;
			}

			await player.PlayAsync(nextTrack);

			await player.TextChannel.SendMessageAsync(embed: await MusicEmbedHelper.CreateMusicEmbed(MusicModuleName, $"**Предыдущий трек: `{track.Title}`\nСейчас играет: `{nextTrack.Title}`**"));
		}
	}
}

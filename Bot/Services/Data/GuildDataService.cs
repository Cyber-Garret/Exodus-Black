using Bot.Models;
using Discord;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Bot.Services.Data
{
	public class GuildDataService : DataConstants
	{
		private static readonly ConcurrentDictionary<ulong, Guild> guildAccounts = new ConcurrentDictionary<ulong, Guild>();

		private readonly ILogger logger;
		private readonly string RootDirectory;

		public GuildDataService(ILogger<GuildDataService> logger, IHostEnvironment host)
		{
			this.logger = logger;
			RootDirectory = host.ContentRootPath;
		}

		internal void LoadData()
		{
			var guildsFolder = Directory.CreateDirectory(Path.Combine(RootDirectory, RootFolder, GuildsFolder));
			var files = guildsFolder.GetFiles("*.json");
			if (files.Length > 0)
			{
				foreach (var file in files)
				{
					var guild = DataStorage.RestoreObject<Guild>(file.FullName);
					guildAccounts.TryAdd(guild.Id, guild);
				}
			}

			logger.LogInformation($"Загружено {guildAccounts.Count} серверных аккаунтов.");
		}

		internal Guild GetGuildAccount(IGuild guild)
		{
			return GetGuildAccount(guild.Id);
		}

		internal Guild GetGuildAccount(ulong id)
		{
			return guildAccounts.GetOrAdd(id, (key) =>
			{
				var newAccount = new Guild { Id = id };
				DataStorage.StoreObject(newAccount, Path.Combine(RootDirectory, RootFolder, GuildsFolder, $"{id}.json"), useIndentations: true);
				return newAccount;
			});
		}

		internal void SaveAccounts(params IGuild[] guilds)
		{
			foreach (var guild in guilds)
			{
				DataStorage.StoreObject(GetGuildAccount(guild.Id), Path.Combine(RootDirectory, RootFolder, GuildsFolder, $"{guild.Id}.json"), useIndentations: true);
			}
		}
		/// <summary>
		/// Saves one or multiple Discord guild accounts by provided Ids
		/// </summary>
		internal void SaveAccounts(params ulong[] ids)
		{
			foreach (var id in ids)
			{
				DataStorage.StoreObject(GetGuildAccount(id), Path.Combine(RootDirectory, RootFolder, GuildsFolder, $"{id}.json"), useIndentations: true);
			}
		}

		/// <summary>
		/// This rewrites ALL Discord guild accounts to the harddrive... Strongly recommend to use SaveAccounts(id1, id2, id3...) where possible instead this.
		/// </summary>
		internal void SaveAccounts()
		{
			foreach (var id in guildAccounts.Keys)
			{
				SaveAccounts(id);
			}
		}
	}
}

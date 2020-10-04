using Discord;

using Failsafe.Models;

using System.Collections.Concurrent;
using System.IO;
using System.Linq;

namespace Failsafe.Core.Data
{
	internal static class GuildData
	{
		private static readonly ConcurrentDictionary<ulong, Guild> GuildAccounts;

		static GuildData()
		{
			var result = DataStorage.LoadJSONFromHDD<Guild>(DataStorage.GuildsFolder);

			if (result != null)
				GuildAccounts = result.ToConcurrentDictionary(k => k.Id);
			else
				GuildAccounts = new ConcurrentDictionary<ulong, Guild>();
		}

		internal static Guild GetGuildAccount(IGuild guild)
		{
			return GetGuildAccount(guild.Id);
		}

		internal static Guild GetGuildAccount(ulong id)
		{
			return GuildAccounts.GetOrAdd(id, (key) =>
			{
				var newAccount = new Guild { Id = id };
				DataStorage.SaveObject(newAccount, Path.Combine(DataStorage.GuildsFolder, $"{id}.json"), useIndentations: true);
				return newAccount;
			});
		}

		/// <summary>
		/// Used for find guild settings by self role message id.
		/// </summary>
		/// <param name="id">Saved message ulong id</param>
		/// <returns>Full guild account settings</returns>
		internal static Guild FindGuildBySelfRoleMessage(ulong id)
		{
			return GuildAccounts.Values.FirstOrDefault(g => g.SelfRoleMessageId == id);
		}

		internal static void SaveAccounts(params IGuild[] guilds)
		{
			foreach (var guild in guilds)
			{
				DataStorage.SaveObject(GetGuildAccount(guild.Id), Path.Combine(DataStorage.GuildsFolder, $"{guild.Id}.json"), useIndentations: true);
			}
		}
		/// <summary>
		/// Saves one or multiple Discord guild accounts by provided Ids
		/// </summary>
		internal static void SaveAccounts(params ulong[] ids)
		{
			foreach (var id in ids)
			{
				DataStorage.SaveObject(GetGuildAccount(id), Path.Combine(DataStorage.GuildsFolder, $"{id}.json"), useIndentations: true);
			}
		}

		/// <summary>
		/// This rewrites ALL Discord guild accounts to the harddrive... Strongly recommend to use SaveAccounts(id1, id2, id3...) where possible instead this.
		/// </summary>
		internal static void SaveAccounts()
		{
			foreach (var id in GuildAccounts.Keys)
			{
				SaveAccounts(id);
			}
		}
	}
}

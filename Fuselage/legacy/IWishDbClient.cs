using System.Collections.Generic;

using Dapper;

using MySqlConnector;

using Neiralink.Models;

namespace Neiralink
{
	public interface IWishDbClient
	{
		IEnumerable<Wish> GetWishes(string lang);
	}

	public class WishDbClient : IWishDbClient
	{
		// Connection string for DB
		private readonly string conn;
		public WishDbClient(string connectionString)
		{
			conn = connectionString;
		}

		public IEnumerable<Wish> GetWishes(string lang)
		{
			using var db = new MySqlConnection(conn);
			return db.Query<Wish>(GetWishQuery(lang));

		}

		private static string GetWishQuery(string lang)
		{
			return $@"SELECT locale.Title, locale.Desc, base.WallScreenshot
							FROM Wish_Base AS base
							INNER JOIN Wish_{lang} AS locale ON base.WishNumber = locale.WishNumberID ORDER BY WishNumber";
		}
	}
}

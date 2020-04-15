using Dapper;

using Microsoft.Data.SqlClient;

using System.Linq;

namespace Web.Models
{
	public interface IBotRepository
	{
		BotStat GetBotStat(int id = 1);
	}

	public class BotRepository : IBotRepository
	{
		private readonly string connectionString = null;

		public BotRepository(string conn)
		{
			connectionString = conn;
		}

		public BotStat GetBotStat(int id = 1)
		{
			using var db = new SqlConnection(connectionString);
			var sql = $"SELECT * FROM BotStat WITH(NOLOCK) WHERE Id = {id}";
			return db.Query<BotStat>(sql).FirstOrDefault();
		}
	}
}

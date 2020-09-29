using Dapper;

using MySql.Data.MySqlClient;

using Neiralink.Enums;
using Neiralink.Models;

using System.Collections.Generic;
using System.Linq;

namespace Neiralink
{
	public interface IMilestoneDbClient
	{
		void AddMilestone(MilestoneInfo milestoneInfo);
		void AddMilestoneLocale(MilestoneInfoLocale infoLocale);
		void DeleteMilestone(byte id);
		void DeleteMilestoneLocale(byte id, LangKey lang);
		IEnumerable<MilestoneInfo> GetAllMilestoneInfos();

		MilestoneInfo GetMilestoneInfo(byte id);
		MilestoneInfoLocale GetMilestoneLocale(byte id, LangKey lang);
		IEnumerable<MilestoneInfoLocale> GetMilestoneLocales(byte id);

		void UpdateMilestone(MilestoneInfo milestoneInfo);
		void UpdateMilestoneLocale(MilestoneInfoLocale infoLocale);
	}

	public class MilesoneDbClient : IMilestoneDbClient
	{
		// Connection string for DB
		private readonly string conn;

		public MilesoneDbClient(string connectionString)
		{
			conn = connectionString;
		}

		public void AddMilestone(MilestoneInfo milestoneInfo)
		{
			using var db = new MySqlConnection(conn);

			var sqlQuery = "INSERT INTO MilestoneInfos (Icon, MaxSpace, MilestoneType, Game) VALUES(@Icon, @MaxSpace, @MilestoneType, @Game)";
			db.Execute(sqlQuery, milestoneInfo);
		}

		public void AddMilestoneLocale(MilestoneInfoLocale infoLocale)
		{
			using var db = new MySqlConnection(conn);

			var sqlQuery = "INSERT INTO MilestoneInfoLocales (MilestoneInfoRowID, LangKey, Name, Alias, Type) VALUES(@MilestoneInfoRowID, @LangKey, @Name, @Alias, @Type)";
			db.Execute(sqlQuery, infoLocale);
		}

		public void DeleteMilestone(byte id)
		{
			using var db = new MySqlConnection(conn);
			var sqlQuery = "DELETE FROM MilestoneInfos WHERE RowID = @id";
			db.Execute(sqlQuery, new { id });
		}

		public void DeleteMilestoneLocale(byte id, LangKey lang)
		{
			using var db = new MySqlConnection(conn);
			var sqlQuery = "DELETE FROM MilestoneInfoLocales WHERE MilestoneInfoRowID = @id AND LangKey = @lang";
			db.Execute(sqlQuery, new { id, lang });
		}

		public IEnumerable<MilestoneInfo> GetAllMilestoneInfos()
		{
			using var db = new MySqlConnection(conn);
			return db.Query<MilestoneInfo>("SELECT * FROM MilestoneInfos");
		}

		public MilestoneInfo GetMilestoneInfo(byte id)
		{
			using var db = new MySqlConnection(conn);
			return db.Query<MilestoneInfo>("SELECT * FROM MilestoneInfos WHERE RowID = @id", new { id }).FirstOrDefault();
		}

		public MilestoneInfoLocale GetMilestoneLocale(byte id, LangKey lang)
		{
			using var db = new MySqlConnection(conn);
			return db.QueryFirst<MilestoneInfoLocale>("SELECT * FROM MilestoneInfoLocales WHERE MilestoneInfoRowID = @id AND LangKey = @lang", new { id, lang });
		}

		public IEnumerable<MilestoneInfoLocale> GetMilestoneLocales(byte id)
		{
			using var db = new MySqlConnection(conn);
			return db.Query<MilestoneInfoLocale>("SELECT * FROM MilestoneInfoLocales WHERE MilestoneInfoRowID = @id", new { id });
		}

		public void UpdateMilestone(MilestoneInfo milestoneInfo)
		{
			using var db = new MySqlConnection(conn);
			var sqlQuery = "UPDATE MilestoneInfos SET Icon = @Icon, MaxSpace = @MaxSpace, MilestoneType = @MilestoneType, Game = @Game WHERE RowID = @RowID";
			db.Execute(sqlQuery, milestoneInfo);
		}

		public void UpdateMilestoneLocale(MilestoneInfoLocale infoLocale)
		{
			using var db = new MySqlConnection(conn);
			var sqlQuery = "UPDATE MilestoneInfoLocales Name = @Name, Alias = @Alias, Type = @Type WHERE MilestoneInfoRowID = @MilestoneInfoRowID AND LangKey = @LangKey";
			db.Execute(sqlQuery, infoLocale);
		}
	}
}

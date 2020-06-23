using Dapper;

using MySql.Data.MySqlClient;

using Neiralink.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neiralink
{
	public interface IMilestoneDbClient
	{
		void AddMilestone(MilestoneInfo milestoneInfo);
		void AddMilestoneLocale(MilestoneInfoLocale infoLocale);
		void DeleteMilestone(byte id);
		IEnumerable<MilestoneInfo> GetAllMilestoneInfos();

		MilestoneInfo GetMilestoneInfo(byte id);
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
			var sqlQuery = "UPDATE MilestoneInfoLocales SET MilestoneInfoRowID = @MilestoneInfoRowID, LangKey = @LangKey, Name = @Name, Alias = @Alias, Type = @Type WHERE MilestoneInfoRowID = @MilestoneInfoRowID AND LangKey = @LangKey";
			db.Execute(sqlQuery, infoLocale);
		}
	}
}

using Dapper;

using Neiralink.Models;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Neiralink
{
	public interface IWelcomeDbClient
	{
		#region Random welcomes
		void CreateWelcome(RandomWelcome randomWelcome);
		void DeleteWelcome(int id);
		IEnumerable<RandomWelcome> GetAllWelcomes();
		List<string> GetWelcomesByLocale(CultureInfo culture);
		RandomWelcome GetWelcome(int id);
		void UpdateWelcome(RandomWelcome randomWelcome);
		#endregion
	}

	public class WelcomeDbClient : IWelcomeDbClient
	{
		private readonly string connectionString = null;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="connectionString">Db connection string</param>
		public WelcomeDbClient(string connectionString)
		{
			this.connectionString = connectionString;
		}

		#region Random welcomes
		public void CreateWelcome(RandomWelcome randomWelcome)
		{
			using var db = new MySqlConnection(connectionString);

			var sqlQuery = "INSERT INTO RandomWelcomes (EN, RU, UK) VALUES(@EN, @RU, @UK)";
			db.Execute(sqlQuery, randomWelcome);

			// if we wanna return added welcome
			//var sqlQuery = "INSERT INTO RandomWelcomes (EN, RU, UK) VALUES(@EN, @RU, @UK); SELECT CAST(SCOPE_IDENTITY() as int)";
			//int? welcomeId = db.Query<int>(sqlQuery, randomWelcome).FirstOrDefault();
			//randomWelcome.RowID = welcomeId.Value;
		}

		public void DeleteWelcome(int id)
		{
			using var db = new MySqlConnection(connectionString);
			var sqlQuery = "DELETE FROM RandomWelcomes WHERE RowID = @id";
			db.Execute(sqlQuery, new { id });
		}

		public IEnumerable<RandomWelcome> GetAllWelcomes()
		{
			using var db = new MySqlConnection(connectionString);
			return db.Query<RandomWelcome>("SELECT * FROM RandomWelcomes").ToList();
		}

		public RandomWelcome GetWelcome(int id)
		{
			using var db = new MySqlConnection(connectionString);
			return db.Query<RandomWelcome>("SELECT * FROM RandomWelcomes WHERE RowID = @id", new { id }).FirstOrDefault();
		}

		public List<string> GetWelcomesByLocale(CultureInfo culture)
		{
			using var db = new MySqlConnection(connectionString);
			return db.Query<string>($"SELECT {culture.TwoLetterISOLanguageName} FROM RandomWelcomes").ToList();
		}

		public void UpdateWelcome(RandomWelcome randomWelcome)
		{
			using var db = new MySqlConnection(connectionString);
			var sqlQuery = "UPDATE RandomWelcomes SET EN = @EN, RU = @RU, UK = @UK WHERE RowID = @RowID";
			db.Execute(sqlQuery, randomWelcome);
		}
		#endregion
	}
}

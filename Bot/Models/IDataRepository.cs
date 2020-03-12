﻿using Dapper;

using System.Data.SqlClient;
using System.Linq;

namespace Bot.Models
{
	public interface IDataRepository
	{
		Catalyst GetCatalyst(string name);
		Exotic GetExotic(string name);
	}

	public class DataRepository : IDataRepository
	{
		private readonly string connectionString = null;

		public DataRepository(string conn)
		{
			connectionString = conn;
		}

		public Catalyst GetCatalyst(string name)
		{
			var sql = $"SELECT * FROM Catalysts WHERE WeaponName LIKE N'%{Alias(name)}%';";
			using var db = new SqlConnection(connectionString);
			return db.Query<Catalyst>(sql).FirstOrDefault();

		}

		public Exotic GetExotic(string name)
		{
			var sql = $"SELECT * FROM Exotics WHERE Name LIKE N'%{Alias(name)}%';";
			using var db = new SqlConnection(connectionString);
			return db.Query<Exotic>(sql).FirstOrDefault();
		}

		private static string Alias(string name)
		{
			return name switch
			{
				"дарси" => "Д.А.Р.С.И.",
				"мида" => "MIDA",
				"сурос" => "SUROS",
				"морозники" => "M0р03ники",
				"топотуны" => "Т0п0тунЬI",
				_ => name,
			};
		}
	}
}
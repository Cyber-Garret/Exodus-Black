using Dapper;

using MySql.Data.MySqlClient;

using Neiralink.Models;

using System.Collections.Generic;

namespace Neiralink
{
	public interface ICatalystDbClient
	{
		void AddCatalyst(Catalyst catalyst);
		void DeleteCatalyst(int id, LangKey lang);
		IEnumerable<Catalyst> GetCatalysts();
		Catalyst GetCatalyst(int id, LangKey lang);
		void UpdateCatalyst(Catalyst catalyst);
	}

	public class CatalystDbClient : ICatalystDbClient
	{
		private readonly string conn;
		public CatalystDbClient(string connectionString)
		{
			conn = connectionString;
		}

		public void AddCatalyst(Catalyst catalyst)
		{
			using var db = new MySqlConnection(conn);
			var query = "INSERT INTO Catalysts (Lang, WeaponName, IconUrl, Description, DropLocation, Masterwork, Bonus) VALUES(@Lang, @WeaponName, @IconUrl, @Description, @DropLocation, @Masterwork, @Bonus)";
			db.Execute(query, catalyst);
		}

		public void DeleteCatalyst(int id, LangKey lang)
		{
			using var db = new MySqlConnection(conn);
			db.Execute("DELETE FROM Catalysts WHERE RowID = @id AND Lang = @lang", new { id, lang });
		}

		public Catalyst GetCatalyst(int id, LangKey lang)
		{
			using var db = new MySqlConnection(conn);
			return db.QueryFirst<Catalyst>("SELECT * FROM Catalysts WHERE RowID = @id AND Lang = @lang", new { id, lang });
		}

		public IEnumerable<Catalyst> GetCatalysts()
		{
			using var db = new MySqlConnection(conn);
			return db.Query<Catalyst>("SELECT * FROM Catalysts");
		}

		public void UpdateCatalyst(Catalyst catalyst)
		{
			using var db = new MySqlConnection(conn);
			var query = "UPDATE Catalysts SET WeaponName = @WeaponName, IconUrl = @IconUrl, Description = @Description, DropLocation = @DropLocation, Masterwork = @Masterwork, Bonus = @Bonus WHERE RowID = @RowID AND Lang = @Lang";
			db.Execute(query, catalyst);
		}
	}
}

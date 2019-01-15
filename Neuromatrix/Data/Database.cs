using System;
using System.Linq;

using Neuromatrix.Models.Db;

namespace Neuromatrix.Data
{
    public static class Database
    {
        public static Gear GetGears(string ItemName)
        {
            using (var DbContext = new SqliteDbContext())
            {
                try
                {
                    Gear gear = DbContext.Gears.Where(g => g.Name.IndexOf(ItemName, StringComparison.CurrentCultureIgnoreCase) != -1).FirstOrDefault();
                    if (gear == null)
                        return null;
                    else
                        return gear;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now} Source: {ex.Source}] Message: {ex.Message}");
                    return null;
                }
            }
        }

    }
}

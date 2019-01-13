using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Discord;

using Neuromatrix.Resources;
using Neuromatrix.Resources.Database;

namespace Neuromatrix.Modules
{
    public static class Data
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
                    return gear;
                }
                catch (Exception ex)
                {

                    Console.WriteLine($"[{DateTime.Now} в {ex.Source}] {ex.Message}");
                    return null;
                }
            }
        }
    }
}

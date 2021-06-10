using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Linq;

using Failsafe.Models;

namespace Failsafe.Core.Data
{
    internal static class CommandStatisticData
    {
        private static readonly ConcurrentDictionary<string, CommandStat> CommandStats = new();

        internal static void TryUpdateStat(CommandStat commandStat)
        {
            CommandStats.AddOrUpdate(commandStat.Name, commandStat, (commandNameKey, oldCommandStat) =>
            {
                oldCommandStat.Count++;
                return oldCommandStat;
            });
        }

        internal static void SaveStatByDay()
        {
            var stat = CommandStats.Values;
            DataStorage.SaveObject(stat,
                Path.Combine(DataStorage.GuildsFolder, $"{DateTime.Now.ToString(CultureInfo.InvariantCulture)}.json"));
        }
    }
}

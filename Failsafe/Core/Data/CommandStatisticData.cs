using Failsafe.Models;

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;

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
            if (!CommandStats.Any()) return;

            var now = DateTime.Now;

            var stat = CommandStats.Values.OrderByDescending(x => x.Count);
            CommandStats.Clear();

            DataStorage.SaveObject(stat,
                Path.Combine(DataStorage.StatFolder, $"{now.Day}-{now.Month}-{now.Year}-{now.Hour}-{now.Minute}.json"));
        }
    }
}

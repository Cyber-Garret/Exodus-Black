using System;
using System.IO;
using System.Threading.Tasks;

using Discord.Commands;
using Newtonsoft.Json;

using Neuromatrix.Resources.Settings;
using Neuromatrix.Resources.Datatypes;

namespace Neuromatrix.Core.Moderation
{
    public class Moderation : ModuleBase<SocketCommandContext>
    {
        [Command("reload"), Summary("Перезагрузка значений из файла settings.json во время работы бота.")]
        public async Task Reload()
        {
            //Checks
            if (Context.User.Id != StaticSettings.Owner)
            {
                await Context.Channel.SendMessageAsync(":x: Только капитан корабля и его подчиненные имею доступ к этой команде!");
                return;
            }

            string SettingsLocation = @"Data\Settings.json";
            if (!File.Exists(SettingsLocation))
            {
                await Context.Channel.SendMessageAsync(":x: Не найден файл настроек. Путь по которому было произведено сканирование записан в моей консоли.");
                Console.WriteLine(SettingsLocation);
                return;
            }
            //Execution
            string JSON = "";
            using (var Stream = new FileStream(SettingsLocation, FileMode.Open, FileAccess.Read))
            using (var ReadSettings = new StreamReader(Stream))
            {
                JSON = ReadSettings.ReadToEnd();
            }

            Settings Settings = JsonConvert.DeserializeObject<Settings>(JSON);
            //Save data
            StaticSettings.Token = Settings.token;
            StaticSettings.Owner = Settings.owner;
            StaticSettings.Log = Settings.log;
            StaticSettings.Version = Settings.version;
            StaticSettings.Banned = Settings.banned;

            await Context.Channel.SendMessageAsync(":white_check_mark: Настройки из файла settings.json были успешно загружены в мою память.");
        }
    }
}

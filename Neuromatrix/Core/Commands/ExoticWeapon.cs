using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;

using Neuromatrix.Core.Data;
using Neuromatrix.Resources.Datatypes;

namespace Neuromatrix.Core.Commands
{
    public class ExoticWeapon : ModuleBase<SocketCommandContext>
    {
        private const string summary = "Полная информация о оружии включая данные о катализаторе.";

        private Embed ExoticMessage(string Type, int id)
        {
            EmbedBuilder Embed = new EmbedBuilder();
            Exotic Weapon = Data.Data.GetExotic(Type, id);
            if (Weapon == null)
            {
                Embed.WithColor(219, 66, 55);
                Embed.WithDescription(":x: Данной информации в моей базе данных нет. :frowning:");
                return Embed.Build();
            }

            StringBuilder desc = new StringBuilder();
            desc.Append($"{Weapon.description}\n");
            if (Weapon.catalyst == 1)
            {
                desc.Append("**Катализатор:**\n" +
                "Есть\n" +
                "**Как получить катализатор:**\n" +
                $"{Weapon.catalystlocation}\n" +
                "**Задание катализатора:**\n" +
                $"{Weapon.catalystquest}\n" +
                "**Бонус катализатора:**\n" +
                $"{Weapon.catalystperk}\n");
            }
            else
            {
                desc.Append("**Катализатор:**\n" +
                    "Отсутствует");
            }
            Embed.WithColor(251, 227, 103);
            Embed.WithTitle(Weapon.type + " - " + Weapon.name);
            Embed.WithThumbnailUrl(Weapon.icon);
            Embed.WithDescription(desc.ToString());
            Embed.WithImageUrl(Weapon.image);
            Embed.WithFooter($"Источник: {Weapon.droplocation}");

            return Embed.Build();
        }

        #region Kinetic weapon
        [Command("Милое дело"), Alias("милое", "дело"), Summary(summary)]
        public async Task SweetBussines() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Kinetic", 0));

        [Command("Буря"), Summary(summary)]
        public async Task Sturm() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Kinetic", 1));

        [Command("Крыло бдительности"), Alias("крыло"), Summary(summary)]
        public async Task VigilanceWing() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Kinetic", 2));

        [Command("Крысиный король"), Alias("крыса", "король"), Summary(summary)]
        public async Task RatKing() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Kinetic", 3));

        [Command("Универсальный инструмент MIDA"), Alias("MIDA", "мида"), Summary(summary)]
        public async Task MIDA() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Kinetic", 4));

        [Command("Багрец"), Summary(summary)]
        public async Task Crimson() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Kinetic", 5));

        [Command("Нефритовый кролик"), Alias("Нефритовый", "кролик", "нефрит"), Summary(summary)]
        public async Task JadeRabbit() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Kinetic", 6));

        [Command("Гекльберри"), Summary(summary)]
        public async Task Huckleberry() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Kinetic", 7));

        [Command("Режим SUROS"), Alias("режим", "сурос", "suros"), Summary(summary)]
        public async Task SurosRegime() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Kinetic", 8));

        [Command("Цербер+1"), Summary(summary)]
        public async Task Cerberus1() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Kinetic", 9));

        [Command("Губитель Желаний"), Alias("губитель", "желаний"), Summary(summary)]
        public async Task WishEnder() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Kinetic", 10));

        [Command("Злоумышленник"), Summary(summary)]
        public async Task Malfeasance() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Kinetic", 11));

        [Command("Пиковый туз"), Alias("пиковый", "туз"), Summary(summary)]
        public async Task AceOfSpades() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Kinetic", 12));

        [Command("Компаньон"), Summary(summary)]
        public async Task Chaperone() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Kinetic", 13));
        #endregion

        #region Energy weapon
        [Command("Холодное сердце"), Alias("холодное", "сердце"), Summary(summary)]
        public async Task Coldheart() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Energy", 0));

        [Command("Бойцовый лев"), Alias("бойцовый"), Summary(summary)]
        public async Task FightingLion() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Energy", 1));

        [Command("Солнечный выстрел"), Alias("солнечный"), Summary(summary)]
        public async Task Sunshot() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Energy", 2));

        [Command("Гравитоновое копье"), Alias("гравитоновое"), Summary(summary)]
        public async Task GravitonLance() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Energy", 3));

        [Command("Клятва небожога"), Alias("клятва", "небожог"), Summary(summary)]
        public async Task SkyburnerOath() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Energy", 4));

        [Command("Анализатор рисков"), Alias("анализатор"), Summary(summary)]
        public async Task Riskrunner() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Energy", 5));

        [Command("Жесткий свет"), Alias("жесткий"), Summary(summary)]
        public async Task HardLight() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Energy", 6));

        [Command("Безжалостный"), Summary(summary)]
        public async Task Merciless() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Energy", 7));

        [Command("Северное сияние"), Alias("бореалис", "северное", "сияние"), Summary(summary)]
        public async Task Borealis() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Energy", 8));

        [Command("Линза Прометея"), Alias("линза", "прометей"), Summary(summary)]
        public async Task PrometheusLens() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Energy", 9));

        [Command("Телесто"), Summary(summary)]
        public async Task Telesto() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Energy", 10));

        [Command("Полярное копье"), Alias("полярное"), Summary(summary)]
        public async Task PolarisLance() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Energy", 11));

        [Command("Тройственный гуль"), Alias("гуль"), Summary(summary)]
        public async Task TrinityGhoul() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Energy", 12));

        [Command("Повелитель волков"), Alias("повелитель", "волк"), Summary(summary)]
        public async Task LordOfWolves() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Energy", 13));

        [Command("Le Monarque"), Alias("монарх", "ля монарх"), Summary(summary)]
        public async Task LeMonarque() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Energy", 14));

        [Command("Йотун"), Summary(summary)]
        public async Task Jotunn() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Energy", 15));

        [Command("Волнолом"), Summary(summary)]
        public async Task Wavesplitter() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Energy", 16));
        #endregion

        #region Power weapon
        [Command("Старатель"), Summary(summary)]
        public async Task TheProspector() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Power", 0));

        [Command("Буксировочная пушка"), Alias("буксировка"), Summary(summary)]
        public async Task TractorCannon() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Power", 1));

        [Command("Легенда об Акрии"), Alias("акрия"), Summary(summary)]
        public async Task LegendOfAcrius() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Power", 2));

        [Command("Д.А.Р.С.И."), Alias("дарси"), Summary(summary)]
        public async Task DARCI() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Power", 3));

        [Command("Уордклиффская катушка"), Alias("катушка"), Summary(summary)]
        public async Task TheWardcliffCoil() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Power", 4));

        [Command("Колония"), Summary(summary)]
        public async Task TheColony() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Power", 5));

        [Command("Нулевая мировая линия"), Alias("нулевая", "мировая"), Summary(summary)]
        public async Task WorldlineZero() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Power", 6));

        [Command("Усыпляющий симулянт"), Alias("симулянт"), Summary(summary)]
        public async Task SleeperSimulant() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Power", 7));

        [Command("Шепот червя"), Alias("червь", "шепот"), Summary(summary)]
        public async Task WhisperOfTheWorm() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Power", 8));

        [Command("Тысяча голосов"), Alias("аннигиляторная пушка", "голос", "1000"), Summary(summary)]
        public async Task OneThousandVoices() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Power", 9));

        [Command("Двухвостый лис"), Alias("двухвостый", "лис"), Summary(summary)]
        public async Task TwoTailedFox() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Power", 10));

        [Command("Черный коготь"), Alias("коготь"), Summary(summary)]
        public async Task BlackTalon() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Power", 11));

        [Command("Губитель королев"), Alias("губитель"), Summary(summary)]
        public async Task TheQueenbreaker() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Power", 12));

        [Command("Владыка грома"), Alias("владыка"), Summary(summary)]
        public async Task Thunderlord() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Power", 13));

        [Command("Анархия"), Summary(summary)]
        public async Task Anarchy() => await Context.Channel.SendMessageAsync("", false, ExoticMessage("Power", 14));
        #endregion
    }
}
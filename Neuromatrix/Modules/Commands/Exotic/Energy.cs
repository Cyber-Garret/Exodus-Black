using System.Threading.Tasks;

using Discord.Commands;

namespace Neuromatrix.Modules.Commands.Exotic
{
    public class Energy : ModuleBase<SocketCommandContext>
    {
        [Command("сердце"), Alias("холодное"), Summary("Информация о лучевой винтовке Холодное сердце.")]
        public async Task Coldheart() => await Context.Channel.SendMessageAsync("", false, Data.ExoticMessage("Energy", 0));

        [Command("бойцовый"), Alias("бойцовый"), Summary("Информация о гранатомете Бойцовый лев.")]
        public async Task FightingLion() => await Context.Channel.SendMessageAsync("", false, Data.ExoticMessage("Energy", 1));

        [Command("солнечный"), Summary("Информация о револьвере Солнечный выстрел.")]
        public async Task Sunshot() => await Context.Channel.SendMessageAsync("", false, Data.ExoticMessage("Energy", 2));

        [Command("гравитоновое"), Summary("Информация о импульсной винтовке Гравитоновое копье.")]
        public async Task GravitonLance() => await Context.Channel.SendMessageAsync("", false, Data.ExoticMessage("Energy", 3));

        [Command("небожог"), Alias("клятва"), Summary("Информация о винтовке разведчика Клятва небожога.")]
        public async Task SkyburnerOath() => await Context.Channel.SendMessageAsync("", false, Data.ExoticMessage("Energy", 4));

        [Command("анализатор"), Summary("Информация о пистолете-пулемете Анализатор рисков.")]
        public async Task Riskrunner() => await Context.Channel.SendMessageAsync("", false, Data.ExoticMessage("Energy", 5));

        [Command("свет"), Alias("жесткий"), Summary("Информация о автомате Жесткий свет.")]
        public async Task HardLight() => await Context.Channel.SendMessageAsync("", false, Data.ExoticMessage("Energy", 6));

        [Command("безжалостный"), Summary("Информация о плазменной винтовке Безжалостный.")]
        public async Task Merciless() => await Context.Channel.SendMessageAsync("", false, Data.ExoticMessage("Energy", 7));

        [Command("бореалис"), Alias("сияние"), Summary("Информация о снайперской винтовке Северное сияние.")]
        public async Task Borealis() => await Context.Channel.SendMessageAsync("", false, Data.ExoticMessage("Energy", 8));

        [Command("линза"), Alias("прометей"), Summary("Информация о лучевой винтовке Линза Прометея.")]
        public async Task PrometheusLens() => await Context.Channel.SendMessageAsync("", false, Data.ExoticMessage("Energy", 9));

        [Command("телесто"), Summary("Информация о плазменной винтовке Телесто.")]
        public async Task Telesto() => await Context.Channel.SendMessageAsync("", false, Data.ExoticMessage("Energy", 10));

        [Command("полярное"), Summary("Информация о винтовке разведчика Полярное копье.")]
        public async Task PolarisLance() => await Context.Channel.SendMessageAsync("", false, Data.ExoticMessage("Energy", 11));

        [Command("гуль"), Summary("Информация о луке Тройственный гуль.")]
        public async Task TrinityGhoul() => await Context.Channel.SendMessageAsync("", false, Data.ExoticMessage("Energy", 12));

        [Command("волк"), Alias("повелитель"), Summary("Информация о дробовике Повелитель волков.")]
        public async Task LordOfWolves() => await Context.Channel.SendMessageAsync("", false, Data.ExoticMessage("Energy", 13));

        [Command("монарх"), Alias("ля монарх"), Summary("Информация о луке Le Monarque.")]
        public async Task LeMonarque() => await Context.Channel.SendMessageAsync("", false, Data.ExoticMessage("Energy", 14));

        [Command("йотун"), Summary("Информация о плазменной винтовке Йотун.")]
        public async Task Jotunn() => await Context.Channel.SendMessageAsync("", false, Data.ExoticMessage("Energy", 15));

        [Command("волнолом"), Summary("Информация о лучевой винтовке Волнолом.")]
        public async Task Wavesplitter() => await Context.Channel.SendMessageAsync("", false, Data.ExoticMessage("Energy", 16));
    }
}

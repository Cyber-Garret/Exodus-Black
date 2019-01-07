using System.Threading.Tasks;

using Discord.Commands;

namespace Neuromatrix.Core.Commands.Exotic
{
    public class Power : ModuleBase<SocketCommandContext>
    {
        [Command("старатель"), Summary("Информация о гранатомете Старатель.")]
        public async Task TheProspector() => await Context.Channel.SendMessageAsync("", false, Data.Data.ExoticMessage("Power", 0));

        [Command("буксировка"), Summary("Информация о дробовике Буксировочная пушка.")]
        public async Task TractorCannon() => await Context.Channel.SendMessageAsync("", false, Data.Data.ExoticMessage("Power", 1));

        [Command("акрия"), Summary("Информация о дробовике Легенда об Акрии.")]
        public async Task LegendOfAcrius() => await Context.Channel.SendMessageAsync("", false, Data.Data.ExoticMessage("Power", 2));

        [Command("дарси"), Summary("Информация о снайперской винтовке Д.А.Р.С.И..")]
        public async Task DARCI() => await Context.Channel.SendMessageAsync("", false, Data.Data.ExoticMessage("Power", 3));

        [Command("катушка"), Summary("Информация о ракетной установке Уорклиффская катушка.")]
        public async Task TheWardcliffCoil() => await Context.Channel.SendMessageAsync("", false, Data.Data.ExoticMessage("Power", 4));

        [Command("колония"), Summary("Информация о гранатомете Колония.")]
        public async Task TheColony() => await Context.Channel.SendMessageAsync("", false, Data.Data.ExoticMessage("Power", 5));

        [Command("нулевая"), Alias("мировая"), Summary("Информация о мече Нулевая мировая линия.")]
        public async Task WorldlineZero() => await Context.Channel.SendMessageAsync("", false, Data.Data.ExoticMessage("Power", 6));

        [Command("симулянт"), Summary("Информация о линейно-плазменной винтовке Усыпляющий симулянт.")]
        public async Task SleeperSimulant() => await Context.Channel.SendMessageAsync("", false, Data.Data.ExoticMessage("Power", 7));

        [Command("червь"), Alias("шепот"), Summary("Информация о снайперской винтовке Шепот червя.")]
        public async Task WhisperOfTheWorm() => await Context.Channel.SendMessageAsync("", false, Data.Data.ExoticMessage("Power", 8));

        [Command("голос"), Alias("1000", "аннигиляторная пушка"), Summary("Информация о плазменной винтовке Тысяча голосов.")]
        public async Task OneThousandVoices() => await Context.Channel.SendMessageAsync("", false, Data.Data.ExoticMessage("Power", 9));

        [Command("лис"), Alias("двухвостый"), Summary("Информация о ракетной установке Двухвостый лис.")]
        public async Task TwoTailedFox() => await Context.Channel.SendMessageAsync("", false, Data.Data.ExoticMessage("Power", 10));

        [Command("коготь"), Summary("Информация о мече Черный коготь.")]
        public async Task BlackTalon() => await Context.Channel.SendMessageAsync("", false, Data.Data.ExoticMessage("Power", 11));

        [Command("губитель королев"), Summary("Информация о линейно-плазменной винтовке Губитель королев.")]
        public async Task TheQueenbreaker() => await Context.Channel.SendMessageAsync("", false, Data.Data.ExoticMessage("Power", 12));

        [Command("владыка"), Summary("Информация о пулемете Владыка грома.")]
        public async Task Thunderlord() => await Context.Channel.SendMessageAsync("", false, Data.Data.ExoticMessage("Power", 13));

        [Command("анархия"), Summary("Информация о гранатомете Анархия.")]
        public async Task Anarchy() => await Context.Channel.SendMessageAsync("", false, Data.Data.ExoticMessage("Power", 14));
    }
}

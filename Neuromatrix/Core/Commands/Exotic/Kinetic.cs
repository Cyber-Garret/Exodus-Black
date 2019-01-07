using System.Threading.Tasks;

using Discord.Commands;

namespace Neuromatrix.Core.Commands.Exotic
{
    public class Kinetic : ModuleBase<SocketCommandContext>
    {
        [Command("дело"), Alias("милое"), Summary("Информация о автомате Милое Дело.")]
        public async Task SweetBussines() => await Context.Channel.SendMessageAsync("", false, Data.Data.ExoticMessage("Kinetic", 0));

        [Command("буря"), Summary("Информация о револьвере Буря.")]
        public async Task Sturm() => await Context.Channel.SendMessageAsync("", false, Data.Data.ExoticMessage("Kinetic", 1));

        [Command("крыло"), Summary("Информация о импульсной винтовке Крыло бдительности.")]
        public async Task VigilanceWing() => await Context.Channel.SendMessageAsync("", false, Data.Data.ExoticMessage("Kinetic", 2));

        [Command("король"), Alias("крыса"), Summary("Информация о пистолете Крысиный король.")]
        public async Task RatKing() => await Context.Channel.SendMessageAsync("", false, Data.Data.ExoticMessage("Kinetic", 3));

        [Command("мида"), Alias("MIDA"), Summary("Информация о винтовке разведчика MIDA.")]
        public async Task MIDA() => await Context.Channel.SendMessageAsync("", false, Data.Data.ExoticMessage("Kinetic", 4));

        [Command("багрец"), Summary("Информация о револьвере Багрец.")]
        public async Task Crimson() => await Context.Channel.SendMessageAsync("", false, Data.Data.ExoticMessage("Kinetic", 5));

        [Command("нефрит"), Alias("кролик"), Summary("Информация о винтовке разведчика Нефритовый кролик.")]
        public async Task JadeRabbit() => await Context.Channel.SendMessageAsync("", false, Data.Data.ExoticMessage("Kinetic", 6));

        [Command("гекльберри"), Summary("Информация о пистолете-пулемете Гекльберри.")]
        public async Task Huckleberry() => await Context.Channel.SendMessageAsync("", false, Data.Data.ExoticMessage("Kinetic", 7));

        [Command("сурос"), Alias("SUROS"), Summary("Информация о автомате SUROS.")]
        public async Task SurosRegime() => await Context.Channel.SendMessageAsync("", false, Data.Data.ExoticMessage("Kinetic", 8));

        [Command("цербер"), Summary("Информация о автомате Цербер+1.")]
        public async Task Cerberus1() => await Context.Channel.SendMessageAsync("", false, Data.Data.ExoticMessage("Kinetic", 9));

        [Command("губитель"), Summary("Информация о луке Губитель Желаний.")]
        public async Task WishEnder() => await Context.Channel.SendMessageAsync("", false, Data.Data.ExoticMessage("Kinetic", 10));

        [Command("злоумышленник"), Summary("Информация о револьвере Злоумышленник.")]
        public async Task Malfeasance() => await Context.Channel.SendMessageAsync("", false, Data.Data.ExoticMessage("Kinetic", 11));

        [Command("туз"), Alias("пиковый"), Summary("Информация о револьвере Пиковый Туз.")]
        public async Task AceOfSpades() => await Context.Channel.SendMessageAsync("", false, Data.Data.ExoticMessage("Kinetic", 12));

        [Command("компаньон"), Summary("Информация о дробовике Компаньон.")]
        public async Task Chaperone() => await Context.Channel.SendMessageAsync("", false, Data.Data.ExoticMessage("Kinetic", 13));
    }
}

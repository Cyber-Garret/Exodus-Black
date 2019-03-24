using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Neuromatrix.Data;
using Neuromatrix.Models.Db;
using Neuromatrix.Preconditions;

namespace Neuromatrix.Modules.Commands
{
    public class ExoticModule : BotModuleBase
    {

        [Command("инфо")]
        [Cooldown(10)]
        public async Task GearInfo([Remainder]string Input = null)
        {
            #region Checks
            //Проверяем ввел ли пользователь параметры для поиска.
            if (Input == null)
            {
                await Context.Channel.SendMessageAsync(":x: Пожалуйста, введите полное или частичное название экзотического снаряжения.");
                return;
            }

            ///Функция синонимов.
            string Alias()
            {
                if (Input == "дарси")
                    return "Д.А.Р.С.И.";
                if (Input == "мида")
                    return "MIDA";
                if (Input == "сурос")
                    return "SUROS";
                return Input;
            }
            //Запрашиваем информацию из локальной бд.
            Gear gear = Database.GetGears(Alias());
            //Если бд вернула null сообщаем пользователю что ничего не нашли.
            if (gear == null)
            {
                await Context.Channel.SendMessageAsync(":x: Этой информации в моей базе данных нет. :frowning:");
                return;
            }
            #endregion

            EmbedBuilder embed = new EmbedBuilder();

            embed.WithColor(Color.Gold);
            //Тип снаряжение и его имя.
            embed.WithTitle(gear.Type + " - " + gear.Name);
            //Иконка снаряжения.
            embed.WithThumbnailUrl(gear.IconUrl);
            //Краткая история снаряжения.
            embed.WithDescription(gear.Description);
            //Если в бд отмечено что оружие имеет катализатор добавляем несколько полей.
            if (gear.Catalyst == 1)
            {
                embed.AddField("Катализатор:", "Есть");
                embed.AddField("Как получить катализатор:", gear.WhereCatalystDrop);
                embed.AddField("Задание катализатора:", gear.CatalystQuest);
                embed.AddField("Бонус катализатора:", gear.CatalystBonus);
            }
            else
            {
                embed.AddField("Катализатор:", "Отсутствует");
            }
            //Экзотическое свойство.
            embed.AddField(gear.PerkName, gear.PerkDescription,true);
            //Второе уникальное или не очень свойство.
            embed.AddField(gear.SecondPerkName, gear.SecondPerkDescription,true);
            //Информация о том как получить снаряжение.
            embed.AddField("Как получить:", gear.DropLocation,true);
            //Скриншот снаряжения.
            embed.WithImageUrl(gear.ImageUrl);

            var app = await Context.Client.GetApplicationInfoAsync();
            embed.WithFooter($"Если нашли какие либо неточности, сообщите моему создателю: {app.Owner.Username}", app.Owner.GetAvatarUrl());


            await Context.Channel.SendMessageAsync($"Итак, {Context.User.Username}, вот что мне известно про это снаряжение.", false, embed.Build());
        }
    }
}

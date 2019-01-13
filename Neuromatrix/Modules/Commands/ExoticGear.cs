using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;

using Neuromatrix.Resources.Database;
using Neuromatrix.Modules;
using Neuromatrix.Resources;

namespace Neuromatrix.Modules.Commands
{
    public class ExoticGear : ModuleBase<SocketCommandContext>
    {
        [Command("инфо")]
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
            Gear gear = Data.GetGears(Alias());
            //Если бд вернула null сообщаем пользователю что ничего не нашли.
            if (gear == null)
            {
                await Context.Channel.SendMessageAsync(":x: Этой информации в моей базе данных нет. :frowning:");
                return;
            }
            #endregion

            EmbedBuilder embed = new EmbedBuilder();

            embed.WithColor(206, 174, 51);
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
            embed.AddInlineField(gear.PerkName, gear.PerkDescription);
            //Второе уникальное или не очень свойство.
            embed.AddInlineField(gear.SecondPerkName, gear.SecondPerkDescription);
            //Информация о том как получить снаряжение.
            embed.AddInlineField("Как получить:", gear.DropLocation);
            //Скриншот снаряжения.
            embed.WithImageUrl(gear.ImageUrl);
            var owner = Context.Guild.GetUser(StaticSettings.Owner);
            embed.WithFooter($"Если нашли какие либо неточности, сообщите моему создателю: {owner.Username}", owner.GetAvatarUrl());


            await Context.Channel.SendMessageAsync($"Итак, {Context.User.Username}, вот что мне известно про это снаряжение.", false, embed.Build());
        }
    }
}

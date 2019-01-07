using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Linq;
using System.Collections.Generic;

using Discord;

using Neuromatrix.Resources.Datatypes;

namespace Neuromatrix.Core.Data
{
    public static class Data
    {
        public static Exotic GetExotic(string Type, int Xml_Item_id)
        {
            string ExotType()
            {
                string ExoticLocation = null;

                if (Type == "Kinetic")
                    ExoticLocation = @"Data\Exotics\Kinetic.xml";
                if (Type == "Energy")
                    ExoticLocation = @"Data\Exotics\Energy.xml";
                if (Type == "Power")
                    ExoticLocation = @"Data\Exotics\Power.xml";
                return ExoticLocation;
            }

            if (!File.Exists(ExotType()))
                return null;

            //Файл найден!!!
            FileStream Stream = new FileStream(ExotType(), FileMode.Open, FileAccess.Read);
            XmlDocument Doc = new XmlDocument();
            Doc.Load(Stream);
            Stream.Dispose();

            List<Exotic> Exotics = new List<Exotic>();
            foreach (XmlNode Node in Doc.DocumentElement)
                Exotics.Add(new Exotic
                {
                    id = Convert.ToInt32(Node.ChildNodes[0].InnerText),
                    name = Node.ChildNodes[1].InnerText,
                    icon = Node.ChildNodes[2].InnerText,
                    image = Node.ChildNodes[3].InnerText,
                    type = Node.ChildNodes[4].InnerText,
                    description = Node.ChildNodes[5].InnerText,
                    perk = Node.ChildNodes[6].InnerText,
                    droplocation = Node.ChildNodes[7].InnerText,
                    catalyst = Convert.ToInt32(Node.ChildNodes[8].InnerText),
                    catalystlocation = Node.ChildNodes[9].InnerText,
                    catalystquest = Node.ChildNodes[10].InnerText,
                    catalystperk = Node.ChildNodes[11].InnerText
                });

            return Exotics.First(x => x.id == Xml_Item_id);
        }

        public static Embed ExoticMessage(string Type, int id)
        {
            EmbedBuilder Embed = new EmbedBuilder();
            Exotic Weapon = GetExotic(Type, id);
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
            Embed.AddInlineField("Особенность:", Weapon.perk);
            Embed.WithDescription(desc.ToString());
            Embed.WithImageUrl(Weapon.image);
            Embed.WithFooter($"Как получить: {Weapon.droplocation}");

            return Embed.Build();
        }
    }
}

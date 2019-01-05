using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
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
                    droplocation = Node.ChildNodes[6].InnerText,
                    catalyst = Convert.ToInt32(Node.ChildNodes[7].InnerText),
                    catalystlocation = Node.ChildNodes[8].InnerText,
                    catalystquest = Node.ChildNodes[9].InnerText,
                    catalystperk = Node.ChildNodes[10].InnerText
                });

            return Exotics.First(x => x.id == Xml_Item_id);
        }
    }
}

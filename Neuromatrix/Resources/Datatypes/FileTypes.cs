using System.Collections.Generic;

namespace Neuromatrix.Resources.Datatypes
{
    public class Settings
    {
        public string token { get; set; }
        public ulong owner { get; set; }
        public List<ulong> log { get; set; }
        public string version { get; set; }
        public List<ulong> banned { get; set; }
    }

    public class Exotic
    {
        public int id { get; set; }
        public string name { get; set; }
        public string icon { get; set; }
        public string image { get; set; }
        public string type { get; set; }
        public string description { get; set; }
        public string perk { get; set; }
        public string droplocation { get; set; }
        public int catalyst { get; set; }
        public string catalystlocation { get; set; }
        public string catalystquest { get; set; }
        public string catalystperk { get; set; }

    }
}

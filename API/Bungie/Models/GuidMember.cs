using System;
using System.Collections.Generic;
using System.Text;

namespace API.Bungie.Models
{
    public class GuidMember
    {
        public string ProfileId { get; set; }
        public string MemberName { get; set; }
        public DateTime LastOnlineDate { get; set; }
    }
}

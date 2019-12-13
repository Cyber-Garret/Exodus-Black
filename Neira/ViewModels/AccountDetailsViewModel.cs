using Destiny2;
using Neira.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Neira.ViewModels
{
    public class AccountDetailsViewModel
    {
        public AccountDetailsViewModel(BungieMembershipType type, long id)
        {
            Type = type;
            Id = id;
        }

        public BungieMembershipType Type { get; }
        public long Id { get; }
        public IList<Character> Characters { get; set; } = new List<Character>();
        public IEnumerable<WeaponMod> WeaponMods { get; set; }
    }
}

using Neira.Helpers;
using Neira.Models;

using System.Collections.Generic;

namespace Neira.ViewModels
{
    public class AccountsViewModel
    {
        public IEnumerable<Account> Accounts { get; set; }

        public string GetAccountName(Account account)
        {
            return Utilities.GetDescription(account.Type);
        }
    }
}

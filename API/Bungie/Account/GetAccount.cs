using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using API.Bungie.Models;
using API.Bungie.Models.Account;
using Newtonsoft.Json;

namespace API.Bungie.Account
{
    public class GetAccount
    {
        public GetAccount()
        {
            RootRequest.LoadWeb();
        }

        public async Task<AccountResult> GetAccountAsync(string profileId, string сharacterId, int count, int page)
        {
            var data = await RootRequest.Web.GetStringAsync($"Platform/Destiny2/{(int)BungieMembershipType.TigerBlizzard}/Account/{profileId}/Character/{сharacterId}/Stats/Activities/?count={count}&page={page}");
            return AccountResult.FromJson(data);
        }
    }
}

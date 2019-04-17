using API.Bungie.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using API.Bungie.Models;
namespace API.Bungie.GroupV2
{
    public class GetAllMembers
    {
        public GetAllMembers()
        {
            RootRequest.LoadWeb();
        }

        public async Task<GroupV2Result> GetGuildMembers(int GroupId)
        {
            var rawData = await RootRequest.Web.GetStringAsync($"Platform/GroupV2/{GroupId}/Members/");
            return JsonConvert.DeserializeObject<GroupV2Result>(rawData);
        }

        public async Task<dynamic> GetXur()
        {
            var membershipType = "4";
            var destinyMembershipId = "4611686018483656383";
            var characterId = "2305843009404218418";
            var vendorHash = "2190858386";
            try
            {
                var rawData = await RootRequest.Web.GetStringAsync($"/Destiny2/{membershipType}/Profile/{destinyMembershipId}/Character/{characterId}/Vendors/{vendorHash}/");
                return JsonConvert.DeserializeObject<dynamic>(rawData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Task.CompletedTask;
            }
        }
    }
}

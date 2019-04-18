using API.Bungie.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

using API.Bungie.Models.GroupV2;

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
            return GroupV2Result.FromJson(rawData);
        }
    }
}

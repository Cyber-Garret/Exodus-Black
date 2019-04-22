using API.Bungie.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

using API.Bungie.Models.Account;
using API.Bungie.Models.GroupV2;
using API.Bungie.Models.GroupV2.GetGroup;

namespace API.Bungie.GroupV2
{
    public class GetAllMembers
    {
        public GetAllMembers()
        {
            RootRequest.LoadWeb();
        }

        internal async Task<GroupV2Result> GetGuildMembers(int GroupId)
        {
            var rawData = await RootRequest.Web.GetStringAsync($"Platform/GroupV2/{GroupId}/Members/");
            return GroupV2Result.FromJson(rawData);
        }

        internal async Task<GetGroupResult> GetGroup(int GroupId)
        {
            var rawData = await RootRequest.Web.GetStringAsync($"Platform/GroupV2/{GroupId}/");
            return GetGroupResult.FromJson(rawData);
        }
    }
}

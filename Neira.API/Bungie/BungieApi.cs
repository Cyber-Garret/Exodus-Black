using System.Threading.Tasks;

using RestSharp;
using Newtonsoft.Json;

using Neira.API.Bungie.Models.Results.GroupV2.GetMembersOfGroup;
using Neira.API.Bungie.Models.Results.GroupV2.GetGroup;
using Neira.API.Bungie.Models.Results.Destiny2.GetProfile;
using System;
using Neira.API.Bungie.Models;

namespace Neira.API.Bungie
{
	public class BungieApi
	{
		const string BaseUrl = "https://www.bungie.net/Platform";
		const string ApiKeyName = "X-API-Key";
		const string ApiKey = "6fdc49f28e454eb380e02931b5ed61d4";

		public GetMembersOfGroup GetMembersOfGroupResponse(long groupId)
		{
			var client = new RestClient(BaseUrl + $"/GroupV2/{groupId}/Members/");
			var request = new RestRequest(Method.GET);
			request.AddHeader(ApiKeyName, ApiKey);
			var response = client.Execute(request);

			return JsonConvert.DeserializeObject<GetMembersOfGroup>(response.Content);
		}

		public GetGroup GetGroupResult(long groupId)
		{
			var client = new RestClient(BaseUrl + $"/GroupV2/{groupId}/");
			var request = new RestRequest(Method.GET);
			request.AddHeader(ApiKeyName, ApiKey);
			var response = client.Execute(request);

			return JsonConvert.DeserializeObject<GetGroup>(response.Content);
		}

		public GetProfile GetProfileResult(string destinyMembershipId, BungieMembershipType membershipType, DestinyComponentType components)
		{
			var client = new RestClient(BaseUrl + $"/Destiny2/{membershipType}/Profile/{destinyMembershipId}/?components={(int)components}");
			var request = new RestRequest(Method.GET);
			request.AddHeader(ApiKeyName, ApiKey);
			var response = client.Execute(request);

			return JsonConvert.DeserializeObject<GetProfile>(response.Content);
		}
	}
}

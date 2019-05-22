using API.Bungie.Models.Results.GroupV2;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace API.Bungie
{
	public class BungieApi
	{
		const string BaseUrl = "https://www.bungie.net/Platform";
		const string ApiKey = "6fdc49f28e454eb380e02931b5ed61d4";

		public async Task<GetMembersOfGroup> GetMembersOfGroupResponse(long groupId)
		{
			var client = new RestClient(BaseUrl + $"/GroupV2/{groupId}/Members/");
			var request = new RestRequest(Method.GET);
			request.AddHeader("X-API-Key", "6fdc49f28e454eb380e02931b5ed61d4");
			var response = client.Execute(request);

			return JsonConvert.DeserializeObject<GetMembersOfGroup>(response.Content);
		}
	}
}

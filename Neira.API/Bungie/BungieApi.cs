
using Neira.API.Bungie.Models.Results.Destiny2;
using Neira.API.Bungie.Models.Results.Destiny2.GetProfile;
using Neira.API.Bungie.Models.Results.GroupV2.GetGroup;
using Neira.API.Bungie.Models.Results.GroupV2.GetMembersOfGroup;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.IO;

namespace Neira.API.Bungie
{
	public class BungieApi
	{
		private readonly Config config;

		public BungieApi()
		{
			config = new Config
			{
				BungieConfig = new BungieConfig
				{
					BaseUrl = "https://www.bungie.net/Platform",
					KeyName = "X-API-Key",
					ApiKey = "6fdc49f28e454eb380e02931b5ed61d4"
				}
			};
		}

		public GetMembersOfGroup GetMembersOfGroupResponse(long groupId)
		{
			var client = new RestClient(config.BungieConfig.BaseUrl + $"/GroupV2/{groupId}/Members/");
			var request = new RestRequest(Method.GET);
			request.AddHeader(config.BungieConfig.KeyName, config.BungieConfig.ApiKey);
			var response = client.Execute(request);

			return JsonConvert.DeserializeObject<GetMembersOfGroup>(response.Content);
		}

		public GetGroup GetGroupResult(long groupId)
		{
			var client = new RestClient(config.BungieConfig.BaseUrl + $"/GroupV2/{groupId}/");
			var request = new RestRequest(Method.GET);
			request.AddHeader(config.BungieConfig.KeyName, config.BungieConfig.ApiKey);
			var response = client.Execute(request);

			return JsonConvert.DeserializeObject<GetGroup>(response.Content);
		}

		public GetProfile GetProfileResult(string destinyMembershipId, int membershipType, DestinyComponentType components)
		{
			try
			{
				var client = new RestClient(config.BungieConfig.BaseUrl + $"/Destiny2/{membershipType}/Profile/{destinyMembershipId}/?components={(int)components}");
				var request = new RestRequest(Method.GET);
				request.AddHeader(config.BungieConfig.KeyName, config.BungieConfig.ApiKey);

				var response = client.Execute(request);

				return JsonConvert.DeserializeObject<GetProfile>(response.Content);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"destinyMembershipId [{destinyMembershipId}] membershipType [{membershipType}] components [{components}]");
				Console.WriteLine(ex);
				return null;
			}

		}

		public GetActivityHistory LoadCharacterActivityHistory(int membershipType, string destinyMembershipId, long characterId)
		{
			var client = new RestClient(config.BungieConfig.BaseUrl + $"/Destiny2/{membershipType}/Account/{destinyMembershipId}/Character/{characterId}/Stats/Activities/");
			var request = new RestRequest(Method.GET);
			request.AddHeader(config.BungieConfig.KeyName, config.BungieConfig.ApiKey);
			var response = client.Execute(request);

			return GetActivityHistory.FromJson(response.Content);
		}
	}
}

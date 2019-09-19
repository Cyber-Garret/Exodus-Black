using System.Threading.Tasks;

using RestSharp;
using Newtonsoft.Json;

using Neira.API.Bungie.Models.Results.GroupV2.GetMembersOfGroup;
using Neira.API.Bungie.Models.Results.GroupV2.GetGroup;
using Neira.API.Bungie.Models.Results.Destiny2.GetProfile;
using System;
using Neira.API.Bungie.Models;
using System.IO;

namespace Neira.API.Bungie
{
	public class BungieApi
	{
		private readonly Config config;

		public BungieApi()
		{
			var json = File.ReadAllText("config.json");
			config = JsonConvert.DeserializeObject<Config>(json);
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
	}
}

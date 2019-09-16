using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Neira.API
{
	internal class Config
	{
		[JsonProperty("bungieconfig")]
		public BungieConfig BungieConfig { get; set; }
	}
	internal class BungieConfig
	{
		[JsonProperty("baseurl")]
		internal string BaseUrl { get; set; }

		[JsonProperty("keyname")]
		internal string KeyName { get; set; }

		[JsonProperty("apikey")]
		internal string ApiKey { get; set; }
	}
}

namespace Tangle.Entities.Models
{
	/// <summary>
	/// Model for config values from json
	/// </summary>
	public class BungieSettings
	{
		/// <summary>
		/// Bungie API Key
		/// </summary>
		public string ApiKey { get; set; }
		/// <summary>
		/// Bungie OAuth client_id
		/// </summary>
		public string ClientId { get; set; }
		/// <summary>
		/// Bungie OAuth client_secret
		/// </summary>
		public string ClientSecret { get; set; }
		/// <summary>
		/// Simple name for identity cookie
		/// </summary>
		public string LoginCookieName { get; set; }
		/// <summary>
		/// Bungie base url
		/// </summary>
		public string BaseUrl { get; set; }
		/// <summary>
		/// Bungie OAuth Authorization URL
		/// </summary>
		public string AuthorizationEndpoint { get; set; }
		/// <summary>
		/// Bungie Token endpoint URL
		/// </summary>
		public string TokenEndpoint { get; set; }
	}
}

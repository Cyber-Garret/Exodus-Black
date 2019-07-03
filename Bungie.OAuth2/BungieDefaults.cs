namespace Bungie.OAuth2
{
	public static class BungieDefaults
	{
		public const string AuthenticationScheme = "Bungie";
		public const string DisplayName = "Bungie";

		public static readonly string AuthorizationEndpoint = "https://www.bungie.net/ru/OAuth/Authorize";
		public static readonly string TokenEndpoint = "https://www.bungie.net/Platform/App/OAuth/token/";
		public static readonly string UserInformationEndpoint = "https://www.bungie.net/Platform/User/GetBungieNetUserById/";
	}
}

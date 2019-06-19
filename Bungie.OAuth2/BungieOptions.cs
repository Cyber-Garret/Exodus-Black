using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Bungie.OAuth2
{
	public class BungieOptions : OAuthOptions
	{
		public BungieOptions()
		{
			CallbackPath = new PathString("/signin-bungie");
			AuthorizationEndpoint = BungieDefaults.AuthorizationEndpoint;
			TokenEndpoint = BungieDefaults.TokenEndpoint;
			UserInformationEndpoint = BungieDefaults.UserInformationEndpoint;
			Scope.Add("code");

			ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id", ClaimValueTypes.UInteger64);
			ClaimActions.MapJsonKey(ClaimTypes.Name, "username", ClaimValueTypes.String);
			ClaimActions.MapJsonKey(ClaimTypes.Email, "email", ClaimValueTypes.Email);
			ClaimActions.MapJsonKey("urn:discord:discriminator", "discriminator", ClaimValueTypes.UInteger32);
			ClaimActions.MapJsonKey("urn:discord:avatar", "avatar", ClaimValueTypes.String);
			ClaimActions.MapJsonKey("urn:discord:verified", "verified", ClaimValueTypes.Boolean);
		}

		/// <summary> Gets or sets the Bungie-assigned appId. </summary>
		public string AppId { get => ClientId; set => ClientId = value; }
		/// <summary> Gets or sets the Bungie-assigned app secret. </summary>
		public string AppSecret { get => ClientSecret; set => ClientSecret = value; }
	}
}

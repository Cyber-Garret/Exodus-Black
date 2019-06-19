using System;
using Microsoft.AspNetCore.Authentication;
using Bungie.OAuth2;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class BungieExtensions
	{
		public static AuthenticationBuilder AddBungie(this AuthenticationBuilder builder)
		  => builder.AddBungie(BungieDefaults.AuthenticationScheme, _ => { });

		public static AuthenticationBuilder AddBungie(this AuthenticationBuilder builder, Action<BungieOptions> configureOptions)
			=> builder.AddBungie(BungieDefaults.AuthenticationScheme, configureOptions);

		public static AuthenticationBuilder AddBungie(this AuthenticationBuilder builder, string authenticationScheme, Action<BungieOptions> configureOptions)
			=> builder.AddBungie(authenticationScheme, BungieDefaults.DisplayName, configureOptions);

		public static AuthenticationBuilder AddBungie(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<BungieOptions> configureOptions)
			=> builder.AddOAuth<BungieOptions, BungieHandler>(authenticationScheme, displayName, configureOptions);
	}
}

using Microsoft.Extensions.Localization;

using System.Reflection;

using Web.Resources;

namespace Web.Services
{
	public class SharedResourcesService
	{
		private readonly IStringLocalizer localizer;
		public SharedResourcesService(IStringLocalizerFactory factory)
		{
			var assemblyName = new AssemblyName(typeof(SharedResources).GetTypeInfo().Assembly.FullName);
			localizer = factory.Create(nameof(SharedResources), assemblyName.Name);
		}

		public string Get(string key)
		{
			return localizer[key];
		}

		public string BotInvite()
		{
			return "https://discord.com/oauth2/authorize?client_id=521693707238506498&scope=bot&permissions=269479104";
		}

		public string BlackExodusInvite()
		{
			return "https://discord.com/invite/WcuNPM9";
		}

		public string DocsUrl()
		{
			return "https://docs.neira.su/";
		}

		public string YandexMoneyUrl()
		{
			return "https://money.yandex.ru/to/410019748161790";
		}
	}
}

using Microsoft.Extensions.Localization;

using System.Reflection;

using WebSite.Resources;

namespace WebSite.Services
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
	}
}

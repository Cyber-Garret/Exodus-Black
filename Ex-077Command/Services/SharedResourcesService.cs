using Microsoft.Extensions.Localization;

using System.Reflection;

using Ex077.Resources;

namespace Ex077.Services
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

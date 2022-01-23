using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection ConfigureAutoMapper(this IServiceCollection services) =>
		services.AddAutoMapper(Assembly.GetEntryAssembly());
}

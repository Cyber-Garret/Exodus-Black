using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System.Collections.Generic;
using System.Globalization;

namespace Web
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddLocalization(options => options.ResourcesPath = "Resources");

			services.AddControllersWithViews();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			app.UseHttpsRedirection();

			app.UseStaticFiles();

			var supportedCultures = new List<CultureInfo>
			{
				new CultureInfo("en"),
				new CultureInfo("ru"),
				new CultureInfo("uk"),
			};
			var localizationOptions = new RequestLocalizationOptions
			{
				DefaultRequestCulture = new RequestCulture("en"),
				SupportedCultures = supportedCultures,
				SupportedUICultures = supportedCultures
			};
			var requestProvider = new RouteDataRequestCultureProvider();
			localizationOptions.RequestCultureProviders.Insert(0, requestProvider);

			app.UseRouter(routes =>
			{
				routes.MapMiddlewareRoute("{culture=en}/{*endpoints}", subApp =>
				{
					subApp.UseRouting();

					subApp.UseRequestLocalization(localizationOptions);

					subApp.UseAuthorization();

					subApp.UseEndpoints(endpoints =>
					{
						endpoints.MapControllerRoute(
							name: "default",
							pattern: "{culture=en}/{controller=Home}/{action=Index}/{id?}");
					});
				});
			});
		}
	}
}

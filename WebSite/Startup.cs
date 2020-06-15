using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using System.Collections.Generic;
using System.Globalization;

using WebSite.Services;

namespace WebSite
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
			services.AddLocalization(opts => { opts.ResourcesPath = "Resources"; });

			services.Configure<RequestLocalizationOptions>(
				opts =>
				{
					var supportedCultures = new List<CultureInfo>
					{
						new CultureInfo("en"),
						new CultureInfo("ru"),
						new CultureInfo("uk"),
					};

					opts.DefaultRequestCulture = new RequestCulture("en");
					// Formatting numbers, dates, etc.
					opts.SupportedCultures = supportedCultures;
					// UI strings that we have localized.
					opts.SupportedUICultures = supportedCultures;
				});
			services.AddSingleton<SharedResourcesService>();
			services.AddControllersWithViews()
				.AddViewLocalization(
				LanguageViewLocationExpanderFormat.Suffix,
				options => { options.ResourcesPath = "Resources"; })
				.AddDataAnnotationsLocalization();
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
				//app.UseHsts();
			}
			//app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthorization();

			var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
			app.UseRequestLocalization(options.Value);

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
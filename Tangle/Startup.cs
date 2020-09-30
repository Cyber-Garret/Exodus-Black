using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using BungieAPI;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Tangle.Entities.Models;

namespace Tangle
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
			//Add bungie settings in service collection for DI
			services.Configure<BungieSettings>(Configuration.GetSection("Bungie"));

			//Get bungie settings
			var bungie = Configuration.GetSection("Bungie").Get<BungieSettings>();

			//Bake config for Bungie API service
			var config = new Destiny2Config(Configuration["AppName"], Configuration["AppVersion"], Configuration["AppId"], Configuration["AppUrl"], Configuration["AppEmail"])
			{
				BaseUrl = bungie.BaseUrl,
				ApiKey = bungie.ApiKey,
				ManifestDatabasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "manifest")
			};
			//Add full Bungie API service
			services.AddDestiny2(config);

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

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}

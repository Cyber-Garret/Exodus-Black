﻿
using System.Collections.Generic;
using System.Globalization;

using Ex077.Entities.Options;
using Ex077.Services;

using Fuselage;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Neiralink;

namespace Ex077
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
			services.AddOptions<BotOptions>().Bind(Configuration.GetSection(BotOptions.OptionsName));
			services.AddOptions<HomeOptions>().Bind(Configuration.GetSection(HomeOptions.OptionsName));

			services.ConfigureAutoMapper()
				.Configure<CookiePolicyOptions>(options =>
			{
				// This lambda determines whether user consent for non-essential cookies is needed for a given request.
				options.CheckConsentNeeded = context => true;
				options.MinimumSameSitePolicy = SameSiteMode.None;
			})
				.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
				.AddCookie(cookieOptions =>
			{
				cookieOptions.LoginPath = "/Login";
			});

			services.AddLocalization(opts => { opts.ResourcesPath = "Resources"; });

			services.Configure<RequestLocalizationOptions>(
				opts =>
				{
					var supportedCultures = new List<CultureInfo>
					{
						new("en"),
						new("ru"),
						new("uk"),
					};

					opts.DefaultRequestCulture = new RequestCulture("en");
					// Formatting numbers, dates, etc.
					opts.SupportedCultures = supportedCultures;
					// UI strings that we have localized.
					opts.SupportedUICultures = supportedCultures;
				});
			services.AddSingleton<SharedResourcesService>();
			//DB
			var connString = Configuration.GetConnectionString("DefaultConnection");
			services.AddTransient<IWelcomeDbClient, WelcomeDbClient>(provider => new WelcomeDbClient(connString));
			services.AddTransient<IMilestoneDbClient, MilesoneDbClient>(provider => new MilesoneDbClient(connString));
			services.AddTransient<ICatalystDbClient, CatalystDbClient>(provider => new CatalystDbClient(connString));
			services.AddDbContext<FuselageContext>();

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
				app.UseDeveloperExceptionPage();
			else
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseForwardedHeaders(new ForwardedHeadersOptions
			{
				ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
			});

			app.UseAuthentication();
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

// Copyright (c) 2019, Raphael Beck. All rights reserved.
// Use of this source code is governed by the BSD 3-Clause license that can be found in the repository root directory's LICENSE file.

using System;
using System.Globalization;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrossPlatformGUI
{
    /// <summary>
    /// Class responsible for constructing the web application's middleware and IoC/service pipeline.<para> </para>
    /// Is consumed by <see cref="Program.CreateWebHostBuilder"/> (call to <c>CreateDefaultBuilder(args).UseStartup&lt;Startup&gt;()</c>).
    /// </summary>
    public class Startup
    {
        private readonly IConfiguration configuration;
        
        private readonly CultureInfo[] supportedCultures =
        {
            // Add all the languages you want to support here.
            new CultureInfo("en"),
            new CultureInfo("de"),
            new CultureInfo("it")
        };

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Configuration method called by the runtime.<para> </para>
        /// Responsible for adding services to the DI container.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> instance for registering objects into the IoC container.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent
                // for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            #region Localization service
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.Configure<RequestLocalizationOptions>(
                opts =>
                {
                    opts.DefaultRequestCulture = new RequestCulture(supportedCultures[0]);

                    // Formatting numbers, dates, etc.
                    opts.SupportedCultures = supportedCultures;

                    // UI strings that we have localized.
                    opts.SupportedUICultures = supportedCultures;
                }
            );
            #endregion

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();

            services.AddAntiforgery(
                options =>
                {
                    options.Cookie.Name = "_af";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.HeaderName = "X-XSRF-TOKEN";
                }
            );
        }

        /// <summary>
        /// Configuration method called by the runtime.<para> </para>
        /// Responsible for configuring the HTTP request pipeline.
        /// </summary>
        /// <param name="app"><see cref="IApplicationBuilder"/><see cref="IApplicationBuilder"/> instance for configuring the request pipeline.</param>
        /// <param name="env"><see cref="IHostingEnvironment"/>The application's hosting environment.</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Hook up the OnShutdown callback.
            var applicationLifetime = app.ApplicationServices.GetRequiredService<IApplicationLifetime>();
            applicationLifetime.ApplicationStopping.Register(OnShutdown);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                
                // The default HSTS value is 30 days.
                // You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            #region Localization
            var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(options.Value);
            #endregion

            app.UseCookiePolicy();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void OnShutdown()
        {
            Console.ResetColor();
            Console.WriteLine("\nGoodbye");
        }
    }
}

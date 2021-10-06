using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using Requestrr.WebApi.config;
using Requestrr.WebApi.RequestrrBot.ChatClients.Discord;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Ombi;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Radarr;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Sonarr;
using Requestrr.WebApi.RequestrrBot;
using Requestrr.WebApi.RequestrrBot.TvShows;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Overseerr;
using Microsoft.AspNetCore.Authentication;
using Requestrr.WebApi.Controllers.Configuration;
using Requestrr.WebApi.Controllers.DownloadClients;
using Requestrr.WebApi.Controllers.ChatClients;
using Requestrr.WebApi.Controllers.Authentication;
using Requestrr.WebApi.RequestrrBot.Movies;

namespace Requestrr.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        private ChatBot _requestrrBot;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddHttpClient();

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

            var authenticationSettings = Configuration.GetSection("Authentication");
            var applicationSettings = Configuration.Get<ApplicationSettings>();

            if (applicationSettings.DisableAuthentication)
            {
                services.AddAuthentication("Disabled")
                    .AddScheme<AuthenticationSchemeOptions, DisabledAuthenticationHandler>("Disabled", options => { });
            }
            else
            {
                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.Zero,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = "Requestrr",
                            ValidAudience = "Requestrr",
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationSettings.GetValue<string>("PrivateKey"))),
                        };
                    });
            }

            services.AddSingleton<OmbiClient, OmbiClient>();
            services.AddSingleton<RadarrClient, RadarrClient>();
            services.AddSingleton<DiscordSettingsProvider>();
            services.AddSingleton<TvShowsSettingsProvider>();
            services.AddSingleton<MoviesSettingsProvider>();
            services.AddSingleton<OmbiSettingsProvider>();
            services.AddSingleton<OverseerrSettingsProvider>();
            services.AddSingleton<BotClientSettingsProvider>();
            services.AddSingleton<ChatClientsSettingsProvider>();
            services.AddSingleton<DownloadClientsSettingsProvider>();
            services.AddSingleton<ApplicationSettingsProvider>();
            services.AddSingleton<AuthenticationSettingsProvider>();
            services.AddSingleton<RadarrSettingsProvider>();
            services.AddSingleton<SonarrSettingsProvider>();
            services.AddSingleton<RequestrrBot.ChatBot>();
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
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "ClientApp/build")),
                RequestPath = !string.IsNullOrWhiteSpace(Program.BaseUrl) ? Program.BaseUrl : string.Empty
            }); ;

            if (!string.IsNullOrWhiteSpace(Program.BaseUrl))
            {
                app.UsePathBase(Program.BaseUrl);
            }

            app.UseRouting();

            app.UseSpaStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "ClientApp/build/static")),
                RequestPath = "/static"
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });

            _requestrrBot = (RequestrrBot.ChatBot)app.ApplicationServices.GetService(typeof(RequestrrBot.ChatBot));
            _requestrrBot.Start();
        }
    }
}
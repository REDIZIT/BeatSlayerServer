using BeatSlayerServer.Utils;
using BeatSlayerServer.Utils.Shop;
using BeatSlayerServer.Services;
using BeatSlayerServer.Services.Chat;
using BeatSlayerServer.Services.Game;
using BeatSlayerServer.Services.MapsManagement;
using BeatSlayerServer.Services.Messaging;
using BeatSlayerServer.Services.Statistics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BeatSlayerServer.Models.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using BeatSlayerServer.Hubs;
using BeatSlayerServer.Services.Dashboard;
using BeatSlayerServer.Services.Messaging.Discord;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using InEditor.Analyze;
using Serilog;
using BeatSlayerServer.Services.Multiplayer;
using System;

namespace BeatSlayerServer
{
    public class Startup
    {
        public IConfiguration AppConfiguration { get; }


        public Startup(IConfiguration config)
        {
            AppConfiguration = config;
        }

        
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ServerSettings>(AppConfiguration);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Seq("http://localhost:5341")
                .CreateLogger();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie((options) =>
                {
                    options.LoginPath = "/Dashboard/Login";
                    options.AccessDeniedPath = "/Dashboard/Login";
                });

            services.AddControllersWithViews();

            services.AddCors(action =>
            action.AddPolicy("CorsPolicy", builder =>
              builder
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithOrigins()
                .AllowCredentials()));

            services.AddSignalR((o) =>
            {
                o.KeepAliveInterval = TimeSpan.FromMinutes(10);
                o.ClientTimeoutInterval = o.KeepAliveInterval * 2;
            }).AddMessagePackProtocol();


            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });



            services.AddSingleton<SettingsWrapper>();


            // Transient allow you to skip error with "some threads using same instance of DbContext"
            // But with singleton RankingService working properly..
            services.AddDbContext<MyDbContext>();

            services.AddTransient<ModerationService>();
            services.AddTransient<MapsService>();

            services.AddSingleton<DiscordBotWrapper>();
            services.AddSingleton<BotService>();

            services.AddSingleton<ConnectionService>();
            services.AddSingleton<TimerService>();
            services.AddSingleton<HeartbeatService>();
            services.AddSingleton<ChatService>();

            services.AddTransient<ShopService>();

            services.AddTransient<PublishService>();

            services.AddTransient<AccountService>();
            services.AddTransient<AccountFilesService>();
            services.AddTransient<VerificationService>();
            services.AddTransient<RankingService>();
            services.AddTransient<NotificationService>();

            services.AddTransient<SimulationService>();

            services.AddTransient<DashboardService>();


            services.AddSingleton<LobbyService>();
        }


        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory, 
            ConnectionService connectionService, HeartbeatService heartbeatService, DiscordBotWrapper dsbot, ILogger<Startup> logger)
        {
            loggerFactory.AddSerilog();

            app.UseDeveloperExceptionPage();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseStaticFiles();

            app.UseStatusCodePagesWithReExecute("/error/{0}");
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // il2cpp unity working
            app.UseCors("CorsPolicy");


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapHub<DashboardHub>("/DashboadHub");
                endpoints.MapHub<GameHub>("/GameHub");
                endpoints.MapHub<EditorHub>("/EditorHub");
            });
        }
    }
}

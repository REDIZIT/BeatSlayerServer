using BeatSlayerServer.Models.Configuration;
using BeatSlayerServer.Models.Database;
using BeatSlayerServer.Services;
using BeatSlayerServer.Utils.Database;
using BeatSlayerServer.Utils.Notifications;
using Microsoft.EntityFrameworkCore;

namespace BeatSlayerServer.Utils
{
    public class MyDbContext : DbContext
    {
        public DbSet<GroupInfo> Groups { get; set; }
        public DbSet<Account> Players { get; set; }
        public DbSet<ReplayInfo> Replays { get; set; }
        public DbSet<CodeVerificationRequest> VerificationRequests { get; set; }
        public DbSet<NotificationInfo> Notifications { get; set; }
        //public DbSet<PurchaseModel> Purchases { get; set; }


        private readonly ServerSettings settings;


        public MyDbContext() { }

        public MyDbContext(SettingsWrapper wrapper)
        {
            settings = wrapper.settings;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if(settings == null)
            {
                // If building project from PM EF Core
                optionsBuilder.UseMySQL("server=195.2.74.235;database=GameDb;uid=admindb;pwd=toor").UseLazyLoadingProxies();
                return;
            }

            string connStr = settings.ConnectionString;
            optionsBuilder.UseMySQL(connStr + ";Charset=utf8;").UseLazyLoadingProxies();
        }
    }
}

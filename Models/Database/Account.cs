using BeatSlayerServer.Dtos;
using BeatSlayerServer.Extensions;
using BeatSlayerServer.Models.Database;
using BeatSlayerServer.Utils.Notifications;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace BeatSlayerServer.Utils.Database
{
    /// <summary>
    /// Used in ef core
    /// </summary>
    public class Account
    {
        public int Id { get; set; }
        public string Nick { get; set; }

        /// <summary>
        /// Hashed password
        /// </summary>
        public string Password { get; set; }
        public string Email { get; set; }
        public AccountRole Role { get; set; }



        public virtual List<Account> Friends { get; set; } = new List<Account>();
        public virtual List<NotificationInfo> Notifications { get; set; } = new List<NotificationInfo>();



        public TimeSpan InGameTime { get; set; }
        public DateTime SignUpTime { get; set; }
        public DateTime LastLoginTime { get; set; }
        public DateTime LastActiveTimeUtc { get; set; }
        public string Country { get; set; }



        public double AllScore { get; set; }
        public double RP { get; set; }
        public int PlaceInRanking { get; set; }

        public virtual ICollection<ReplayInfo> Replays { get; set; } = new List<ReplayInfo>();
        [NotMapped] public IEnumerable<ReplayInfo> ReplaysUniq { get { return Replays.OrderByDescending(c => c.RP).DistinctBy(c => c.Map); } }


        /// <summary>
        /// Accuracy from 0 to 1 (Hits / AllCubes)
        /// </summary>
        public float Accuracy { get { return (Hits + Misses) > 0 ? Hits / (Hits + Misses) : -1; } }
        public int MaxCombo { get; set; }
        public int Hits { get; set; }
        public int Misses { get; set; }
        
        // Count of replays grades
        public int SS { get; set; }
        public int S { get; set; }
        public int A { get; set; }
        public int B { get; set; }
        public int C { get; set; }
        public int D { get; set; }



        // Map creator stuff
        public int MapsPublished { get; set; }
        public int PublishedMapsPlayed { get; set; }
        public int PublishedMapsLiked { get; set; }


        // Shop stuff
        public int Coins { get; set; }
        public virtual List<PurchaseModel> Purchases { get; set; } = new List<PurchaseModel>();





        public Account() { }
        public Account(string nick, string password, string country)
        {
            Nick = nick;
            Password = SecurityHelper.GetMd5Hash(password);
            Country = country;
            SignUpTime = DateTime.Now;

            PlaceInRanking = -1;
        }



        public void ApplyGrade(Grade grade)
        {
            switch (grade)
            {
                case Grade.SS: SS++; break;
                case Grade.S: S++; break;
                case Grade.A: A++; break;
                case Grade.B: B++; break;
                case Grade.C: C++; break;
                case Grade.D: D++; break;
            }
        }


        public AccountData Cut(ToStringType type, string password = "")
        {
            bool b_public = type == ToStringType.View || type == ToStringType.All;
            bool b_all = type == ToStringType.All;
            bool b_login = type == ToStringType.Login;


            if (b_all || b_login)
            {
                if (SecurityHelper.GetMd5Hash(password) != Password) return null;
            }


            AccountData acc = new AccountData();

            acc.Nick = Nick;
            acc.Role = Role;
            acc.Country = Country;

            if (b_public || b_all || b_login)
            {
                acc.InGameTimeTicks = InGameTime.Ticks;
                //acc.LastLoginTime = LastLoginTime;
                acc.LastActiveTimeUtcTicks = LastActiveTimeUtc.Ticks;
                acc.MapsPublished = MapsPublished;
                acc.MaxCombo = MaxCombo;
                acc.Misses = Misses;
                acc.Hits = Hits;
                acc.RP = (long)RP;
                acc.AllScore = (long)AllScore;
                acc.PlaceInRanking = PlaceInRanking;
                acc.PublishedMapsLiked = PublishedMapsLiked;
                acc.PublishedMapsPlayed = PublishedMapsPlayed;
                acc.Friends = new List<AccountData>();
                foreach(var friend in Friends)
                {
                    acc.Friends.Add(new AccountData()
                    {
                        Id = friend.Id,
                        Nick = friend.Nick,
                        //LastActiveTimeUtcTicks = friend.LastActiveTimeUtc.Ticks
                    });
                }

                acc.SS = SS;
                acc.S = S;
                acc.A = A;
                acc.B = B;
                acc.C = C;
                acc.D = D;
                //acc.SignUpTime = SignUpTime;
            }
            if (b_login)
            {
                acc.Email = Email;
                acc.Coins = Coins;
                acc.Purchases = Purchases;
                acc.Notifications = Notifications;
            }

            return acc;
        }

        // Deprecated. Use Cut()
        public enum ToStringType
        {
            Login,
            View,
            All
        }
    }
    public enum AccountRole
    {
        Player,
        Moderator,
        Developer
    }


    /// <summary>
    /// Used in game
    /// </summary>
    public class AccountData
    {
        
       

        public int Id { get; set; }
        public string Nick { get; set; }

        /// <summary>
        /// Hashed password
        /// </summary>
        public string Password { get; set; }
        public string Email { get; set; }
        public AccountRole Role { get; set; }



        public List<AccountData> Friends { get; set; } = new List<AccountData>();
        public List<NotificationInfo> Notifications { get; set; } = new List<NotificationInfo>();



        /*public TimeSpan InGameTime { get; set; }

        public DateTime SignUpTime { get; set; }
        public DateTime LastLoginTime { get; set; }*/
        public long InGameTimeTicks { get; set; }
        public long SignUpTimeUtcTicks { get; set; }
        public long LastLoginTimeUtcTicks { get; set; }
        public long LastActiveTimeUtcTicks { get; set; }

        public TimeSpan InGameTime => new TimeSpan(InGameTimeTicks);
        public DateTime SignUpTimeUtc => new DateTime(SignUpTimeUtcTicks);
        public DateTime LastLoginTimeUtc => new DateTime(LastLoginTimeUtcTicks);
        public DateTime LastActiveTimeUtc => new DateTime(LastActiveTimeUtcTicks);


        public bool IsOnline => (DateTime.UtcNow - LastActiveTimeUtc).TotalSeconds < 40;





        public string Country { get; set; }



        public long AllScore { get; set; }
        public long RP { get; set; }
        public int PlaceInRanking { get; set; }

        //public virtual ICollection<ReplayInfo> Replays { get; set; } = new List<ReplayInfo>();


        /// <summary>
        /// Accuracy from 0 to 1 (Hits / AllCubes)
        /// </summary>
        public float Accuracy { get { return (Hits + Misses) > 0 ? (float)Hits / (float)(Hits + Misses) : -1; } }
        public int MaxCombo { get; set; }
        public int Hits { get; set; }
        public int Misses { get; set; }

        // Count of replays grades
        public int SS { get; set; }
        public int S { get; set; }
        public int A { get; set; }
        public int B { get; set; }
        public int C { get; set; }
        public int D { get; set; }


        // Map creator stuff
        public int MapsPublished { get; set; }
        public int PublishedMapsPlayed { get; set; }
        public int PublishedMapsLiked { get; set; }


        // Shop stuff
        public int Coins { get; set; }
        public List<PurchaseModel> Purchases { get; set; }
    }
}

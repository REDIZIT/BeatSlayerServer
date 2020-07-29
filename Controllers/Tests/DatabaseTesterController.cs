using BeatSlayerServer.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatSlayerServer.Controllers.Tests
{
    public class DatabaseTesterController : Controller
    {
        private readonly MyDbContext ctx;

        public DatabaseTesterController(MyDbContext ctx)
        {
            this.ctx = ctx;
        }

        public async Task<string> Test()
        {
            return await Task.Run(() =>
            {
                // Skip connection step
                ctx.Players.FirstOrDefault(c => c.Nick == "REDIZIT");

                StringBuilder builder = new StringBuilder();
                Stopwatch w = new Stopwatch();



               

                //var groups = ctx.Groups.Select(c => new
                //{
                //    Id = c.Id,
                //    Author = c.Author,
                //    Name = c.Name,
                //    Maps = c.Maps.Select(m => m.Nick)
                //});

                //var publishedMaps = ctx.Groups.Where(g => g.Maps.Any(c => c.Nick == "REDIZIT")).Select(c => new
                //{
                //    Author = c.Author,
                //    Name = c.Name,
                //    MapsCount = c.Maps.Where(m => m.Nick == "REDIZIT").Count()
                //});

                var player = ctx.Players.Where(c => c.Nick == "REDIZIT").Select(c => c.Nick).ToList();

                w.Start();

                
                ctx.Players.Where(c => c.Password != null);

                w.Stop();

                //string json = JsonConvert.SerializeObject(publishedMaps.ToList()[0], new JsonSerializerSettings()
                //{
                //    Formatting = Formatting.Indented,
                //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                //});
                //Console.WriteLine(json);

                //foreach (var map in publishedMaps.ToList())
                //{
                //    builder.AppendLine(map.Author + "-" + map.Name + "                    " + map.MapsCount);
                //}

                //Console.WriteLine(publishedMaps.Count());

                builder.AppendLine($"Test4 average time is {w.ElapsedMilliseconds}ms");

                return builder.ToString();
            });
        }
        private void Test1()
        {
            var players = ctx.Players.Where(c => c.Nick != "");
            foreach (var player in players)
            {
                var replays = ctx.Replays.Where(c => c.Player.Id == player.Id && c.Score > 1000);
            }
        }
        private void Test2()
        {
            var players = ctx.Players.Where(c => c.Nick != "").Include(c => c.Replays);
            foreach (var player in players)
            {
                var replays = player.Replays.Where(c => c.Score > 1000);
            }
        }
        private void Test3()
        {
            var players = ctx.Players
                .Where(c => c.Nick != "")
                .SelectMany(p => p.Replays.Where(r => r.Score > 1000));

            //Console.WriteLine(players.First().);
        }
        private void Test4()
        {
            
        }
    }
}

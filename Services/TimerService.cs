using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace BeatSlayerServer.Services
{
    public class TimerService
    {
        private List<Timer> Timers { get; set; } = new List<Timer>();
        private bool IsWatchmakerWorking { get; set; }

        public void AddTimer(Action onTick, TimeSpan period)
        {
            double tickTime = period.TotalMilliseconds;

            Timer timer = new Timer(tickTime);
            Timers.Add(timer);

            timer.Elapsed += new ElapsedEventHandler((o, e) => onTick());

            Task.Run(StartWatchmaker);
        }

        /// <summary>
        /// Start timers at round time (if interval is 10 seconds start in 4:50, not 4:49)
        /// </summary>
        private async Task StartWatchmaker()
        {
            if (IsWatchmakerWorking) return;
            IsWatchmakerWorking = true;

            while(Timers.Where(c => !c.Enabled).Count() > 0)
            {
                foreach (var timer in Timers.Where(c => !c.Enabled))
                {
                    int intervalSeconds = (int)Math.Ceiling(timer.Interval / 1000f);
                    int currentSeconds = (int)Math.Ceiling(DateTime.Now.TimeOfDay.TotalSeconds - 1);

                    if (currentSeconds % intervalSeconds == 0)
                    {
                        timer.Start();
                    }
                }

                await Task.Delay(100);
            }

            IsWatchmakerWorking = false;
        }
    }
}

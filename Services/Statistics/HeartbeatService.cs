using BeatSlayerServer.Models.Configuration;
using BeatSlayerServer.Utils;
using BeatSlayerServer.Utils.Statistics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BeatSlayerServer.Services.Statistics
{
    public class HeartbeatService
    {
        private HeartbeatFrame[] Frames { get; set; }
        private HeartbeatFrame CurrentFrame { get; set; }
        private List<IHeartbeatData> CurrentData { get; set; } = new List<IHeartbeatData>();


        private readonly string filepath;
        private readonly ServerSettings settings;

        public HeartbeatService(ConnectionService connectionService, SettingsWrapper wrapper, TimerService timerService)
        {
            settings = wrapper.settings;

            if (settings.Heartbeat.DataCollectingTime == 0) return;
            filepath = settings.Heartbeat.DataFilePath;

            float chartSize = 60f / (settings.Heartbeat.DataCollectingTime / 60f) * 24;
            Frames = new HeartbeatFrame[(int)chartSize];

            timerService.AddTimer(CaptureFrame, TimeSpan.FromSeconds(settings.Heartbeat.DataCollectingTime));

            connectionService.OnOnlineChange += OnOnlineChange;

            ResetCollectedInfo();
            LoadData();
        }




        public ChartInfo GetChartInfo(int timeoffset)
        {
            string subtitle = "Data is collected " + DelayToString(GetDelay());

            return new ChartInfo()
            {
                subtitle = subtitle,
                times = Frames.Select(c => c == null ? "-" : FormatTime(c.Time, timeoffset)).ToArray(),
                oldOnlineData = Frames.Select(c => c == null ? 0 : c.OldOnline).ToArray(),
                signalrOnlineData = Frames.Select(c => c == null ? 0 : c.SignalrOnline).ToArray(),
                gameLaunches = Frames.Select(c => c == null ? 0 : c.GameLaunchCount).ToArray(),
                gameLaunchesAnonim = Frames.Select(c => c == null ? 0 : c.GameLaunchAnonimCount).ToArray()
            };
        }
        public ChartInfo GetChartMap(int timeoffset)
        {
            string subtitle = "Data is collected " + DelayToString(GetDelay());

            return new ChartInfo()
            {
                subtitle = subtitle,
                times = Frames.Select(c => c == null ? "-" : FormatTime(c.Time, timeoffset)).ToArray(),
                gamesApprovedCount = Frames.Select(c => c == null ? 0 : c.GamesApprovedCount).ToArray(),
                gamesCount = Frames.Select(c => c == null ? 0 : c.GamesCount).ToArray()
            };
        }


        public void CaptureFrame()
        {
            CurrentFrame = new HeartbeatFrame()
            {
                Time = DateTime.UtcNow
            };

            foreach (var data in CurrentData)
            {
                data.Apply(CurrentFrame);
            }


            AddData(Frames, CurrentFrame);

            Console.WriteLine(CurrentFrame.Time);

            SaveData();
            ResetCollectedInfo();
        }



        public void AddData(IHeartbeatData data)
        {
            CurrentData.Add(data);
        }
        private void OnOnlineChange(int online)
        {
            AddData(new HeartbeatDataOnline(online));
        }



        #region Service code

        private void SaveData()
        {
            var fileinfo = new FileInfo(filepath);
            Directory.CreateDirectory(fileinfo.Directory.FullName);

            string json = JsonConvert.SerializeObject(Frames);
            File.WriteAllText(filepath, json);
        }
        private void LoadData()
        {
            if (!File.Exists(filepath)) return;

            string json = File.ReadAllText(filepath);
            Frames = JsonConvert.DeserializeObject<HeartbeatFrame[]>(json);
        }



        /// <summary>
        /// Get data collecting time in seconds
        /// </summary>
        private int GetDelay()
        {
            return settings.Heartbeat.DataCollectingTime;
        }
        private string DelayToString(int seconds)
        {
            int minutes = (int)Math.Floor(seconds / 60f);

            if (minutes == 0) return $"every {seconds} seconds";
            else
            {
                if (minutes == 1) return $"every minute";
                else return $"every {minutes} minutes";
            }
        }
        private void ResetCollectedInfo()
        {
            CurrentData.Clear();
        }
        private void AddData(object[] arr, object d)
        {
            for (int i = 1; i < arr.Length; i++)
            {
                arr[i - 1] = arr[i];
            }
            arr[^1] = d;
        }
        private string FormatTime(DateTime t, int minutesOffset)
        {
            DateTime time = t.AddMinutes(-minutesOffset);
            return time.ToString("HH:mm:ss");
        }

        #endregion
    }
}

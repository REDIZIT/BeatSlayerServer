using BeatSlayerServer.Models.Configuration;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Reflection;

namespace BeatSlayerServer.Services
{
    /// <summary>
    /// Allows you use <see cref="ServerSettings"/> always refreshed (Any file change will refresh this class).
    /// Use DI and add <see cref="SettingsWrapper"/> like singleton service.
    /// </summary>
    public class SettingsWrapper
    {
        /// <summary>
        /// Always fresh ServerSettings. Use it.
        /// </summary>
        public ServerSettings settings;

        public SettingsWrapper(IOptionsMonitor<ServerSettings> mon)
        {
            settings = mon.CurrentValue;
            mon.OnChange(OnFileChanged);
        }

        /// <summary>
        /// Copy only properties values from new settings (from file) to <see cref="settings"/>.
        /// We can't just use settings = newSettings due to <see cref="settings"/> will become another object.
        /// </summary>
        private void OnFileChanged(ServerSettings newSettings)
        {
            foreach (PropertyInfo property in typeof(ServerSettings).GetProperties().Where(p => p.CanWrite))
            {
                property.SetValue(settings, property.GetValue(newSettings, null), null);
            }
        }
    }
}

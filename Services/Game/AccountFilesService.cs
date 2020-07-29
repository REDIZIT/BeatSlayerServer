using BeatSlayerServer.Models.Configuration;
using System.IO;

namespace BeatSlayerServer.Services.Game
{
    /// <summary>
    /// Service for management account files and folders
    /// </summary>
    public class AccountFilesService
    {
        private readonly ServerSettings settings;

        public AccountFilesService(SettingsWrapper wrapper)
        {
            settings = wrapper.settings;
        }

        public void CreateDataFolder(string nick)
        {
            string path = settings.AccountsDataPath + "/" + nick;
            Directory.CreateDirectory(path);
        }

        public byte[] GetAvatar(string nick)
        {
            string filepath = GetAvatarPath(nick);

            if (filepath == "") return File.ReadAllBytes(settings.DefaultAvatarPath);
            else return File.ReadAllBytes(filepath);
        }
        public void SetAvatar(string nick, byte[] bytes, string extension)
        {
            string folderPath = settings.AccountsDataPath + "/" + nick;
            string path = folderPath + "/avatar" + extension;

            RemoveAvatar(nick);

            Directory.CreateDirectory(folderPath);
            File.WriteAllBytes(path, bytes);
        }
        public void RemoveAvatar(string nick)
        {
            string currentAvatarPath = GetAvatarPath(nick);
            if (currentAvatarPath != "") File.Delete(currentAvatarPath);
        }




        public byte[] GetBackground(string nick)
        {
            string filepath = GetBackgroundPath(nick);

            if (filepath == "") return new byte[0];
            else return File.ReadAllBytes(filepath);
        }
        public void SetBackground(string nick, byte[] bytes, string extension)
        {
            string folderPath = settings.AccountsDataPath + "/" + nick;
            string path = folderPath + "/background" + extension;

            RemoveBackground(nick);

            Directory.CreateDirectory(folderPath);
            File.WriteAllBytes(path, bytes);
        }
        public void RemoveBackground(string nick)
        {
            string currentAvatarPath = GetBackgroundPath(nick);
            if (currentAvatarPath != "") File.Delete(currentAvatarPath);
        }




        #region Service code

        public string GetAvatarPath(string nick)
        {
            return FindFile(settings.AccountsDataPath + "/" + nick + "/avatar", ".png", ".jpg");
        }
        public string GetBackgroundPath(string nick)
        {
            return FindFile(settings.AccountsDataPath + "/" + nick + "/background", ".png", ".jpg");
        }


        /// <summary>
        /// Find file with unknown extension
        /// </summary>
        /// <param name="pathWithoutExtension">File path without extension (Data/avatar)</param>
        /// <param name="extensions">Extension with dot (.png, .mp3, .ogg)</param>
        /// <returns>Empty string if not found or path to file</returns>
        public string FindFile(string pathWithoutExtension, params string[] extensions)
        {
            foreach (string extension in extensions)
            {
                if (File.Exists(pathWithoutExtension + extension))
                {
                    return pathWithoutExtension + extension;
                }
            }
            return "";
        }

        #endregion
    }
}

using System;
using System.IO;

namespace BeatSlayerServer.Utils
{
    /// <summary>
    /// Help with searching files with unkown extension
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// Try to find file by filename and given extensions
        /// </summary>
        /// <param name="filename">Path to file without extension</param>
        /// <param name="foundfilename">Found file by given extensions</param>
        /// <param name="extensions">Extensions to search with</param>
        /// <returns>Is file found with these extensions</returns>
        public static bool TryFindFile(string filename, out string foundfilename, params string[] extensions)
        {
            foreach (string extension in extensions)
            {
                string path = filename + extension;
                if (File.Exists(path))
                {
                    foundfilename = path;
                    return true;
                }
            }

            foundfilename = "";
            return false;
        }
    }
}

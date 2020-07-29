namespace BeatSlayerServer.Utils
{
    public static class UrlHelper
    {
        public static string Encode(string url)
        {
            return url.Replace("&", "%amp%");
        }
        public static string Decode(string url)
        {
            return url.Replace("%amp%", "&");
        }
    }
}

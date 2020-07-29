using System;
using System.IO;

namespace BeatSlayerServer.Utils
{
    public class AudioCutter
    {
        //public List<byte> fileBytes;
        public byte[] fileBytes;


        public void LoadFile(string filepath)
        {
            fileBytes = File.ReadAllBytes(filepath);
        }

        public byte[] CutAudioFile(float timeInSeconds, float startTrim, float trimSeconds)
        {
            float averageBytesPerSecond = GetAverageTime(timeInSeconds);

            int startByte = (int)Math.Floor(averageBytesPerSecond * startTrim);
            int trimBytesCount = (int)Math.Floor(averageBytesPerSecond * trimSeconds);

            byte[] resultBytes = new byte[trimBytesCount];

            int idx = 0;
            for (int i = startByte; i < startByte + trimBytesCount; i++)
            {
                resultBytes[idx] = fileBytes[i];
                idx++;
            }

            return resultBytes;
        }




        // Not used
        public string GetHeader()
        {
            string str = "";

            int idx = 0;
            for (int i = 0/*1089*/; i < 2049; i++)
            {
                idx++;
                str += ByteToString(fileBytes[i]) + (idx % 4 == 0 ? "\n" : idx % 2 == 0 ? "   " : " ");
            }

            return str;
        }
        public string ByteToString(byte b)
        {
            return Convert.ToString(b, 2).PadLeft(8, '0');
        }

        // Not used
        public byte[] CutMp3File(float startSeconds, float trimSeconds)
        {
            int bytesPerSecond = 20000;
            int startByte = (int)Math.Floor(bytesPerSecond * startSeconds);
            int trimBytesCount = (int)Math.Floor(bytesPerSecond * trimSeconds);



            byte[] resultBytes = new byte[trimBytesCount];

            byte[] ID3tag = new byte[3]
            {
                fileBytes[0], fileBytes[1], fileBytes[2]
            };

            int skipFirstBytes = 0;
            if (ID3tag == new byte[3] { 73, 68, 51 })
            {
                skipFirstBytes = 1089;
            }


            int idx = 0;
            for (int i = skipFirstBytes + startByte; i < startByte + trimBytesCount - skipFirstBytes; i++)
            {
                resultBytes[idx] = fileBytes[i];
                idx++;
            }

            return resultBytes;
        }

        // Works
        public byte[] SimpleCut(float startSeconds, float trimSeconds)
        {
            int bytesPerSecond = (int)Math.Floor(GetAverageTime(3 * 60 + 26));
            int startByte = (int)Math.Floor(bytesPerSecond * startSeconds);
            int trimBytesCount = (int)Math.Floor(bytesPerSecond * trimSeconds);

            byte[] resultBytes = new byte[trimBytesCount];

            int idx = 0;
            for (int i = startByte; i < startByte + trimBytesCount; i++)
            {
                resultBytes[idx] = fileBytes[i];
                idx++;
            }

            return resultBytes;
        }



        public float GetAverageTime(float timeInSeconds)
        {
            return fileBytes.Length / timeInSeconds;
        }
    }
}

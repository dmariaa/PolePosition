using System;

namespace PolePosition
{
    public static class Utils
    {
        public static string FormatTime(float time)
        {
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(time * 1000);
            return string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D4}", 
                timeSpan.Hours, 
                timeSpan.Minutes,
                timeSpan.Seconds, 
                timeSpan.Milliseconds);
        }
    }
}
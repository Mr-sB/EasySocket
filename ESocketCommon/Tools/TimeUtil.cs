using System;

namespace ESocket.Common.Tools
{
    public static class TimeUtil
    {
        public static DateTime GetCurrentUtcTime()
        {
            return DateTime.UtcNow;
        }

        /// <summary>
        /// 获得time1-time2的时间差(秒)
        /// </summary>
        /// <param name="time1">时间1</param>
        /// <param name="time2">时间2</param>
        /// <param name="signed">是否带符号</param>
        /// <returns>时间差(秒)</returns>
        public static long GetDifferenceSeconds(this DateTime time1, DateTime time2, bool signed = false)
        {
            if (time1 == null || time2 == null) return 0;
            long seconds = (long)(time1.Subtract(time2).TotalSeconds);
            return signed ? seconds : Math.Abs(seconds);
        }

        /// <summary>
        /// 获得DateTime.UtcNow-time的时间差(秒)
        /// </summary>
        /// <param name="time">时间</param>
        /// <param name="signed">是否带符号</param>
        /// <returns>时间差(秒)</returns>
        public static long GetDifferenceSeconds(this DateTime time, bool signed = false)
        {
            if (time == null) return 0;
            return DateTime.UtcNow.GetDifferenceSeconds(time);
        }
    }
}

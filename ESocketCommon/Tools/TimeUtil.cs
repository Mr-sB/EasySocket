using System;

namespace ESocket.Common.Tools
{
    public static class TimeUtil
    {
        private static readonly DateTime mZeroTime = new DateTime(1970, 1, 1);
        public static readonly long ZERO_TIME_TICKS = mZeroTime.Ticks;
        
        /// <summary>
        /// 获取当前Utc时间
        /// </summary>
        /// <returns>Utc时间</returns>
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
            long seconds = (long)time1.Subtract(time2).TotalSeconds;
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
            return GetCurrentUtcTime().GetDifferenceSeconds(time);
        }

        /// <summary>
        /// 获取time的总共微妙数
        /// </summary>
        /// <param name="time">时间</param>
        /// <returns>微秒</returns>
        public static long GetTotalMilliseconds(this DateTime time)
        {
            return (long)time.Subtract(mZeroTime).TotalMilliseconds;
        }

        /// <summary>
        /// 将微秒数转换为时间
        /// </summary>
        /// <param name="milliseconds">微秒</param>
        /// <returns>时间</returns>
        public static DateTime ParseFromMilliseconds(this long milliseconds)
        {
            return new DateTime(ZERO_TIME_TICKS).AddMilliseconds(milliseconds);
        }
    }
}

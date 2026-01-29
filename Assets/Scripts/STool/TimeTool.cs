using System;
using UnityEngine;

namespace STool
{
    public static class TimeUtils
    {
        /// <summary>
        /// 将秒数转换为时、分、秒
        /// </summary>
        /// <param name="totalSeconds">总秒数（float）</param>
        /// <param name="hours">输出的小时数</param>
        /// <param name="minutes">输出的分钟数</param>
        /// <param name="seconds">输出的秒数</param>
        /// <param name="remain">剩余2位小数</param>
        // ReSharper disable once InconsistentNaming
        public static void SecondsToHMS(float totalSeconds, out int hours, out int minutes, out int seconds,
            out int remain)
        {
            var t = Mathf.FloorToInt(totalSeconds); // 转成整数秒，舍去小数部分

            hours = t / 3600;
            minutes = (t % 3600) / 60;
            seconds = t % 60;
            remain = Mathf.RoundToInt((totalSeconds % 1 - totalSeconds % 0.01f) * 100);
        }

        #region 字符串转换
        // 复用缓冲区，避免反复分配
        private static readonly char[] Buffer = new char[8];

        /// <summary>
        /// 格式化时间为 "xx:xx:xx"
        /// 0 GC（仅最终 string 分配，无法避免）
        /// </summary>
        public static string ToTimeString(int hour, int minute, int second)
        {
            Buffer[0] = (char)('0' + hour / 10);
            Buffer[1] = (char)('0' + hour % 10);
            Buffer[2] = ':';
            Buffer[3] = (char)('0' + minute / 10);
            Buffer[4] = (char)('0' + minute % 10);
            Buffer[5] = ':';
            Buffer[6] = (char)('0' + second / 10);
            Buffer[7] = (char)('0' + second % 10);

            return new string(Buffer);
        }
        #endregion
    }
}

using System.Text;

namespace ESocket.Common.Tools
{
    /// <summary>
    /// 工具类
    /// </summary>
    internal static class TransporterTool
    {
        /// <summary>
        /// 获得接收缓冲区
        /// </summary>
        /// <returns>字节数组</returns>
        public static byte[] GetReceiveBuffer()
        {
            return new byte[ESocketConst.ReceiveBufferSize];
        }

        /// <summary>
        /// PackageModel转字节数组
        /// </summary>
        /// <returns>字节数组</returns>
        public static byte[] ToPackageBuffer(this PackageModel model)
        {
            if (model == null) return null;
            return Encoding.UTF8.GetBytes(model.ToJson() + ESocketConst.SendPackageEndFlag);
        }

        /// <summary>
        /// Json转PackageModel
        /// </summary>
        /// <param name="json">json</param>
        /// <returns>PackageModel</returns>
        public static PackageModel ToPackageModel(this string json)
        {
            if (json == null || json.Length <= 0) return null;
            return json.ToObject<PackageModel>();
        }

        /// <summary>
        /// 字节数组转为字符串
        /// </summary>
        /// <param name="buffer">字节数组</param>
        /// <returns>字符串</returns>
        public static string ToFullString(this byte[] buffer)
        {
            if (buffer == null) return "null";
            return ToFullString(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 字节数组转为字符串
        /// </summary>
        /// <param name="buffer">字节数组</param>
        /// <param name="offset">起始偏移</param>
        /// <param name="count">长度</param>
        /// <returns>字符串</returns>
        public static string ToFullString(this byte[] buffer, int offset, int count)
        {
            if (buffer == null) return "null";
            StringBuilder sb = new StringBuilder().Append('[');
            int len = buffer.Length - offset;
            if (count < len) len = count;
            for (int i = offset; i < len; i++)
            {
                if (i != offset)
                    sb.Append(',');
                sb.Append(buffer[i]);
            }
            return sb.Append(']').ToString();
        }
    }
}

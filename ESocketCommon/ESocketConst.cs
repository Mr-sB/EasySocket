namespace ESocket.Common
{
    public static class ESocketConst
    {
        /// <summary>
        /// 接收缓冲区大小/byte
        /// </summary>
        public static int ReceiveBufferSize = 1024 * 1024;
        /// <summary>
        /// 发送心跳包间隔/s
        /// </summary>
        public static int SendHeartbeatInterval = 10;
        /// <summary>
        /// 心跳包超时时间/s，超过时间就判断为断开连接
        /// </summary>
        public static int HeartbeatTimeout = 15;
        /// <summary>
        /// 消息结尾标记。转义字符，json里不包含，不产生冲突
        /// 用于解决粘包问题
        /// 对方发来了1M的数据量过来，但是，本地的buffer只有1024字节，那就代表socket需要重复很多次才能真正收完这逻辑上的一整个消息。
        /// 对方发来了5条2个字符的消息，本地的buffer（大小1024字节）会将这5条消息全部收入囊下...
        /// </summary>
        public const string SendPackageEndFlag = "\r\n";
        /// <summary>
        /// 包分隔符
        /// </summary>
        public static readonly string[] PackageSeparator = new[] { SendPackageEndFlag };
    }
}

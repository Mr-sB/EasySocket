namespace ESocket.Common
{
    /// <summary>
    /// 包代码，区别包类型
    /// </summary>
    public enum PackageCode
    {
        /// <summary>
        /// 连接
        /// </summary>
        Connect,
        /// <summary>
        /// 心跳
        /// </summary>
        Heartbeat,
        /// <summary>
        /// 请求
        /// </summary>
        Request,
        /// <summary>
        /// 响应
        /// </summary>
        Response
    }
}

using ESocket.Common.Tools;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ESocket.Common
{
    /// <summary>
    /// 传输消息的抽象类
    /// </summary>
    public abstract class Transporter
    {
        /// <summary>
        /// 通信套接字
        /// </summary>
        protected Socket mSocket;
        /// <summary>
        /// 监听客户端消息的线程
        /// </summary>
        private LoopThread mClientThread;
        /// <summary>
        /// 检测心跳线程
        /// </summary>
        private LoopThread mListenHeartbeatThread;
        /// <summary>
        /// 上一次接收心跳(收到消息)的时间
        /// </summary>
        public DateTime LastListenHeartbeatTime { private set; get; }
        /// <summary>
        /// 上一次发送心跳(发送消息)的时间
        /// </summary>
        public DateTime LastSendHeartbeatTime { private set; get; }
        
        /// <summary>
        /// 接收到消息
        /// </summary>
        /// <param name="model"></param>
        protected abstract void OnReceive(PackageModel model);
        
        /// <summary>
        /// 异常中断
        /// </summary>
        protected virtual void ExceptionDisconnect()
        {
            DisconnectInternal();
        }
        
        /// <summary>
        /// 中断连接
        /// </summary>
        protected void DisconnectInternal()
        {
            //中断线程
            mClientThread?.Stop();
            mClientThread = null;
            mListenHeartbeatThread?.Stop();
            mListenHeartbeatThread = null;
            //关闭通信的套接字
            try
            {
                mSocket?.Shutdown(SocketShutdown.Both);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                mSocket?.Close();
            }
            mSocket = null;
        }

        /// <summary>
        /// 转为字符串
        /// </summary>
        /// <returns>返回Socket.RemoteEndPoint.ToString()。如果Socket为null则返回"null"</returns>
        public sealed override string ToString()
        {
            if (mSocket == null) return "null";
            try
            {
                return mSocket.RemoteEndPoint.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "null";
            }
        }

        public Transporter(Socket socket)
        {
            mSocket = socket;
        }

        protected void Init()
        {
            LastListenHeartbeatTime = TimeUtil.GetCurrentUtcTime();
            LastSendHeartbeatTime = TimeUtil.GetCurrentUtcTime();
            //初始化监听客户端消息线程
            InitListenReceiveThread();
            //初始化心跳检测线程
            InitListenHeartbeatThread();
        }

        /// <summary>
        /// 发送请求
        /// </summary>
        /// <param name="parameters">参数列表</param>
        public void SendRequest(Dictionary<string, object> parameters)
        {
            Send(PackageCode.Request, new OperationRequest(parameters));
        }

        /// <summary>
        /// 回复响应
        /// </summary>
        /// <param name="returnCode">返回码</param>
        /// <param name="parameters">参数列表</param>
        public void SendResponse(int returnCode, Dictionary<string, object> parameters)
        {
            Send(PackageCode.Response, new OperationResponse(returnCode, parameters));
        }

        /// <summary>
        /// 发送心跳包
        /// </summary>
        protected void SendHeartbeat()
        {
            Send(PackageCode.Heartbeat, null);
        }

        /// <summary>
        /// 发送连接信息
        /// </summary>
        /// <param name="connectCode">连接状态</param>
        protected void SendConnect(ConnectCode connectCode)
        {
            Send(PackageCode.Connect, new OperationConnect(connectCode));
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        private void Send(PackageCode packageCode, IOperation operation)
        {
            if (mSocket == null) return;
            try
            {
                mSocket.Send(new PackageModel(packageCode, operation).ToPackageBuffer());
                LastSendHeartbeatTime = TimeUtil.GetCurrentUtcTime();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// 初始化监听通信的线程
        /// </summary>
        private void InitListenReceiveThread()
        {
            //与客户端通信的线程
            mClientThread = new LoopThread(ListenReceiveThread) {IsBackground = true};
            mClientThread.Start();
        }

        /// <summary>
        /// 初始化检测心跳线程
        /// </summary>
        private void InitListenHeartbeatThread()
        {
            mListenHeartbeatThread = new LoopThread(ListenHeartbeatThread) {IsBackground = true};
            mListenHeartbeatThread.Start();
        }

        /// <summary>
        /// 监听消息
        /// </summary>
        private void ListenReceiveThread(LoopThread.Token token)
        {
            if (mSocket == null) return;
            //创建内存缓冲区
            byte[] recBuffer = TransporterTool.GetReceiveBuffer();
            StringBuilder recStringBuilder = new StringBuilder();
            while (token.IsLooping)
            {
                try
                {
                    int length;
                    try
                    {
                        //接收到的信息放入缓冲区，并获得字节数组长度
                        length = mSocket.Receive(recBuffer);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("中断连接\nMessage:{0}\nStackTrace:{1}", e.Message, e.StackTrace);
                        ExceptionDisconnect();
                        return;
                    }
                    LastListenHeartbeatTime = TimeUtil.GetCurrentUtcTime();
                    //接收到的字符串
                    string recString = Encoding.UTF8.GetString(recBuffer, 0, length);
                    //拼接之后的结果
                    recString = recStringBuilder.Append(recString).ToString();
                    string[] packageJsons = recString.Split(ESocketConst.PackageSeparator, StringSplitOptions.RemoveEmptyEntries);
                    if (packageJsons.Length > 0)
                    {
                        //没有完整包
                        if (packageJsons.Length == 1 && !recString.EndsWith(ESocketConst.SendPackageEndFlag)) continue;
                        recStringBuilder.Clear();
                        try
                        {
                            for (int i = 0, maxIndex = packageJsons.Length - 1; i <= maxIndex; i++)
                            {
                                var json = packageJsons[i];
                                //最后一个包不完整
                                if (maxIndex > 0 && i == maxIndex && !recString.EndsWith(ESocketConst.SendPackageEndFlag))
                                {
                                    recStringBuilder.Append(json);
                                    break;
                                }
                                //将字节数组转为Model
                                PackageModel model = json.ToPackageModel();
                                OnReceive(model);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        /// <summary>
        /// 监听心跳,是否正常连接
        /// </summary>
        private void ListenHeartbeatThread(LoopThread.Token token)
        {
            if (mSocket == null) return;
            var timeout = TimeSpan.FromSeconds(1);
            while (token.IsLooping)
            {
                var lastListenHeartbeatTime = LastListenHeartbeatTime;
                var interval = lastListenHeartbeatTime.GetDifferenceSeconds();
                //心跳超时
                if (interval >= ESocketConst.HeartbeatTimeout)
                {
                    Console.WriteLine("Heartbeat timeout...\nLastListenHeartbeatTime:{0}\tInterval:{1}", lastListenHeartbeatTime, interval);
                    //断开连接
                    ExceptionDisconnect();
                    return;
                }
                //隔一秒检测一次
                Thread.Sleep(timeout);
            }
        }
    }
}

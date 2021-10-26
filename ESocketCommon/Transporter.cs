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
        /// 监听消息的线程
        /// </summary>
        private LoopThread mReceiveThread;
        /// <summary>
        /// 发送消息的线程
        /// </summary>
        private LoopThread mSendThread;
        /// <summary>
        /// 检测心跳线程
        /// </summary>
        private LoopThread mListenHeartbeatThread;
        /// <summary>
        /// 接收缓冲区
        /// </summary>
        private readonly byte[] mRecBuffer = TransporterTool.GetReceiveBuffer();
        /// <summary>
        /// 接收到的字节
        /// </summary>
        private readonly List<byte> mRecDataList = new List<byte>();
        /// <summary>
        /// 当前需要接收的数据包的长度
        /// </summary>
        private int mCurDataLength;
        /// <summary>
        /// 发送消息缓冲区
        /// </summary>
        private readonly Queue<byte[]> mWaitSendDataQueue = new Queue<byte[]>();
        /// <summary>
        /// 发送等待器
        /// </summary>
        private readonly EventWaitHandle mSendWaiter = new AutoResetEvent(false);
        
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
            mReceiveThread?.Stop();
            mReceiveThread = null;
            mListenHeartbeatThread?.Stop();
            mListenHeartbeatThread = null;
            //关闭通信的套接字
            try
            {
                mSocket?.Shutdown(SocketShutdown.Both);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
            finally
            {
                mSocket?.Close();
            }
            mSocket = null;
            mRecDataList.Clear();
            mCurDataLength = 0;
            mWaitSendDataQueue.Clear();
            mSendWaiter.Close();
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
                Logger.LogError(e);
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
            mSendWaiter.Reset();
            //初始化接收消息线程
            InitReceiveThread();
            //初始化发送消息线程
            InitSendThread();
            //初始化心跳检测线程
            InitListenHeartbeatThread();
        }

        /// <summary>
        /// 发送请求
        /// </summary>
        /// <param name="parameters">参数列表</param>
        public void SendRequest(Dictionary<string, object> parameters)
        {
            WaitSend(PackageCode.Request, new OperationRequest(parameters));
        }

        /// <summary>
        /// 回复响应
        /// </summary>
        /// <param name="returnCode">返回码</param>
        /// <param name="parameters">参数列表</param>
        public void SendResponse(int returnCode, Dictionary<string, object> parameters)
        {
            WaitSend(PackageCode.Response, new OperationResponse(returnCode, parameters));
        }

        /// <summary>
        /// 发送心跳包
        /// </summary>
        protected void SendHeartbeat()
        {
            WaitSend(PackageCode.Heartbeat, null);
        }

        /// <summary>
        /// 发送连接信息
        /// </summary>
        /// <param name="connectCode">连接状态</param>
        protected void SendConnect(ConnectCode connectCode)
        {
            WaitSend(PackageCode.Connect, new OperationConnect(connectCode));
        }

        /// <summary>
        /// 加入发送缓冲区
        /// </summary>
        private void WaitSend(PackageCode packageCode, IOperation operation)
        {
            if (mSocket == null) return;
            lock (mWaitSendDataQueue)
            {
                mWaitSendDataQueue.Enqueue(new PackageModel(packageCode, operation).ToPackageBuffer());
            }
            //可以开始发送
            mSendWaiter.Set();
        }
        
        /// <summary>
        /// 发送消息
        /// </summary>
        private void Send(byte[] data)
        {
            if (mSocket == null) return;
            try
            {
                if (mSocket.Connected)
                    mSocket.Send(data);
                else
                    Logger.LogError("网络未连接");
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }

        /// <summary>
        /// 初始化接收消息的线程
        /// </summary>
        private void InitReceiveThread()
        {
            //接收消息的线程
            mReceiveThread = new LoopThread(ReceiveThread) {IsBackground = true};
            mReceiveThread.Start();
        }
        
        /// <summary>
        /// 初始化发送消息的线程
        /// </summary>
        private void InitSendThread()
        {
            //接收消息的线程
            mSendThread = new LoopThread(SendThread) {IsBackground = true};
            mSendThread.Start();
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
        private void ReceiveThread(LoopThread.Token token)
        {
            if (mSocket == null) return;
            while (token.IsLooping)
            {
                try
                {
                    int length;
                    try
                    {
                        //接收到的信息放入缓冲区，并获得字节数组长度
                        length = mSocket.Receive(mRecBuffer);
                    }
                    catch (Exception e)
                    {
                        //线程中断，直接返回
                        if(!token.IsLooping) return;
                        Logger.LogError("中断连接\nMessage:{0}\nStackTrace:{1}", e.Message, e.StackTrace);
                        ExceptionDisconnect();
                        return;
                    }
                    LastListenHeartbeatTime = TimeUtil.GetCurrentUtcTime();
                    for (int i = 0; i < length; i++)
                    {
                        mRecDataList.Add(mRecBuffer[i]);
                        //读取当前数据包总长度
                        if (mRecDataList.Count == ESocketConst.PackageLengthByteArrayLength)
                            mCurDataLength = TransporterTool.ConvertByteToInt(mRecDataList);
                        //当前数据包接收完毕
                        else if (mCurDataLength > 0 && mRecDataList.Count == mCurDataLength)
                        {
                            //接收到的完整数据json
                            string json = Encoding.UTF8.GetString(mRecDataList.ToArray(), ESocketConst.PackageLengthByteArrayLength,
                                mCurDataLength - ESocketConst.PackageLengthByteArrayLength);
                            mRecDataList.Clear();
                            mCurDataLength = 0;
                            //将字节数组转为Model
                            PackageModel model = json.ToPackageModel();
                            OnReceive(model);
                        }
                    }
                }
                catch(Exception e)
                {
                    Logger.LogError(e);
                }
            }
        }

        private void SendThread(LoopThread.Token token)
        {
            if (mSocket == null) return;
            while (token.IsLooping)
            {
                try
                {
                    if (mWaitSendDataQueue.Count > 0)
                        Send(mWaitSendDataQueue.Dequeue());
                    else
                        mSendWaiter.WaitOne();//没有数据了，等待
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                }
            }
        }

        /// <summary>
        /// 监听心跳,是否正常连接
        /// </summary>
        private void ListenHeartbeatThread(LoopThread.Token token)
        {
            if (mSocket == null) return;
            while (token.IsLooping)
            {
                var lastListenHeartbeatTime = LastListenHeartbeatTime;
                var interval = lastListenHeartbeatTime.GetDifferenceSeconds();
                //心跳超时
                if (interval >= ESocketConst.HeartbeatTimeout)
                {
                    Logger.LogError("Heartbeat timeout...\nLastListenHeartbeatTime:{0}\tInterval:{1}", lastListenHeartbeatTime, interval);
                    //断开连接
                    ExceptionDisconnect();
                    return;
                }
                //隔一秒检测一次
                Thread.Sleep(1000);
            }
        }
    }
}

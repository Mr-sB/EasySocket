using ESocket.Common;
using ESocket.Common.Tools;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ESocket.Client
{
    public sealed class ESocketPeer : Transporter
    {
        private IPeerListener mListener;
        /// <summary>
        /// 发送心跳的线程
        /// </summary>
        private LoopThread mSendHeartbeatThread;
        /// <summary>
        /// 连接状态
        /// </summary>
        public ConnectCode ConnectCode { private set; get; } = ConnectCode.Disconnect;

        public ESocketPeer(IPeerListener listener) : base(CreateSocket())
        {
            mListener = listener;
        }

        private static Socket CreateSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="serverIP">服务器IP</param>
        /// <param name="port">端口号</param>
        public void Connect(string serverIP, int port)
        {
            if (mSocket == null)
                mSocket = CreateSocket();
            mSocket.Connect(IPAddress.Parse(serverIP), port);
            Init();
        }

        /// <summary>
        /// 异常中断
        /// </summary>
        protected sealed override void ExceptionDisconnect()
        {
            base.ExceptionDisconnect();
            DisconnectInternal();
        }

        /// <summary>
        /// 主动断开连接
        /// </summary>
        public void Disconnect()
        {
            if (mSocket == null || ConnectCode == ConnectCode.Disconnect) return;
            //发送关闭连接信息
            SendConnect(ConnectCode.Disconnect);
            base.DisconnectInternal();
            DisconnectInternal();
        }

        /// <summary>
        /// 被动断开连接
        /// </summary>
        private void BeDisconnected()
        {
            base.DisconnectInternal();
            DisconnectInternal();
        }

        /// <summary>
        /// 在base.DisconnectInternal()之后执行
        /// </summary>
        private new void DisconnectInternal()
        {
            mSendHeartbeatThread?.Stop();
            mSendHeartbeatThread = null;
            ConnectCode = ConnectCode.Disconnect;
            mListener?.OnConnectStateChanged(ConnectCode.Disconnect);
        }

        /// <summary>
        /// 初始化发送心跳线程
        /// </summary>
        private void InitSendHeartbeatThread()
        {
            mSendHeartbeatThread = new LoopThread(SendHeartbeatThread) {IsBackground = true};
            mSendHeartbeatThread.Start();
        }

        /// <summary>
        /// 发送心跳,保持正常连接
        /// </summary>
        private void SendHeartbeatThread(LoopThread.Token token)
        {
            if (mSocket == null) return;
            while (token.IsLooping)
            {
                //发送心跳
                var interval = LastSendHeartbeatTime.GetDifferenceSeconds();
                if (interval >= ESocketConst.SendHeartbeatInterval)
                {
                    SendHeartbeat();
                }
                //隔一秒检测一次
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// 监听消息
        /// </summary>
        protected sealed override void OnReceive(PackageModel model)
        {
            if (mListener == null) return;

            switch (model.PackageCode)
            {
                case PackageCode.Connect:
                    var connectCode = model.GetOperationConnect().ConnectCode;
                    switch (connectCode)
                    {
                        case ConnectCode.Connect:
                            ConnectSuccess();
                            break;
                        case ConnectCode.Disconnect:
                            BeDisconnected();
                            break;
                    }
                    mListener.OnConnectStateChanged(connectCode);
                    break;
                case PackageCode.Request:
                    mListener.OnOperationRequest(model.GetOperationRequest());
                    break;
                case PackageCode.Response:
                    mListener.OnOperationResponse(model.GetOperationResponse());
                    break;
            }
        }

        /// <summary>
        /// 连接成功，开始初始化
        /// </summary>
        private void ConnectSuccess()
        {
            ConnectCode = ConnectCode.Connect;
            InitSendHeartbeatThread();
        }
    }
}

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
        private Thread mSendHeartbeatThread;
        /// <summary>
        /// 发送心跳的线程是否停止,所有线程中数据同步
        /// </summary>
        private volatile bool mIsSendHeartbeatThreadStop;
        /// <summary>
        /// 连接状态
        /// </summary>
        public ConnectCode ConnectCode { private set; get; } = ConnectCode.Disconnect;

        public ESocketPeer(IPeerListener listener) : base(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
        {
            mListener = listener;
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="serverIP">服务器IP</param>
        /// <param name="port">端口号</param>
        public void Connect(string serverIP, int port)
        {
            mSocket.Connect(IPAddress.Parse(serverIP), port);
            Init();
        }

        /// <summary>
        /// 异常中断
        /// </summary>
        protected override void ExceptionDisconnect()
        {
            DisconnectInternal();
            base.ExceptionDisconnect();
        }

        /// <summary>
        /// 主动断开连接
        /// </summary>
        public void Disconnect()
        {
            if (mSocket == null || ConnectCode == ConnectCode.Disconnect) return;
            //发送关闭连接信息
            SendConnect(ConnectCode.Disconnect);
            DisconnectInternal();
            base.DisconnectInternal();
        }

        /// <summary>
        /// 被动断开连接
        /// </summary>
        private void BeDisconnected()
        {
            DisconnectInternal();
            base.DisconnectInternal();
        }

        private new void DisconnectInternal()
        {
            mListener?.OnConnectStateChanged(ConnectCode.Disconnect);
            mIsSendHeartbeatThreadStop = true;
            mSendHeartbeatThread = null;
        }

        /// <summary>
        /// 初始化发送心跳线程
        /// </summary>
        private void InitSendHeartbeatThread()
        {
            mSendHeartbeatThread = new Thread(SendHeartbeatThread) {IsBackground = true};
            mSendHeartbeatThread.Start();
        }

        /// <summary>
        /// 发送心跳,保持正常连接
        /// </summary>
        private void SendHeartbeatThread()
        {
            if (mSocket == null) return;
            while (!mIsSendHeartbeatThreadStop)
            {
                //发送心跳
                var interval = LastSendHeartbeatTime.GetDifferenceSeconds();
                if (interval >= ESocketConst.SendHeartbeatInterval)
                {
                    SendHeartbeat();
                }
                //隔一秒检测一次
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        /// <summary>
        /// 监听消息
        /// </summary>
        protected override void OnReceive(PackageModel model)
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

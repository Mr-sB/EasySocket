using ESocket.Common;
using System;
using System.Net.Sockets;

namespace ESocket.Server
{
    /// <summary>
    /// 客户类
    /// </summary>
    public abstract class ClientPeer : Transporter
    {
        protected virtual void OnDisconnect() { }
        protected abstract void OnOperationRequest(OperationRequest request);
        protected virtual void OnOperationResponse(OperationResponse response) { }

        public ClientPeer(Socket socket) : base(socket)
        {
            Init();
            //连接成功
            SendConnect(ConnectCode.Connect);
        }

        /// <summary>
        /// 异常中断
        /// </summary>
        protected sealed override void ExceptionDisconnect()
        {
            DisconnectInternal();
            base.ExceptionDisconnect();
        }

        /// <summary>
        /// 主动断开连接
        /// </summary>
        public void Disconnect()
        {
            DisconnectInternal();
            //发送关闭连接信息
            SendConnect(ConnectCode.Disconnect);
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
            Console.WriteLine("客户端{0}中断连接", ToString());
            OnDisconnect();
            ApplicationBase.Instance.OnDisconnect(this);
        }

        /// <summary>
        /// 监听消息
        /// </summary>
        protected sealed override void OnReceive(PackageModel model)
        {
            switch (model.PackageCode)
            {
                case PackageCode.Connect:
                    var connectCode = model.GetOperationConnect().ConnectCode;
                    switch (connectCode)
                    {
                        case ConnectCode.Disconnect:
                            BeDisconnected();
                            break;
                    }
                    break;
                case PackageCode.Heartbeat:
                    //回应心跳包
                    SendHeartbeat();
                    break;
                case PackageCode.Request:
                    OnOperationRequest(model.GetOperationRequest());
                    break;
                case PackageCode.Response:
                    OnOperationResponse(model.GetOperationResponse());
                    break;
            }
        }
    }
}

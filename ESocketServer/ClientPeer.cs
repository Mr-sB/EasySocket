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
        /// 断开连接
        /// </summary>
        public sealed override void Disconnect()
        {
            Console.WriteLine("客户端{0}中断连接", ToString());
            OnDisconnect();
            ApplicationBase.Instance.OnDisconnect(this);
            //发送关闭连接信息
            SendConnect(ConnectCode.Disconnect);
            base.Disconnect();
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
                            Disconnect();
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

using ESocket.Common;
using ESocket.Common.Tools;
using ESocket.Server;
using System;
using System.Net.Sockets;

namespace ESocket.Example.Server
{
    public class ExampleClientPeer : ClientPeer
    {
        public ExampleClientPeer(Socket socket) : base(socket) { }

        protected override void OnOperationRequest(OperationRequest request)
        {
            if (request.Parameters.TryGetParameter<string>("", out var value))
            {
                Console.WriteLine("OnOperationRequest:{0}", value);
                SendResponse(1, ESocketParameterTool.NewParameters.AddParameter("", "服务器收到消息:" + value));
            }
            else
                Console.WriteLine("OnOperationRequest 参数错误");
        }

        protected override void OnOperationResponse(OperationResponse response)
        {
            Console.WriteLine("OnOperationResponse");
        }
    }
}

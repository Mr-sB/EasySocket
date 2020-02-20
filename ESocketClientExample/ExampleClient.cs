using System;
using ESocket.Client;
using ESocket.Common;
using ESocket.Common.Tools;

namespace ESocket.Example.Client
{
    public class ExampleClient : IPeerListener
    {
        static void Main(string[] args)
        {
            //在Connect之前可以初始化一些信息
            ESocketConst.ReceiveBufferSize = 1024 * 1024;
            //创建peer
            var peer = new ESocketPeer(new ExampleClient());
            //连接
            peer.Connect("127.0.0.1", 5000);
            while (true)
            {
                //发消息
                peer.SendRequest(ESocketParameterTool.NewParameters.AddParameter("", Console.ReadLine()));
            }
        }

        public void OnConnectStateChanged(ConnectCode connectCode)
        {
            Console.WriteLine("OnConnectStateChanged:{0}", connectCode);
        }

        public void OnOperationRequest(OperationRequest request)
        {
            Console.WriteLine("OnOperationRequest");
        }

        public void OnOperationResponse(OperationResponse response)
        {
            if (response.Parameters.TryGetParameter<string>("", out var value))
                Console.WriteLine("OnOperationResponse:{0}_{1}", response.ReturnCode, value);
            else
                Console.WriteLine("OnOperationResponse 参数错误");
        }
    }
}

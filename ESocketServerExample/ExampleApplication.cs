using ESocket.Server;
using System;
using System.Net.Sockets;

namespace ESocket.Example.Server
{
    public sealed class ExampleApplication : ApplicationBase
    {
        public ExampleApplication(Config config) : base(config) { }

        protected override void Setup()
        {
            //在此处可以初始化一些信息
            ESocket.Common.ESocketConst.ReceiveBufferSize = 1024 * 1024;
        }

        protected override ClientPeer CreatePeer(Socket clientSocket)
        {
            Console.WriteLine("Client connect:{0}", clientSocket.RemoteEndPoint.ToString());
            return new ExampleClientPeer(clientSocket);
        }
    }
}

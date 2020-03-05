using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ESocket.Server
{
    /// <summary>
    /// 继承该对象作为服务器程序入口
    /// 在项目根目录下创建ESocketConfig.json配置信息
    /// </summary>
    public abstract class ApplicationBase
    {
        /// <summary>
        /// 单例
        /// </summary>
        public static ApplicationBase Instance { private set; get; }
        /// <summary>
        /// 配置信息
        /// </summary>
        private Config mConfig;
        /// <summary>
        /// 服务端套接字
        /// </summary>
        private Socket mServerSocket;
        /// <summary>
        /// 已连接的客户端套接字
        /// </summary>
        private Dictionary<string, ClientPeer> mConnectedClients;

        /// <summary>
        /// 服务器启动的配置
        /// </summary>
        protected virtual void Setup() { }
        /// <summary>
        /// 服务器关闭
        /// </summary>
        protected virtual void OnTearDown() { }
        /// <summary>
        /// 客户端连接成功
        /// </summary>
        /// <param name="clientSocket">与客户端通信的套接字</param>
        protected abstract ClientPeer CreatePeer(Socket clientSocket);

        public ApplicationBase(Config config)
        {
            Instance = this;
            mConfig = config;
        }

        public void Start()
        {
            //配置
            Setup();
            //初始化套接字
            InitSocket();
            //初始化线程
            InitConnectThread();
        }

        /// <summary>
        /// 初始化套接字
        /// </summary>
        private void InitSocket()
        {
            mServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //绑定
            mServerSocket.Bind(new IPEndPoint(IPAddress.Parse(mConfig.ConnectConfig.ServerIP), mConfig.ConnectConfig.Port));
            //监听，最大队列程度20
            mServerSocket.Listen(20);
        }

        /// <summary>
        /// 初始化线程
        /// </summary>
        private void InitConnectThread()
        {
            mConnectedClients = new Dictionary<string, ClientPeer>();
            //监听连接请求的线程
            Thread connectThread = new Thread(ListenConnectThread);
            //后台线程，不会阻止应用结束(随着应用结束而结束)
            connectThread.IsBackground = true;
            connectThread.Start();
        }

        //监听连接请求
        private void ListenConnectThread()
        {
            Socket connectSocket;
            while (true)
            {
                try
                {
                    connectSocket = null;
                    //接收连接 (接收到客户端调用Socket.Connect)
                    connectSocket = mServerSocket.Accept();
                }
                catch (Exception e)
                {
                    //套接字监听异常
                    Console.WriteLine(e.Message);
                    return;
                }

                //获取客户端ip地址和端口号
                EndPoint clientEndPoint = connectSocket.RemoteEndPoint;
                if (clientEndPoint == null) continue;
                //旧的peer移除
                if (mConnectedClients.TryGetValue(clientEndPoint.ToString(), out var oldPeer))
                    oldPeer.Disconnect();
                //创建ClientPeer
                ClientPeer clientPeer = CreatePeer(connectSocket);
                //添加客户端信息   
                mConnectedClients.Add(clientEndPoint.ToString(), clientPeer);
            }
        }        

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="clientPeer"></param>
        internal void OnDisconnect(ClientPeer clientPeer)
        {
            string address = clientPeer.ToString();
            if (mConnectedClients.ContainsKey(address))
                mConnectedClients.Remove(address);
        }
    }
}

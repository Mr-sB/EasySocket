using System;
using System.Threading;

namespace ESocket.Common.Tools
{
    public class LoopThread
    {
        public class Token
        {
            /// <summary>
            /// 是否在循环运行.所有线程中数据同步
            /// </summary>
            internal volatile bool mIsLooping = true;
            public bool IsLooping => mIsLooping;
        }
        
        private Thread mThread;
        /// <summary>
        /// 所有线程中数据同步
        /// </summary>
        private volatile Token mLoopToken;
        private bool mInit;
        private bool mHasParameter;

        public bool IsBackground
        {
            get => mThread != null && mThread.IsBackground;
            set
            {
                if (mThread != null)
                    mThread.IsBackground = value;
            }
        }

        /// <summary>
        /// 无参数线程
        /// </summary>
        public LoopThread(Action<Token> start)
        {
            if (start == null)
            {
                mInit = false;
                return;
            }
            mInit = true;
            mLoopToken = new Token();
            mHasParameter = false;
            mThread = new Thread(() => { start(mLoopToken); });
        }
        
        /// <summary>
        /// 带参数线程
        /// </summary>
        public LoopThread(Action<Token, object> start)
        {
            if (start == null)
            {
                mInit = false;
                return;
            }
            mInit = true;
            mLoopToken = new Token();
            mHasParameter = true;
            mThread = new Thread( obj => { start(mLoopToken, obj); });
        }

        public void Start()
        {
            if(!mInit) return;
            mLoopToken.mIsLooping = true;
            if(!mHasParameter)
                mThread.Start();
            else
                mThread.Start(null);
        }
        
        public void Start(object obj)
        {
            if(!mInit) return;
            mLoopToken.mIsLooping = true;
            if(!mHasParameter)
                mThread.Start();
            else
                mThread.Start(obj);
        }
        
        public void Stop()
        {
            if(!mInit) return;
            mLoopToken.mIsLooping = false;
        }
    }
}
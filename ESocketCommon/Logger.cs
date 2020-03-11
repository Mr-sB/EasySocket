using System;

namespace ESocket.Common
{
    public static class Logger
    {
        private static Action<string> mLogger = Console.WriteLine;
        private static Action<string> mErrorLogger = Console.WriteLine;

        /// <summary>
        /// To runtime toggle Logger logging [ON/OFF].
        /// </summary>
        public static bool LogEnabled = true;
        
        /// <summary>
        /// Set log action
        /// </summary>
        public static void SetLogger(Action<string> logger, Action<string> errorLogger)
        {
            if (logger != null)
                mLogger = logger;
            if (errorLogger != null)
                mErrorLogger = errorLogger;
        }

        /// <summary>
        /// Log
        /// </summary>
        public static void Log(string value)
        {
            if(!LogEnabled) return;
            mLogger?.Invoke(value);
        }
        
        /// <summary>
        /// Log
        /// </summary>
        public static void Log(string format, object arg)
        {
            if(!LogEnabled) return;
            mLogger?.Invoke(string.Format(format, arg));
        }
        
        /// <summary>
        /// Log
        /// </summary>
        public static void Log(string format, params object[] args)
        {
            if(!LogEnabled) return;
            if(args == null || args.Length <= 0)
                mLogger?.Invoke(format);
            else
                mLogger?.Invoke(string.Format(format, args));
        }
        
        /// <summary>
        /// Log
        /// </summary>
        public static void Log(object value)
        {
            if(!LogEnabled) return;
            mLogger?.Invoke(value == null ? "null" : value.ToString());
        }

        /// <summary>
        /// Error log
        /// </summary>
        public static void LogError(string value)
        {
            if(!LogEnabled) return;
            mErrorLogger?.Invoke(value);
        }
        
        /// <summary>
        /// Error log
        /// </summary>
        public static void LogError(string format, object arg)
        {
            if(!LogEnabled) return;
            mErrorLogger?.Invoke(string.Format(format, arg));
        }
        
        /// <summary>
        /// Error log
        /// </summary>
        public static void LogError(string format, params object[] args)
        {
            if(!LogEnabled) return;
            if(args == null || args.Length <= 0)
                mErrorLogger?.Invoke(format);
            else
                mErrorLogger?.Invoke(string.Format(format, args));
        }
        
        /// <summary>
        /// Error log
        /// </summary>
        public static void LogError(object value)
        {
            if(!LogEnabled) return;
            mErrorLogger?.Invoke(value == null ? "null" : value.ToString());
        }
    }
}
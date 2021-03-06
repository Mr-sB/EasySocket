﻿using System;
using System.IO;
using ESocket.Common;
using ESocket.Common.Tools;

namespace ESocket.Server
{
    public class Config
    {
        /// <summary>
        /// 应用程序入口配置
        /// </summary>
        public ApplicationConfig ApplicationConfig;
        /// <summary>
        /// Socket连接配置
        /// </summary>
        public ConnectConfig ConnectConfig;

        public static string GetConfigRelativePath()
        {
            return "./ESocketConfig.json";
        }

        public static Config LoadConfig()
        {
            string configRelativePath = GetConfigRelativePath();
            if (!File.Exists(configRelativePath))
            {
                Logger.LogError("{0} does not exit!", configRelativePath);
                return null;
            }
            try
            {
                using (StreamReader streamReader = new StreamReader(configRelativePath))
                {
                    return streamReader.ReadToEnd().ToObject<Config>();
                }
            }
            catch(Exception e)
            {
                Logger.LogError(e);
                return null;
            }
        }
    }

    public class ApplicationConfig
    {
        public string BaseDirectory;
        /// <summary>
        /// 程序集文件名称
        /// </summary>
        public string AssemblyFileName;
        /// <summary>
        /// 入口类 命名空间.类名
        /// </summary>
        public string TypeWithNamespace;
    }

    public class ConnectConfig
    {
        /// <summary>
        /// 服务器私网IP
        /// </summary>
        public string ServerIP;
        /// <summary>
        /// 端口号
        /// </summary>
        public int Port;
    }
}

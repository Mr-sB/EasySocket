using System;
using System.Collections.Generic;

namespace ESocket.Common.Tools
{
    /// <summary>
    /// 参数工具类
    /// </summary>
    public static class ESocketParameterTool
    {
        /// <summary>
        /// 获得一个新的参数列表
        /// </summary>
        public static Dictionary<string, object> NewParameters => new Dictionary<string, object>();
        
        /// <summary>
        /// 往参数列表添加参数
        /// </summary>
        /// <param name="parameters">参数列表</param>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <typeparam name="T">value类型</typeparam>
        /// <returns>参数列表</returns>
        public static Dictionary<string, object> AddParameter<T>(this Dictionary<string, object> parameters, string key, T value)
        {
            object objValue = value;
            if (parameters == null)
                Console.WriteLine("parameters is null");
            else if (parameters.ContainsKey(key))
                Console.WriteLine("parameters already contains parameter key:{0}", key);
            else
                parameters.Add(key, typeof(T).NeedMap() ? objValue.ToJson() : objValue);
            return parameters;
        }

        /// /// <summary>
        /// 从参数列表中获取参数
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="parameters">参数列表</param>
        /// <param name="key">key</param>
        /// <param name="parameter">参数</param>
        /// <returns>是否成功</returns>
        public static bool TryGetParameter<T>(this Dictionary<string, object> parameters, string key, out T parameter)
        {
            parameter = default(T);
            if (parameters == null)
            {
                Console.WriteLine("parameters is null");
                return false;
            }
            if (parameters.TryGetValue(key, out var value))
            {
                parameter = typeof(T).NeedMap() ? ((string)value).ToObject<T>() : (T)value;
                return true;
            }
            Console.WriteLine("parameters does not contains parameter key:{0}", key);
            return false;
        }
    }
}

using LitJson;
using System;
using System.Collections.Generic;
using System.Text;

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
        /// <returns>参数列表</returns>
        public static Dictionary<string, object> AddParameter(this Dictionary<string, object> parameters, string key, object value)
        {
            if (parameters == null)
                Console.WriteLine("parameters is null");
            else if (parameters.ContainsKey(key))
                Console.WriteLine("parameters already contains parameter code:", key);
            else
                parameters.Add(key, value.GetType().NeedMap() ? JsonMapper.ToJson(value) : value);
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
                parameter = typeof(T).NeedMap() ? JsonMapper.ToObject<T>((string)value) : (T)value;
                return true;
            }
            Console.WriteLine("parameters does not contains parameter code:", key);
            return false;
        }

        /// <summary>
        /// Parameter转字节数组
        /// </summary>
        /// <returns>字节数组</returns>
        public static byte[] ToBuffer(this Dictionary<string, object> parameters)
        {
            if (parameters == null) return null;
            return Encoding.UTF8.GetBytes(JsonMapper.ToJson(parameters));
        }

        /// <summary>
        /// 给定数据类型的数据是否需要转换为json
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool NeedMap(this Type type)
        {
            if (type == null) return false;
            return type.IsClass && type != typeof(string);
        }
    }
}

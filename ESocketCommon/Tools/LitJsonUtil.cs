using System;
using LitJson;

namespace ESocket.Common.Tools
{
    public static class LitJsonUtil
    {
        /// <summary>
        /// ToJson(null)时返回的值
        /// </summary>
        public const string NULL_JSON = "null";
        
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

        /// <summary>
        /// 对象转为json
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns>json</returns>
        public static string ToJson(this object obj)
        {
            try
            {
                return obj == null ? NULL_JSON : JsonMapper.ToJson(obj);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                return string.Empty;
            }
        }

        /// <summary>
        /// json转为对象
        /// </summary>
        /// <param name="json">json</param>
        /// <typeparam name="T">对象类型</typeparam>
        /// <returns>对象</returns>
        public static T ToObject<T>(this string json)
        {
            if (json == NULL_JSON) return default(T);
            try
            {
                return JsonMapper.ToObject<T>(json);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                return default(T);
            }
        }
    }
}
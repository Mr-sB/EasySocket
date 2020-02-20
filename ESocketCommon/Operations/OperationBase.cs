using System.Collections.Generic;

namespace ESocket.Common
{
    /// <summary>
    /// 操作信息基类
    /// </summary>
    public abstract class OperationBase : IOperation
    {
        /// <summary>
        /// 参数列表,value只支持LitJson能够解析的类型
        /// </summary>
        public Dictionary<string, object> Parameters;

        /// <summary>
        /// 无参构造函数
        /// </summary>
        public OperationBase() { }
        /// <summary>
        /// 有参构造函数
        /// </summary>
        /// <param name="parameters"></param>
        public OperationBase(Dictionary<string, object> parameters)
        {
            Parameters = parameters;
        }
    }
}

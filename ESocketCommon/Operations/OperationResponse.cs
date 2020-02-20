using System.Collections.Generic;

namespace ESocket.Common
{
    public class OperationResponse : OperationBase
    {
        /// <summary>
        /// 返回码，代表返回状态
        /// </summary>
        public int ReturnCode;

        public OperationResponse() : base() { }
        public OperationResponse(int returnCode, Dictionary<string, object> parameter) : base(parameter)
        {
            ReturnCode = returnCode;
        }
    }
}

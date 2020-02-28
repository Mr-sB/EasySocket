using ESocket.Common.Tools;

namespace ESocket.Common
{
    public class PackageModel
    {
        /// <summary>
        /// 包码
        /// </summary>
        public PackageCode PackageCode;
        /// <summary>
        /// 参数json字符串
        /// </summary>
        public string operationJson;

        public PackageModel() { }
        public PackageModel(PackageCode packageCode, IOperation operation)
        {
            PackageCode = packageCode;
            if (operation != null)
                operationJson = operation.ToJson();
        }

        public OperationConnect GetOperationConnect()
        {
            if (PackageCode != PackageCode.Connect) return null;
            return operationJson.ToObject<OperationConnect>();
        }

        public OperationRequest GetOperationRequest()
        {
            if (PackageCode != PackageCode.Request) return null;
            return operationJson.ToObject<OperationRequest>();
        }

        public OperationResponse GetOperationResponse()
        {
            if (PackageCode != PackageCode.Response) return null;
            return operationJson.ToObject<OperationResponse>();
        }
    }
}

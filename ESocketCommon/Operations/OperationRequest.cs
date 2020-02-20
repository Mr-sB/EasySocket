using System.Collections.Generic;

namespace ESocket.Common
{
    public class OperationRequest : OperationBase
    {
        public OperationRequest() : base() { }

        public OperationRequest(Dictionary<string, object> parameter) : base(parameter) { }
    }
}

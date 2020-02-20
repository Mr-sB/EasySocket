using ESocket.Common;

namespace ESocket.Client
{
    public interface IPeerListener
    {
        void OnConnectStateChanged(ConnectCode connectCode);
        void OnOperationRequest(OperationRequest request);
        void OnOperationResponse(OperationResponse response);
    }
}

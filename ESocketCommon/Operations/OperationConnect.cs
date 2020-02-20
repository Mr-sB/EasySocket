namespace ESocket.Common
{
    public class OperationConnect : IOperation
    {
        public ConnectCode ConnectCode;

        public OperationConnect() { }
        public OperationConnect(ConnectCode connectCode)
        {
            ConnectCode = connectCode;
        }
    }
}

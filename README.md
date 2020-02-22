# EasySocket
Easy Network communication. No need to care about socket, just use the API, and you can easily done Network communication.

## Feather
* Package socket conmunication, and you do not need to care about socket.
* Provide server and client API, all you need is inherit the base class, and you can easily done network code.
* Use LitJson to serialize/deserialize data inside, just put the data in send function, no need to serialize/deserialize again.

## Usage
### Server
* Create a library project, add `ESocketCommon.dll`, `ESocketServer.dll`, `LitJSON.dll`.
* Inherit `ApplicationBase` class as your server application.
* You have to implement `CreatePeer` function to create a client peer.
```c#
protected abstract ClientPeer CreatePeer(Socket clientSocket);
```
* You can override these functions to listen server application's status.
```c#
protected virtual void Setup() { }
protected virtual void OnTearDown() { }
```
* You can setup config in `Setup` function, such as `ESocketConst.ReceiveBufferSize`, `ESocketConst.SendHeartbeatInterval`, `ESocketConst.HeartbeatTimeout`.
* Inherit `ClientPeer` class to take charge of conmunicating with client.
* You have to implement `OnOperationRequest` function to handle client's messages.
```c#
protected abstract void OnOperationRequest(OperationRequest request);
```
* You can override these functions to listen more actions.
```c#
protected virtual void OnDisconnect() { }
protected virtual void OnOperationResponse(OperationResponse response) { }
```
* You can use these functions to send messages to client.
```c#
public void SendRequest(Dictionary<string, object> parameters);
public void SendResponse(int returnCode, Dictionary<string, object> parameters);
```
* Create a `ESocketConfig.json` file in your project, you can set some config in there.
```json
{
  "ApplicationConfig": {
    "BaseDirectory": "Example",
    "AssemblyFileName": "ESocketServerExample",
    "TypeWithNamespace": "ESocket.Example.Server.ExampleApplication"
  },
  "ConnectConfig": {
    "ServerIP": "127.0.0.1",
    "Port": 5000
  }
}
```
`ApplicationConfig` tells where your server dll is.
* When your code is done, generate your project and put your server dll in `ESocket-OnPremise-Server-SDK/%BaseDirectory` path.
* Copy `ESocketConfig.json` to `ESocket-OnPremise-Server-SDK/bin_Launcher`, then launch `ESocket-OnPremise-Server-SDK/bin_Launcher/ESocketServerLauncher.exe`, it will startup your server application.
### Client
* Add `ESocketCommon.dll`, `ESocketClient.dll`, `LitJSON.dll` to your client project.
* Inherit `IPeerListener` interface to take charge of conmunicating with server.
* You have to implement these functions to handle server's messages.
```c#
void OnConnectStateChanged(ConnectCode connectCode);
void OnOperationRequest(OperationRequest request);
void OnOperationResponse(OperationResponse response);
```
* Create a `ESocketPeer` to conect to server and listen server's messages.
```c#
var peer = new ESocketPeer(%IPeerListener);
//Connect
peer.Connect("127.0.0.1", 5000);
```
* You can use these functions to send messages to server.
```c#
public void SendRequest(Dictionary<string, object> parameters);
public void SendResponse(int returnCode, Dictionary<string, object> parameters);
```
## Tools
* There are some tools in `ESocketParameterTool` class, which can help you handle parameters.
```c#
public static Dictionary<string, object> NewParameters => new Dictionary<string, object>();
public static Dictionary<string, object> AddParameter(this Dictionary<string, object> parameters, string key, object value);
public static bool TryGetParameter<T>(this Dictionary<string, object> parameters, string key, out T parameter);
```
## Example
* There is an example server application in `ESocket-OnPremise-Server-SDK/Example`, and an example client application in `ESocket-OnPremise-Server-SDK/Example/Client`.
## Thanks
* Thanks for LitJSON/litjson to provide Json serialize/deserialize functions. https://github.com/LitJSON/litjson

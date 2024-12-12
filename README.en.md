# RequestBuilder

RequestBuilder is a framework for creating REST API requests. Based on the Builder pattern, it minimizes duplicate code that occurs during the request creation process.

## Features

- Minimizes duplicate code in request creation process
- Design based on Builder pattern

## Requirements

### Required Libraries
- UniTask

### Scripting Define Symbols
- UNITASK_SUPPORT
- JSON_SUPPORT

## Getting Started

### Usage Example

1. Define a static class for REST APIs.
2. Define a BaseRequest containing BaseUrl and RequestHeader information.
3. Define a builder for creating requests by appending additional information to the predefined BaseRequest.

```csharp
public static class RestAPI
{
    static Request.RequestBuilder BaseRequest =>
        Request.Builder("http://123.456.7.89:1234")
            .AddRequestHeader(("Content-Type", "application/json"));
    
    public static Request GetUserInfoRequest(string id) => BaseRequest.SetPath($"user/{id}").ToGetRequest();
    public static readonly Request GetScenarioPreset = BaseRequest.SetPath("scenario/preset").ToGetRequest();
    public static readonly Request PostLogInRequest = BaseRequest.SetPath("auth/login").ToPostRequest();
    public static readonly Request PostRegisterMemberRequest = BaseRequest.SetPath("auth/register/member").ToPostRequest();
}
```

4. Request usage example:

```csharp
// When you want to send a request
Packet userInfoPacket = await RestAPI.GetUserInfoRequest(id).SendAsync().GetPacketAsync();

if(userInfoPacket.result == UnityWebRequest.Result.Success) 
    Debug.Log(JToken.Parse(userInfoPacket.body)); // Success
else if(userInfoPacket.result == UnityWebRequest.Result.ProtocolError)
    Debug.Log("Account does not exist.");
else if(userInfoPacket.result == UnityWebRequest.Result.ConnectionError)
    Debug.Log("Network connection is lost.");
else 
    Debug.Log($"{userInfoPacket.result}({userInfoPacket.responseCode}) : {userInfoPacket.error}");
```

When you call SendAsync on the pre-created RequestBuilder, the Builder automatically creates and sends a new Request. If needed, you can map the response result to a Packet struct by calling GetPacketAsync.

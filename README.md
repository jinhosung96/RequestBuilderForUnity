# RequestBuilder

RequestBuilder는 Rest API에 대한 Request 생성 프레임워크입니다. Builder 패턴을 기반으로 설계되어 Request 생성 과정에서 발생하는 중복 코드를 최소화합니다.

## 특징

- Request 생성 과정에서 발생하는 중복 코드 최소화
- Builder 패턴 기반 설계

## 요구 사항

### 함께 사용된 라이브러리
- UniTask

### Scripting Define Symbols
- UNITASK_SUPPORT
- JSON_SUPPORT

## 설치 방법

### Package Manager를 통한 설치

Unity 2019.3.4f1 이상 버전에서는 Package Manager에서 직접 Git URL을 통해 설치할 수 있습니다.

1. Package Manager 창을 엽니다 (Window > Package Manager)
2. '+' 버튼을 클릭하고 "Add package from git URL"을 선택합니다
3. 다음 URL을 입력합니다:
```
https://github.com/jinhosung96/RequestBuilderForUnity.git
```

또는 `Packages/manifest.json` 파일에 직접 추가할 수 있습니다:
```json
{
  "dependencies": {
    "com.jhs-library.auto-path-generator": "https://github.com/jinhosung96/RequestBuilderForUnity.git"
  }
}
```

특정 버전을 설치하려면 URL 뒤에 #{version} 태그를 추가하면 됩니다:
```
https://github.com/jinhosung96/RequestBuilderForUnity.git#1.0.0
```

## 시작하기

### 사용 예시

1. Rest API들에 대한 정적 클래스를 정의합니다.
2. BaseUrl과 RequestHeader에 대한 정보를 포함한 BaseRequest를 정의합니다.
3. 미리 정의한 BaseRequest에 추가 정보를 이어 붙여 Request 생성할 빌더를 정의합니다.

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

4. Request 사용 예시:

```csharp
// Request를 보내고 싶을 때
Packet userInfoPacket = await RestAPI.GetUserInfoRequest(id).SendAsync().GetPacketAsync();

if(userInfoPacket.result == UnityWebRequest.Result.Success) 
    Debug.Log(JToken.Parse(userInfoPacket.body)); // 성공
else if(userInfoPacket.result == UnityWebRequest.Result.ProtocolError)
    Debug.Log("존재하지 않는 계정입니다.");
else if(userInfoPacket.result == UnityWebRequest.Result.ConnectionError)
    Debug.Log("네트워크 연결이 끊겨있습니다.");
else 
    Debug.Log($"{userInfoPacket.result}({userInfoPacket.responseCode}) : {userInfoPacket.error}");
```

미리 생성해둔 RequestBuilder에 SendAsync를 호출하면 Builder가 새로운 Request를 자동으로 생성하여 보냅니다. 필요에 따라 GetPacketAsync를 호출하는 것으로 응답 결과를 Packet 구조체로 매핑하여 받아올 수 있습니다.

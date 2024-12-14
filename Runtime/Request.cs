#if UNITASK_SUPPORT && JSON_SUPPORT
using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace JHS.Library.RequestBuilder.Runtime
{
    public struct Packet
    {
        public UnityWebRequest.Result result;
        public long responseCode;
        public string error;
        public string body;

        public Packet(UnityWebRequest.Result result, long responseCode, string error, string body)
        {
            this.result = result;
            this.responseCode = responseCode;
            this.error = error;
            this.body = body;
        }
    }
    
    public static class RequestExtensions
    {
        public static async UniTask<Packet> GetPacketAsync(this UniTask<UnityWebRequest> getRequestAsync)
        {
            try
            {
                using var request = await UniTask.RunOnThreadPool(async () => await getRequestAsync);
                
                if (request.result != UnityWebRequest.Result.Success) return new(request.result, request.responseCode, request.error, null);
                return new(request.result, request.responseCode, request.error, request.downloadHandler.text);
            }
            catch (Exception e)
            {
                Debug.Log(e);
                throw;
            }
        }
    }
    
    public class Request
    {
        #region Inner Classes

        public static RequestBuilder Builder(string baseUrl) => new(baseUrl);

        public class RequestBuilder
        {
            readonly string baseUrl;
            string path;
            readonly Dictionary<string, string> requestHeaders = new();

            public RequestBuilder(string baseUrl) => this.baseUrl = baseUrl;

            public RequestBuilder AddRequestHeader((string name, string value) requestHeader)
            {
                requestHeaders[requestHeader.name] = requestHeader.value;
                return this;
            }

            public RequestBuilder SetPath(string path)
            {
                this.path = path;
                return this;
            }

            public Request ToGetRequest() => new(baseUrl, path, requestHeaders, "GET");
            public Request ToPostRequest() => new(baseUrl, path, requestHeaders, "POST");
            public Request ToPutRequest() => new(baseUrl, path, requestHeaders, "PUT");
            public Request ToDeleteRequest() => new(baseUrl, path, requestHeaders, "DELETE");
        }

        #endregion

        readonly string baseUrl;
        readonly string path;
        readonly Dictionary<string, string> requestHeaders = new();
        readonly string method;

        protected Request(string baseUrl, string path, Dictionary<string, string> requestHeaders, string method)
        {
            this.baseUrl = baseUrl;
            this.path = path;
            this.requestHeaders = requestHeaders;
            this.method = method;
        }
        
        public async UniTask<UnityWebRequest> SendAsync() => await SendAsync(new DownloadHandlerBuffer());
        public async UniTask<UnityWebRequest> SendAsync(string body)
        {
            requestHeaders["Content-Type"] = "text/plain";
            return await SendAsync(new DownloadHandlerBuffer(), new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)));
        }
        public async UniTask<UnityWebRequest> SendAsync<TPacket>(TPacket packet) where TPacket : struct => await SendAsync(JToken.FromObject(packet));
        public async UniTask<UnityWebRequest> SendAsync(JToken body)
        {
            requestHeaders["Content-Type"] = "application/json";
            return await SendAsync(new DownloadHandlerBuffer(), new UploadHandlerRaw(Encoding.UTF8.GetBytes(body.ToString())));
        }
        public async UniTask<UnityWebRequest> SendAsync(DownloadHandler downloadHandler, UploadHandler uploadHandler = default) => 
            await SendAsync(new UnityWebRequest($"{baseUrl}/{path}", method) { uploadHandler = uploadHandler, downloadHandler = downloadHandler });

        protected async UniTask<UnityWebRequest> SendAsync(UnityWebRequest request)
        {
            Debug.Log($"Send : {request.url}");

            foreach (var requestHeader in requestHeaders) request.SetRequestHeader(requestHeader.Key, requestHeader.Value);

            var operation = request.SendWebRequest();
            while (!operation.isDone) await UniTask.Yield();
            
            if (request.result != UnityWebRequest.Result.Success) Debug.Log($"{request.result} : {request.error}");
            else if(!string.IsNullOrWhiteSpace(request.downloadHandler.text)) Debug.Log(JToken.Parse(request.downloadHandler.text));
            
            return request;
        }
    }
}
#endif
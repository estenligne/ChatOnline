using System;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Global.Enums;
using Global.Models;

namespace XamApp.Services
{
    public static class HTTPClient
    {
        private static HttpClient httpClient = null;

#if DEBUG
        public const string AccountBaseURL = "http://" + Local.IP + ":44361/api/Account/";
        public const string WebAPIBaseURL = "http://" + Local.IP + ":44363";
#else
        public const string AccountBaseURL = "https://account.estenligne.com/api/Account/";
        public const string WebAPIBaseURL = "https://api.chatonline.estenligne.com";
#endif

        public static Uri GetFileUri(string fileName)
        {
            string path = "/api/File/Download?fileName=" + fileName;
            return new Uri(WebAPIBaseURL + path);
        }

        public static async Task<HttpClient> NewClient(bool doSignIn = true, int timeout = 60)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(WebAPIBaseURL);
            client.Timeout = TimeSpan.FromSeconds(timeout);
            if (doSignIn) await SignIn(client, null);
            return client;
        }

        public static void SetAuthorization(HttpClient client, string authorization)
        {
            if (client == null)
                client = httpClient;

            if (client != null)
                client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authorization);
        }

        private static async Task<HttpResponseMessage> SignIn(HttpClient client, HttpResponseMessage oldResponse)
        {
            var dataStore = Models.DataStore.Instance;
            var user = await dataStore.GetUserAsync();
            if (user != null && user.Password != null)
            {
                var userDto = new ApplicationUserDTO
                {
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Password = user.Password,
                    RememberMe = user.RememberMe,
                };
                var response = await PostAsync(client, AccountBaseURL + "SignIn", userDto);
                if (response.IsSuccessStatusCode)
                {
                    var result = await HTTPClient.ReadAsAsync<ApplicationUserDTO>(response);
                    user.Authorization = result.Authorization;
                    await dataStore.UpdateUserAsync(user);
                    SetAuthorization(client, user.Authorization);
                }
                return response;
            }
            else if (oldResponse != null)
            {
                return oldResponse;
            }
            else
            {
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent("There is no prior sign in done on this app.");
                return response;
            }
        }

        public static void Clear()
        {
            if (httpClient != null)
            {
                httpClient.Dispose();
                httpClient = null;
            }
        }

        private static async Task<HttpClient> Client()
        {
            if (httpClient == null)
                httpClient = await NewClient();
            return httpClient;
        }

        private enum Method
        {
            Get,
            Post,
            Put,
            Patch,
            Delete
        }

        private static Task<HttpResponseMessage> Send(Method method, HttpClient client, string requestUri, HttpContent content)
        {
            switch (method)
            {
                case Method.Get: return client.GetAsync(requestUri);
                case Method.Post: return client.PostAsync(requestUri, content);
                case Method.Put: return client.PutAsync(requestUri, content);
                case Method.Patch: return client.PatchAsync(requestUri, content);
                case Method.Delete: return client.DeleteAsync(requestUri);
                default: throw new NotImplementedException($"Method {method} not supported");
            }
        }

        private static async Task<HttpResponseMessage> Request(Method method, HttpClient client, string requestUri, HttpContent content)
        {
            try
            {
                if (client == null)
                    client = await Client();

                HttpResponseMessage response = await Send(method, client, requestUri, content);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    response = await SignIn(client, response);
                    if (response.IsSuccessStatusCode)
                    {
                        response = await Send(method, client, requestUri, content);
                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                var response = new HttpResponseMessage(HttpStatusCode.GatewayTimeout);
                response.ReasonPhrase = ex.Message;
                response.Content = new StringContent(ex.StackTrace);
                return response;
            }
        }

        public static Task<HttpResponseMessage> GetAsync(HttpClient client, string requestUri)
        {
            return Request(Method.Get, client, requestUri, null);
        }

        public static Task<HttpResponseMessage> PostAsync<T>(HttpClient client, string requestUri, T model)
        {
            return Request(Method.Post, client, requestUri, GetStringContent(model));
        }

        public static Task<HttpResponseMessage> PutAsync<T>(HttpClient client, string requestUri, T model)
        {
            return Request(Method.Put, client, requestUri, GetStringContent(model));
        }

        public static Task<HttpResponseMessage> PatchAsync<T>(HttpClient client, string requestUri, T model)
        {
            return Request(Method.Patch, client, requestUri, GetStringContent(model));
        }

        public static Task<HttpResponseMessage> DeleteAsync(HttpClient client, string requestUri)
        {
            return Request(Method.Delete, client, requestUri, null);
        }

        public static Task<HttpResponseMessage> PostFile(HttpClient client, string parameterName, string fileName, System.IO.Stream content)
        {
            var streamContent = new StreamContent(content);
            var fileContent = new MultipartFormDataContent();
            fileContent.Add(streamContent, parameterName, fileName);
            return Request(Method.Post, client, "/api/File", fileContent);
        }

        private static StringContent GetStringContent<T>(T model)
        {
            if (model == null) return null;
            string json = JsonConvert.SerializeObject(model);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        public static async Task<T> ReadAsAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content);
        }

        private class HttpBadRequest
        {
            public string Code { get; set; }
            public string Description { get; set; }
        }

        private class HttpError // is normally from System.Web.Http.HttpError
        {
            public string Message { get; set; }
            public string ExceptionMessage { get; set; }
        }

        public static async Task<string> GetResponseError(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.GatewayTimeout)
                return response.ReasonPhrase;

            string content = null;
            if (response.Content != null)
                content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(content))
                return response.ReasonPhrase;

            var contentType = response.Content.Headers.ContentType.MediaType;

            if (contentType == "application/json")
            {
                try
                {
                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        var errors = JsonConvert.DeserializeObject<List<HttpBadRequest>>(content);
                        content = "";
                        foreach (var error in errors)
                            content += $"* {error.Description}\n";
                    }
                    else
                    {
                        var error = JsonConvert.DeserializeObject<HttpError>(content);
                        content = error.ExceptionMessage ?? error.Message;
                    }
                }
                catch (Exception) { } // 'content' from ReadAsStringAsync() is preserved
            }

            return content;
        }

        public static async Task<string> RegisterFcmToken(long deviceUsedId, string fcmToken)
        {
            string error = null;
            if (deviceUsedId != 0)
            {
                try
                {
                    var args = $"?deviceUsedId={deviceUsedId}&fcmToken={fcmToken}";
                    var url = "/api/DeviceUsed/RegisterFcmToken" + args;

                    var response = await PatchAsync<string>(null, url, null);

                    if (!response.IsSuccessStatusCode)
                        error = await GetResponseError(response);
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }
            }
            if (error != null)
            {
                System.Diagnostics.Debug.WriteLine($"RegisterFcmToken(): {error}");
            }
            return error;
        }
    }
}

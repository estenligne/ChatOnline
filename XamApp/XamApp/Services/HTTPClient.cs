using System;
using System.Text;
using System.Net;
using System.Net.Http;
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
        private static string WebAPIBaseURL = "http://192.168.43.64:44363";
#else
        private static string WebAPIBaseURL = "https://estenligne.com:44364";
#endif

        public static async Task<HttpClient> NewClient(bool doLogin = true, int timeout = 60)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(WebAPIBaseURL);
            client.Timeout = TimeSpan.FromSeconds(timeout);
            if (doLogin) await LoginAsync(client, null);
            return client;
        }

        private static async Task<HttpResponseMessage> LoginAsync(HttpClient client, HttpResponseMessage oldResponse)
        {
            var user = await Models.DataStore.Instance.GetUserAsync();
            if (user != null && user.Password != null)
            {
                var userDto = new ApplicationUserDTO
                {
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Password = user.Password,
                    RememberMe = true
                };
                var response = await PostAsync(client, "/api/Account/Login", userDto);
                return response;
            }
            else if (oldResponse != null)
            {
                return oldResponse;
            }
            else
            {
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent("There is no prior login done on this app.");
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

        private static HttpResponseMessage OnException(Exception ex)
        {
            var response = new HttpResponseMessage(HttpStatusCode.GatewayTimeout);
            response.ReasonPhrase = ex.Message;
            response.Content = new StringContent(ex.StackTrace);
            return response;
        }

        public static async Task<HttpResponseMessage> GetAsync(HttpClient client, string requestUri)
        {
            try
            {
                if (client == null)
                    client = await Client();

                var response = await client.GetAsync(requestUri);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    response = await LoginAsync(client, response);
                    if (response.IsSuccessStatusCode)
                    {
                        response = await client.GetAsync(requestUri);
                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                return OnException(ex);
            }
        }

        public static async Task<HttpResponseMessage> PostAsync<T>(HttpClient client, string requestUri, T model)
        {
            try
            {
                if (client == null)
                    client = await Client();

                var content = GetStringContent(model);
                var response = await client.PostAsync(requestUri, content);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    response = await LoginAsync(client, response);
                    if (response.IsSuccessStatusCode)
                    {
                        response = await client.PostAsync(requestUri, content);
                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                return OnException(ex);
            }
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

                    var response = await PostAsync<string>(null, url, null);

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

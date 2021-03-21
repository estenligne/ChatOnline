using System;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Global.Models;

namespace XamApp.Services
{
    public static class HTTPClient
    {
        private static HttpClient httpClient = null;

        private static string WebAPIBaseURL = "http://192.168.43.64:44363";

        public static async Task<HttpClient> NewClient(bool doLogin = true, int timeout = 60)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(WebAPIBaseURL);
            client.Timeout = TimeSpan.FromSeconds(timeout);
            if (doLogin) await LoginAsync(client);
            return client;
        }

        private static async Task<HttpResponseMessage> LoginAsync(HttpClient client)
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
                return await PostAsync(client, "/api/User/Login", userDto);
            }
            else return new HttpResponseMessage(HttpStatusCode.BadRequest);
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
                    response = await LoginAsync(client);
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
                    response = await LoginAsync(client);
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

        private class HttpError // is normally from System.Web.Http.HttpError
        {
            public string Message { get; set; }
            public string ExceptionMessage { get; set; }
        }

        public static async Task<string> GetResponseAsString(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.GatewayTimeout)
                return response.ReasonPhrase;

            var content = await response.Content.ReadAsStringAsync();
            var contentType = response.Content.Headers.ContentType.MediaType;

            if (contentType == "application/json")
            {
                if (response.IsSuccessStatusCode)
                {
                    content = JsonConvert.DeserializeObject<string>(content);
                }
                else
                {
                    var error = JsonConvert.DeserializeObject<HttpError>(content);
                    content = error.ExceptionMessage ?? error.Message;
                }
            }
            else if (!response.IsSuccessStatusCode && content.Length > 255)
                content = $"Error {(int)response.StatusCode}: {response.ReasonPhrase}";

            return content;
        }
    }
}

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Global.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace WebAPI.Services
{
    public static class HTTPClient
    {
        public static async Task<HttpClient> GetAuthenticated(IConfiguration configuration)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(configuration["JwtSecurity:Issuer"]);

            var userDto = new ApplicationUserDTO()
            {
                Email = "chatonline@estenligne.com",
                Password = configuration["JwtSecurity:SecretKey"]
            };

            string json = JsonConvert.SerializeObject(userDto);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("/api/Account/SignIn", stringContent);
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException(content);

            userDto = JsonConvert.DeserializeObject<ApplicationUserDTO>(content);
            httpClient.DefaultRequestHeaders.Add("Authorization", userDto.Authorization);

            return httpClient;
        }
    }
}

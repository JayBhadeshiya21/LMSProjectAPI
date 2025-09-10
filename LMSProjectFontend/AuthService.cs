using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LMSProjectFontend.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace LMSProjectFontend
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;

        public AuthService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5281/api/");
        }

        public async Task<string?> AuthenticateUserAsync(string email, string password, string role)
        {
            var requestData = new
            {
                Email = email,     
                Password = password,
                Role = role
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("UserAPI/login", content);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            return null;
        }
    }
}

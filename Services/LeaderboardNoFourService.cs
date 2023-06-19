using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Portfolio.Interfaces;
using Portfolio.Utils;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Net;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace Portfolio.Services
{
    public class LeaderboardNoFourService : ILeaderboardNoFourService
    {
        private readonly HttpClient _httpClient;

        private Configuration Configuration;

        public LeaderboardNoFourService(HttpClient httpClient, Configuration configuration)
        {
            _httpClient = httpClient;
            Configuration = configuration;
        }

        public async Task<TValue?> GetFromJsonAsync<TValue>(string requestURI)
        {
            try
            {
                return await this._httpClient.GetFromJsonAsync<TValue>(requestURI);
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("Touit");
            }

            return default(TValue);
        }

        public async Task<HttpResponseMessage> PostAsJsonAsync<TValue>(string requestURI, TValue value)
        {
            return await this._httpClient.PostAsJsonAsync(requestURI, value);
        }
    }
}

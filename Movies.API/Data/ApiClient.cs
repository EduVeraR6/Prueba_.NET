using IdentityModel.Client;
using System.Net.Http.Headers;

namespace Movies.API.Data
{

    public class ApiClient
    {
        private readonly HttpClient _httpClient;

        public ApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> CallApiAsync(string apiUrl)
        {
            var tokenResponse = await GetTokenAsync();

            if (tokenResponse.IsError)
            {
                throw new Exception($"Error retrieving token: {tokenResponse.Error}");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

            var response = await _httpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        private async Task<TokenResponse> GetTokenAsync()
        {
            var client = new HttpClient();
            var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5005");

            if (disco.IsError)
            {
                throw new Exception($"Error retrieving discovery document: {disco.Error}");
            }

            return await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "movieClient",
                ClientSecret = "secret", 
                Scope = "movieAPI"
            });
        }
    }
}

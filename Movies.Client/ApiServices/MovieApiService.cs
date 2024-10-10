using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Movies.Client.Models;
using Newtonsoft.Json;
using System.Text;

namespace Movies.Client.ApiServices
{
    public class MovieApiService : IMovieApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MovieApiService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<List<Movie>> GetMovies()
        {
            var httpClient = _httpClientFactory.CreateClient("MovieAPIClient");
            var response = await httpClient.GetAsync($"api/Movies");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var pelicula = JsonConvert.DeserializeObject<List<Movie>>(content);
            return pelicula;
        }

        public async Task<Movie> GetMovie(int id)
        {
            var httpClient = _httpClientFactory.CreateClient("MovieAPIClient");
            var response = await httpClient.GetAsync($"/movie/{id}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var pelicula = JsonConvert.DeserializeObject<Movie>(content);
            return pelicula;
        }

        public async Task<Movie> CreateMovie(MovieRegister movie)
        {
            var httpClient = _httpClientFactory.CreateClient("MovieAPIClient");

            var movieJson = JsonConvert.SerializeObject(movie);
            var content = new StringContent(movieJson, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("/movie", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var movieCreated = JsonConvert.DeserializeObject<Movie>(responseContent);

            return movieCreated;
        }



        public async Task<Movie> UpdateMovie(int id , Movie movie)
        {
            var httpClient = _httpClientFactory.CreateClient("MovieAPIClient");

            var movieJson = JsonConvert.SerializeObject(movie);
            var content = new StringContent(movieJson, Encoding.UTF8, "application/json");

            var response = await httpClient.PutAsync($"/movie/{id}", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var movieUpdate = JsonConvert.DeserializeObject<Movie>(responseContent);

            return movieUpdate;
        }

        public async Task DeleteMovie(int id)
        {
            var httpClient = _httpClientFactory.CreateClient("MovieAPIClient");


            var response = await httpClient.DeleteAsync($"/movie/{id}");
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();

          
        }

        public async Task<UserInfoViewModel> GetUserInfo()
        {
            var idpClient = _httpClientFactory.CreateClient("IDPClient");

            var metaDataResponse = await idpClient.GetDiscoveryDocumentAsync();

            if (metaDataResponse.IsError)
            {
                throw new HttpRequestException("Something went wrong while requesting the access token");
            }

            var accessToken = await _httpContextAccessor
                .HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            var userInfoResponse = await idpClient.GetUserInfoAsync(
               new UserInfoRequest
               {
                   Address = metaDataResponse.UserInfoEndpoint,
                   Token = accessToken
               });

            if (userInfoResponse.IsError)
            {
                throw new HttpRequestException("Something went wrong while getting user info");
            }

            var userInfoDictionary = new Dictionary<string, string>();

            foreach (var claim in userInfoResponse.Claims)
            {
                if (!userInfoDictionary.ContainsKey(claim.Type))
                {
                    userInfoDictionary.Add(claim.Type, claim.Value);
                }
            }

            return new UserInfoViewModel(userInfoDictionary);
        }


    }
}

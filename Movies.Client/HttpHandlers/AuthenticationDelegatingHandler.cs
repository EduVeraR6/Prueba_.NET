using System.IdentityModel.Tokens.Jwt;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Movies.Client.HttpHandlers
{
    public class AuthenticationDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthenticationDelegatingHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
          
            var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

          
            if (string.IsNullOrWhiteSpace(accessToken) || TokenExpired(accessToken))
            {
               
                accessToken = await RenewAccessTokenAsync();

                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    throw new HttpRequestException("No se logró refrescar el token.");
                }
            }

            request.SetBearerToken(accessToken);//token en el request

            return await base.SendAsync(request, cancellationToken);
        }

        private bool TokenExpired(string accessToken)
        {
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;

            if (expClaim != null && long.TryParse(expClaim, out long exp))
            {
                var expirationTimeUtc = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;

                var expirationTimeEcuador = TimeZoneInfo.ConvertTimeFromUtc(expirationTimeUtc, TimeZoneInfo.FindSystemTimeZoneById("America/Guayaquil"));

                var currentTimeEcuador = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("America/Guayaquil"));

                return expirationTimeEcuador < currentTimeEcuador;
            }

            return true; 
        }



        private async Task<string> RenewAccessTokenAsync()
        {
            var refreshToken = await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return null; 
            }

            var client = new HttpClient();
            var discoveryDocument = await client.GetDiscoveryDocumentAsync("https://localhost:5005");

            if (discoveryDocument.IsError)
            {
                throw new HttpRequestException("Error : No se logro acceder al Discovery Document");
            }

            var tokenResponse = await client.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = discoveryDocument.TokenEndpoint,
                ClientId = "movies_mvc_client",
                ClientSecret = "secret",
                RefreshToken = refreshToken
            });

            if (tokenResponse.IsError)
            {
                throw new HttpRequestException("Error mientras se renovava el token.");
            }

            // Guardar el nuevo access token y refresh token en el contexto de autenticación
            var authenticateInfo = await _httpContextAccessor.HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            authenticateInfo.Properties.UpdateTokenValue(OpenIdConnectParameterNames.AccessToken, tokenResponse.AccessToken);
            authenticateInfo.Properties.UpdateTokenValue(OpenIdConnectParameterNames.RefreshToken, tokenResponse.RefreshToken);
            await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, authenticateInfo.Principal, authenticateInfo.Properties);

            return tokenResponse.AccessToken;
        }
    }
}

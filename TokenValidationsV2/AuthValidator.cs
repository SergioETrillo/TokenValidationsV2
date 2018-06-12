using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using OpenIdConnectProtocols = Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace AnotherNameSpace
{
    public static class AuthValidator
    {
        private static readonly IConfigurationManager<OpenIdConnectProtocols.OpenIdConnectConfiguration> _configurationManager;

        private static readonly string ISSUER = "https://sergiotest.eu.auth0.com//";
        private static readonly string AUDIENCE = "http://functionhellonamevalidatetokens.azurewebsites.net/"; // TODO - enter your audience here. i.e. "https://api.wolftracker.com"

        static AuthValidator()
        {
            var documentRetriever = new HttpDocumentRetriever { RequireHttps = ISSUER.StartsWith("https://") };

            _configurationManager = new ConfigurationManager<OpenIdConnectProtocols.OpenIdConnectConfiguration>(
                $"{ISSUER}.well-known/openid-configuration",
                new OpenIdConnectProtocols.OpenIdConnectConfigurationRetriever(),
                documentRetriever
            );
        }

        public static async Task<ClaimsPrincipal> ValidateTokenAsync(AuthenticationHeaderValue value)
        {
            if (value?.Scheme != "Bearer")
                return null;

            var config = _configurationManager.GetConfigurationAsync(CancellationToken.None).Result; // await here was creating a weird error

            var validationParameter = new TokenValidationParameters
            {
                RequireSignedTokens = true,
                ValidAudience = AUDIENCE,
                ValidateAudience = true,
                ValidIssuer = ISSUER,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                IssuerSigningKeys = config.SigningKeys
            };

            ClaimsPrincipal result = null;
            var tries = 0;

            while (result == null && tries <= 1)
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    result = handler.ValidateToken(value.Parameter, validationParameter, out var token);
                }
                catch (SecurityTokenSignatureKeyNotFoundException ex1)
                {
                    // This exception is thrown if the signature key of the JWT could not be found.
                    // This could be the case when the issuer changed its signing keys, so we trigger a 
                    // refresh and retry validation.
                    _configurationManager.RequestRefresh();
                    tries++;
                }
                catch (SecurityTokenException ex2)
                {
                    return null;
                }
                catch (Exception ex)
                {
                    if (ex is System.Reflection.ReflectionTypeLoadException)
                    {
                        var typeLoadException = ex as ReflectionTypeLoadException;
                        var loaderExceptions = typeLoadException.LoaderExceptions;
                    }
                }
            }

            return result;
        }
    }
}

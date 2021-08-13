using Microsoft.ACR.DISQO.Service.Common;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.ACR.DISQO.Service.AuthService
{
    public class AuthService : IAuthService
    {

        private IConfiguration _config;

        public AuthService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<string> GetACRBearerToken(string repoName, string imageName)
        {

            var tenantId = _config["tenant_id"];
            var ServiceName = $"{repoName}.azurecr.io";

            var adAccessToken = await GetAccessToken(_config["client_id"], _config["client_secret"], tenantId);

            var acrRefreshToken = await GetRefreshToken(ServiceName, tenantId, adAccessToken);

            return await GetBearerToken(ServiceName, imageName, acrRefreshToken);
        }

        public async Task<string> GetAccessToken(string clientId, string clientSecret, string tenantId)
        {

            var dictAccessToken = new Dictionary<string, string> {
                    { "grant_type", "client_credentials" },
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "resource", "https://management.core.windows.net" }
                };

            using var requestMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://login.windows.net/{tenantId}/oauth2/token"),
                Content = new FormUrlEncodedContent(dictAccessToken)
            };
            return JObject.Parse(await Util.GetRESTContent(requestMessage))["access_token"].ToString();
        }

        private async Task<string> GetRefreshToken(string serviceName, string tenantId, string accessToken)
        {

            var dictRefreshToken = new Dictionary<string, string> {
                    { "grant_type", "access_token" },
                    { "tenant", tenantId },
                    { "service", serviceName},
                    { "access_token", accessToken }
                };

            using var reqMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"https://{serviceName}/oauth2/exchange"),
                Content = new FormUrlEncodedContent(dictRefreshToken)
            };

            return JObject.Parse(await Util.GetRESTContent(reqMessage))["refresh_token"].ToString();

        }

        private async Task<string> GetBearerToken(string serviceName, string imageName, string refreshToken)
        {
            var dictBToken = new Dictionary<string, string>()
            {
                    { "grant_type", "refresh_token" },
                    { "scope", $"repository:{imageName}:pull,push" }, 
                    { "service", serviceName},
                    { "refresh_token", refreshToken }
            };

            using var reqMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"https://{serviceName}/oauth2/token"),
                Content = new FormUrlEncodedContent(dictBToken)
            };

            return JObject.Parse(await Util.GetRESTContent(reqMessage))["access_token"].ToString();
        }
    }
}
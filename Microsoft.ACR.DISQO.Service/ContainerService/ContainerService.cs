using Microsoft.ACR.DISQO.Service.AuthService;
using Microsoft.ACR.DISQO.Service.Common;
using Microsoft.ACR.DISQO.Service.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.ACR.DISQO.Service.ContainerService
{
    public class ContainerService : IContainerService
    {

        private IConfiguration _config;

        private readonly IAuthService _authService;

        private readonly ILogger<ContainerService> _logger;

        private string AcrToken { get; set; }

        private string AdToken { get; set; }

        private const string apiVersion = "api-version=2021-04-01";

        public ContainerService(IConfiguration config, IAuthService auth, ILogger<ContainerService> logger)
        {
            _config = config;
            _authService = auth;
            _logger = logger;
        }

        public async Task<bool> PassQuarantine(string repositoryFullURI, string manifestId, bool pass)
        {
            var retVal = false;
            try
            {
                var acr = new ACRModel(repositoryFullURI);

                using var reqMessage = Util.GetRequestMessage(HttpMethod.Patch, $"https://{acr.ServerURL}/acr/v1/{acr.ImageRepo}/_manifests/{manifestId}");
                reqMessage.Content = new StringContent(@"{""quarantineState"": ""[passed]""}".Replace("[passed]", pass ? "Passed" : "Quarantined" ));

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = await GetAuthHeaderWithACRToken(acr.HostName, acr.ImageRepo);
                
                var resp = await client.SendAsync(reqMessage);
                retVal = resp.IsSuccessStatusCode;

                var body = await resp.Content.ReadAsStringAsync();

                _logger.LogInformation($"response Body = {body}");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex,ex.Message);
            }

            return retVal;
        }

        public async Task<string> UpdateQuarantineStatus(string repositoryFullURI, bool enabled)
        {

            var resource = await FindRepositoryResource(new ACRModel(repositoryFullURI).HostName);

            var content = @"{""properties"": {""policies"": { ""QuarantinePolicy"":{ ""status"": ""{X}""}}}}".Replace("{X}", enabled ? "enabled" : "disabled");

            var reqMessage = Util.GetRequestMessage(HttpMethod.Patch, GetRepositoryURI(resource));            
            reqMessage.Content = new StringContent(content, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = await GetAuthHeaderWithAccessToken();

            var body = await client.GetRESTContent(reqMessage);

            return JObject.Parse(body)["error"] != null ? JObject.Parse(body)["error"]["message"].ToString() : "";

        }

        public async Task<bool> QuarantineEnabled(string repositoryName)
        {

            var retVal = false;

            try
            {

                var resource = await FindRepositoryResource(repositoryName);

                if (resource != null) {

                    using var client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization = await GetAuthHeaderWithAccessToken();

                    var body = await client.GetRESTContent(Util.GetRequestMessage(HttpMethod.Get, GetRepositoryURI(resource)));

                    var enabledValue = JObject.Parse(body)["properties"]["policies"]["quarantinePolicy"]["status"].ToString();

                    retVal = enabledValue.Equals("enabled", StringComparison.CurrentCultureIgnoreCase);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return retVal;
        }

        public async Task<ImageManifest> GetRepositoryManifests(ACRModel model)
        {

            var assessments = await GetImageAssessments(model.HostName);

            using var reqMessage = Util.GetRequestMessage(HttpMethod.Get, $"https://{model.ServerURL}/acr/v1/{model.ImageRepo}/_manifests");
            {

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = await GetAuthHeaderWithACRToken(model.HostName, model.ImageRepo);

                var resp = await client.SendAsync(reqMessage);

                var package = JsonConvert.DeserializeObject<ImageManifest>(await resp.Content.ReadAsStringAsync());

                if(package != null && package.Manifests != null)
                    package.Manifests = package.Manifests.OrderByDescending(x => x.CreatedTime).ToList();

                package.QuarantineEnabled = await QuarantineEnabled(model.HostName);

                return AddSecurityAssessmentstoImages(package, assessments);

            }
        }

        public async Task<bool> ProcessImageQuarantineStatus(ContainerImageRequest newImage)
        {
            var retVal = true;

            _logger.LogInformation($"entered ContainerService.ProcessImageQuarantineStatus ");

            var assessments = GetByDigestId(await GetImageAssessments(newImage.HostName), newImage.Target.Digest);

            _logger.LogInformation($"Assessments for digest {newImage.Target.Digest} = {assessments.Count()}");

            if (assessments.Count() > 0)
                retVal = !await PassQuarantine(newImage.FullHostRepoName, newImage.Target.Digest, false);

            return retVal;
        }

        private string GetRepositoryURI(ResourceManifest resource)
        {
            return $"https://management.azure.com{resource.Id}?api-version=2019-05-01";
        }

        private async Task<ResourceManifest> GetResource(string subscription, string repositoryName) {

            var uri = $"https://management.azure.com{subscription}/resources?$filter=resourceType%20eq%20%27Microsoft.ContainerRegistry%2Fregistries%27%20and%20Name%20eq%20%27{repositoryName}%27&{apiVersion}";

            using var reqMessage = Util.GetRequestMessage(HttpMethod.Get, uri);

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = await GetAuthHeaderWithAccessToken();

            var data = JObject.Parse(await client.GetRESTContent(reqMessage))["value"][0].ToString();

            return JsonConvert.DeserializeObject<ResourceManifest>(data);

        }

        private async Task<ResourceManifest> FindRepositoryResource(string repositoryName)
        {
            var subs = await GetSubscriptions();

            ResourceManifest retVal = null;

            foreach (var sub in subs) {
                var resource = await GetResource(sub.Id, repositoryName);

                if (!(resource is null))
                {
                    retVal = resource;
                    break;
                }
            }

            return retVal;
            
        }

        private async Task<List<SecurityAssessmentManifest>> GetImageAssessments(string repositoryName)
        {

            List<SecurityAssessmentManifest> retVal = null;

            var data = await GetAssessmentLink(repositoryName);

            if (!string.IsNullOrEmpty(data))
            {
                
                var uri = $"https://management.azure.com{data}?api-version=2020-01-01";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = await GetAuthHeaderWithAccessToken();

                var body = await client.GetRESTContent(Util.GetRequestMessage(HttpMethod.Get, uri));

                retVal = JsonConvert.DeserializeObject<List<SecurityAssessmentManifest>>(JObject.Parse(body)["value"].ToString());
                
            }

            return retVal;
        }

        private async Task<string> GetAssessmentLink(string repositoryName)
        {

            var repo = await FindRepositoryResource(repositoryName);

            var uri = $"https://management.azure.com{repo.Id}/providers/Microsoft.Security/assessments?api-version=2020-01-01";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = await GetAuthHeaderWithAccessToken();

            var body = await client.GetRESTContent(Util.GetRequestMessage(HttpMethod.Get, uri));

            var securityAssessments = JsonConvert.DeserializeObject<List<SecurityManifest>>(JObject.Parse(body)["value"].ToString());

            var linkObj = securityAssessments.Where(x => x.properties.AdditionalData != null).FirstOrDefault();
            
            return linkObj != null ? linkObj.properties.AdditionalData.subAssessmentsLink : "";
        }

        private async Task<List<SubscriptionManifest>> GetSubscriptions()
        {

            using var reqMessage = Util.GetRequestMessage(HttpMethod.Get, $"https://management.azure.com/subscriptions?{apiVersion}");

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = await GetAuthHeaderWithAccessToken();

            var resp = await client.SendAsync(reqMessage);

            var data = JObject.Parse(await resp.Content.ReadAsStringAsync())["value"].ToString();

            var tmp = JsonConvert.DeserializeObject<List<SubscriptionManifest>>(data);

            return tmp;

        }

        private async Task<AuthenticationHeaderValue> GetAuthHeaderWithACRToken(string repositoryHostName, string imageRepositoryName)
        {            
            AcrToken = !string.IsNullOrEmpty(AcrToken) ? AcrToken : await _authService.GetACRBearerToken(repositoryHostName, imageRepositoryName);
            return new AuthenticationHeaderValue("Bearer", AcrToken);
        }

        private async Task<AuthenticationHeaderValue> GetAuthHeaderWithAccessToken()
        {
            AdToken = !string.IsNullOrEmpty(AdToken) ? AdToken : await _authService.GetAccessToken(_config["client_id"], _config["client_secret"], _config["tenant_id"]);
            return new AuthenticationHeaderValue("Bearer", AdToken);
        }

        private List<SecurityAssessmentManifest> GetByDigestId(List<SecurityAssessmentManifest> assessments, string digestId)
        {
            return assessments.Where(x => x.Properties.ResourceDetails.Id.Contains(digestId)).ToList();
        }

        private ImageManifest AddSecurityAssessmentstoImages(ImageManifest manifest, List<SecurityAssessmentManifest> assessments)
        {
            if(manifest != null && manifest.Manifests != null)
                manifest.Manifests.ForEach(x => x.SecurityAssessments = GetByDigestId(assessments, x.Digest));

            return  manifest;
        }
    }
}
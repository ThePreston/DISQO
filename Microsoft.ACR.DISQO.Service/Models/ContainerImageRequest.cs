using Newtonsoft.Json;
using System;

namespace Microsoft.ACR.DISQO.Service.Models
{
    public class ContainerImageRequest
    {

        [JsonProperty("id")]
        public string Id;

        [JsonProperty("timestamp")]
        public DateTime Timestamp;

        [JsonProperty("action")]
        public string Action;

        [JsonProperty("target")]
        public Target Target;

        [JsonProperty("request")]
        public Request Request;

        public string HostName { get { return Request.Host.Split('.')[0]; } }

        public string FullHostRepoName { get { return $"{Request.Host}/{Target.Repository}" ; } }

    }
    public class Target
    {
        [JsonProperty("mediaType")]
        public string MediaType;

        [JsonProperty("size")]
        public int Size;

        [JsonProperty("digest")]
        public string Digest;

        [JsonProperty("length")]
        public int Length;

        [JsonProperty("repository")]
        public string Repository;

        [JsonProperty("tag")]
        public string Tag;
    }

    public class Request
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("host")]
        public string Host;

        [JsonProperty("method")]
        public string Method;

        [JsonProperty("useragent")]
        public string Useragent;
    }

}

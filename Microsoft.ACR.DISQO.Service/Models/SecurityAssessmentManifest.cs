using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.ACR.DISQO.Service.Models
{
    public class SecurityAssessmentManifest
    {

        public Properties Properties { get; set; }
    }


    public class Status
    {
        public string Code { get; set; }
        public string Severity { get; set; }
    }

    public class ResourceDetails
    {
        public string Source { get; set; }
        public string Id { get; set; }
    }

    public class Cve
    {
        public string Title { get; set; }
        public string Link { get; set; }
    }

    public class VendorReference
    {
        public string Title { get; set; }
        public string Link { get; set; }
    }

    public class Findings
    {
        public int Low { get; set; }
        public int Medium { get; set; }
        public int High { get; set; }
        public int Critical { get; set; }
    }

    public class PipelineData
    {
        public string GitHubSha { get; set; }
        public string GitHubRef { get; set; }
    }

    public class CicdData
    {
        public string Status { get; set; }
        public string RunUrl { get; set; }
        public string RepositoryUrl { get; set; }
        public Findings Findings { get; set; }
        public PipelineData PipelineData { get; set; }
    }

    public class AdditionalData
    {
        public string AssessedResourceType { get; set; }
        public string Type { get; set; }
        
        public bool Patchable { get; set; }
        public List<Cve> Cve { get; set; }
        public DateTime PublishedTime { get; set; }
        public List<VendorReference> VendorReferences { get; set; }
        public string RegistryHost { get; set; }
        public string RepositoryName { get; set; }
        public string ImageDigest { get; set; }
        public CicdData CicdData { get; set; }

        public string subAssessmentsLink { get; set; }
    }

    public class Properties
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public Status Status { get; set; }
        public string Remediation { get; set; }
        public string Impact { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public DateTime TimeGenerated { get; set; }
        public ResourceDetails ResourceDetails { get; set; }

        [JsonProperty("additionalData")]
        public AdditionalData AdditionalData { get; set; }
    }

}

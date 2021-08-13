using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.ACR.DISQO.Service.Models
{
    public class ImageManifest
    {
        public bool QuarantineEnabled { get; set; }
        public string Registry { get; set; }
        public string ImageName { get; set; }
        public List<Manifest> Manifests { get; set; }
    }
    
    public class Manifest 
    { 
        public string Digest { get; set; }
        public int ImageSize { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string Architecture { get; set; }
        public string Os { get; set; }
        public string MediaType { get; set; }
        public string ConfigMediaType { get; set; }
        public List<string> Tags { get; set; }

        public string Security
        {
            get
            {
                var retVal = "";

                if(SecurityAssessments.Count > 0)
                {
                    var highCount = SecurityAssessments.Where(x => x.Properties.Status.Severity == "High").Count();
                    var mediumCount = SecurityAssessments.Where(x => x.Properties.Status.Severity == "Medium").Count();
                    var lowCount = SecurityAssessments.Where(x => x.Properties.Status.Severity == "Medium").Count();

                    retVal = $"High = {highCount} <br> Medium ={mediumCount} <br> Low = {lowCount}";
                }
                return retVal ;
            }
        }

        [JsonProperty("cicdurl")]
        public string CICDURL
        {
            get
            {
                var retVal = SecurityAssessments.Select(x => x.Properties.AdditionalData.CicdData.RunUrl).FirstOrDefault() ?? "";
                return retVal;
            }
        }

        [JsonProperty("cicd")]
        public string CICD
        {
            get
            {
                var retVal = "";

                if (SecurityAssessments.Count > 0)
                {
                    var criticalCount = SecurityAssessments.Select(x => Convert.ToInt32(x.Properties?.AdditionalData?.CicdData?.Findings?.Critical ?? 0)).Sum();
                    var highCount = SecurityAssessments.Select(x => Convert.ToInt32(x.Properties?.AdditionalData?.CicdData?.Findings?.High ?? 0)).Sum();
                    var mediumCount = SecurityAssessments.Select(x => Convert.ToInt32(x.Properties?.AdditionalData?.CicdData?.Findings?.Medium ?? 0)).Sum();
                    var lowCount = SecurityAssessments.Select(x => Convert.ToInt32(x.Properties?.AdditionalData?.CicdData?.Findings?.Low ?? 0)).Sum();
                    
                    retVal = $"Critical = {criticalCount}  <br> High = {highCount} <br> Medium ={mediumCount} <br> Low = {lowCount}";
                }
                return retVal;
            }
        }




        public List<SecurityAssessmentManifest> SecurityAssessments { get; set; }

        public string Passable{ get {
                return !QuarantineState.Equals("passed", StringComparison.CurrentCultureIgnoreCase) ? Digest : "";
            } }

        public string QuarantineState { get { return ChangeableAttributes.QuarantineState; } }

        public ChangeableAttributes ChangeableAttributes { get; set; }
    }

    public class ChangeableAttributes
    {
        public bool DeleteEnabled { get; set; }
        public bool WriteEnabled { get; set; }
        public bool ReadEnabled { get; set; }
        public bool ListEnabled { get; set; }
        public string QuarantineState { get; set; }
        public string QuarantineDetails { get; set; }
    }

}
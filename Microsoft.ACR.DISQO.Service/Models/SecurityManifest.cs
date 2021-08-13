using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.ACR.DISQO.Service.Models
{
    public class SecurityManifest
    {
        public string type { get; set; }

        public string id { get; set; }

        public string name { get; set; }

        public Properties properties { get; set; }

    }

    //public class Properties
    //{
    //    public string displayName { get; set; }

    //    public AdditionalData additionalData { get; set; }
    //}
    
    //public class AdditionalData
    //{
    //    public string subAssessmentsLink { get; set; }
    //}
}

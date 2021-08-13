using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.ACR.DISQO.Service.Models
{
    public class ACRModel
    {

        public ACRModel(string fullServerRepoURI)
        {

            var splitdata = fullServerRepoURI.Split('/', StringSplitOptions.RemoveEmptyEntries);

            ServerURL = splitdata[0];
            ImageRepo = splitdata[1];
            HostName = ServerURL.Split('.', StringSplitOptions.RemoveEmptyEntries)[0];

        }
        public ACRModel(string serverHostName, string imageRepository)
        {
            HostName = serverHostName;
            ServerURL = $"{serverHostName}.azurecr.io";
            ImageRepo = imageRepository;
        }

        public string  ServerURL { get; private set; }

        public string HostName { get; private set; }

        public string ImageRepo { get; set; }

    }
}

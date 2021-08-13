namespace Microsoft.ACR.DISQO.Service.Models
{
    class SubscriptionManifest
    {
        public string Id{ get; set; }
        public string authorizationSource { get; set; }
        public string subscriptionId { get; set; }
        public string tenantId { get; set; }
        public string displayName { get; set; }

        public SubscriptionManifest()
        {
        }
    }
}
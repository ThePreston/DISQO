using System.Threading.Tasks;

namespace Microsoft.ACR.DISQO.Service.AuthService
{
    public interface IAuthService
    {
        Task<string> GetAccessToken(string clientId, string clientSecret, string tenantId);

        Task<string> GetACRBearerToken(string registryName, string imageName);
    }
}
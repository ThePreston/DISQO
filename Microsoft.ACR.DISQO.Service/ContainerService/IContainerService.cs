using Microsoft.ACR.DISQO.Service.Models;
using System.Threading.Tasks;

namespace Microsoft.ACR.DISQO.Service.ContainerService
{
    public interface IContainerService
    {
        Task<string> UpdateQuarantineStatus(string repositoryFullURI, bool enabled);

        Task<bool> QuarantineEnabled(string repositoryName);

        Task<bool> PassQuarantine(string repositoryFullURI, string manifestId, bool pass);

        Task<ImageManifest> GetRepositoryManifests(ACRModel model);

        Task<bool> ProcessImageQuarantineStatus(ContainerImageRequest newImage);

        Task<bool?> ImageScanAssessmentFail(ContainerImageRequest newImage);
    }
}
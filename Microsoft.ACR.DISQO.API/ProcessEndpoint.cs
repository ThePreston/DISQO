using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ACR.DISQO.Service.ContainerService;
using Microsoft.ACR.DISQO.Service.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Microsoft.ACR.DISQO.API
{
    public class ProcessEndpoint
    {

        private readonly ILogger<ProcessEndpoint> _logger;

        private readonly IContainerService _service;

        public ProcessEndpoint(ILogger<ProcessEndpoint> logger, IContainerService service)
        {
            _logger = logger;
            _service = service;
        }

        [FunctionName("ImageProcessor")]
        public async Task<string> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {

            var input = context.GetInput<ContainerImageRequest>();

            var outputMessage = "No Scan Result Found";

            for (int i = 0; i < 15; i++)
            {

                var result = await context.CallActivityAsync<bool?>("ImageResults", input);

                if (result is null || !result.HasValue)
                {
                    DateTime deadline = context.CurrentUtcDateTime.Add(TimeSpan.FromMinutes(1));
                    await context.CreateTimer(deadline, CancellationToken.None);
                }
                else
                {

                    if (result.Value)
                        await _service.PassQuarantine(input.FullHostRepoName, input.Target.Digest, false);
                            
                    outputMessage = $"Scan Result = {result}";

                    break;
                }
            }

            return outputMessage;
        }

        [FunctionName("ImageResults")]
        public async Task<bool?> ImageResults([ActivityTrigger] ContainerImageRequest imageRequest, ILogger log)
        {

            log.LogInformation($"Entered ImageResults.");

            return await _service.ImageScanAssessmentFail(imageRequest);

        }

        [FunctionName("ImagePushWebhook")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {

            var data = await req.Content.ReadAsAsync<ContainerImageRequest>();

            string instanceId = await starter.StartNewAsync("ImageProcessor", data);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
using Microsoft.ACR.DISQO.Service.AuthService;
using Microsoft.ACR.DISQO.Service.ContainerService;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Microsoft.ACR.DISQO.API.Startup))]
namespace Microsoft.ACR.DISQO.API
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
            
            builder.Services.AddSingleton<IConfiguration>(config);

            builder.Services.AddTransient<IAuthService, AuthService>();

            builder.Services.AddTransient<IContainerService, ContainerService>();

        }
    }
}
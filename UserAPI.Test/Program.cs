using System;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace UserAPI.Test
{
    class Program
    {
        [Option(Description = "Service base URL", Template = "--url")]
        private string ServiceBaseUrl { get; }
        
        [Option(Description = "Host", Template = "--host")]
        private string Host { get; }

        [Option(Description = "Virtual users count", Template = "--vuser")]
        private short VirtualUsersCount { get; }

        [Option(Description = "Delay in milliseconds", Template = "--delay")]
        private int? Delay { get; }

        private readonly ITestService _testService;
        private readonly ILogger<Program> _logger;

        public Program(ITestService testService, ILogger<Program> logger)
        {
            _testService = testService;
            _logger = logger;
        }

        public static async Task<int> Main(string[] args)
        {
            return await new HostBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    services.AddHttpClient();
                    services.AddSingleton<ITestService, TestService>();
                })
                .RunCommandLineApplicationAsync<Program>(args);
        }

        private void OnExecute()
        {
            var userCount = VirtualUsersCount > 0 ? VirtualUsersCount : 1;
            
            _logger.LogInformation("Load test is starting...");

            try
            {
                var serviceUrl = new Uri(ServiceBaseUrl);
                
                while (true)
                {
                    try
                    {
                        Parallel.For(0, userCount, _ => _testService.RunAsync(serviceUrl, Host));
                    }
                    catch (AggregateException e)
                    {
                        _logger.LogError(e.Message);
                    }

                    if (Delay.HasValue)
                        Thread.Sleep(Delay.Value);
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Load test aborted. ServiceBaseUrl: {ServiceBaseUrl}. Reason: {Environment.NewLine}{e.Message}");
            }
        }
    }
}
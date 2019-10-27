using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using UnTaskAlert.Common;
using UnTaskAlert.Models;

namespace UnTaskAlert
{
    public class UnTaskAlertFunction
    {
        private readonly IMonitoringService _service;
        private readonly Config _config;
        private readonly IDbAccessor _dbAccessor;

        public UnTaskAlertFunction(IMonitoringService service, IOptions<Config> options, IDbAccessor dbAccessor)
        {
            _service = Arg.NotNull(service, nameof(service));
            _config = Arg.NotNull(options.Value, nameof(options));
            _dbAccessor = Arg.NotNull(dbAccessor, nameof(dbAccessor));
        }

        [FunctionName("activeTaskMonitoring")]
        public async Task Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Executing monitoring task");
            log.LogInformation($"Reading subscribers: '{_config.Subscribers}'");

            var subscribers = await _dbAccessor.GetSubscribers();
            foreach (var subscriber in subscribers)
            {
                try
                {
                    await _service.PerformMonitoring(subscriber,
                        _config.AzureDevOpsAddress,
                        _config.AzureDevOpsAccessToken,
                        log);
                }
                catch (Exception e)
                {
                    log.LogError(e.ToString());
                }
            }
        }
    }
}

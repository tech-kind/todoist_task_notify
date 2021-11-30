using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace todoist_task_notify
{
    public class FuncTaskNotify
    {
        [FunctionName("TaskNotify")]
        public void Run([TimerTrigger("0 30 7 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}

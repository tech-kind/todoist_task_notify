using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;

namespace todoist_task_notify
{
    public class FuncTaskNotify
    {
        [FunctionName("TaskNotify")]
        public void Run([TimerTrigger("0 30 7 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            // Tokenを環境変数から取得
            var todoist_token = Environment.GetEnvironmentVariable("TODOIST_TOKEN",
                EnvironmentVariableTarget.Process);
            var line_token = Environment.GetEnvironmentVariable("LINE_TOKEN",
                EnvironmentVariableTarget.Process);
            if (string.IsNullOrEmpty(todoist_token) || string.IsNullOrEmpty(line_token))
            {
                log.LogWarning("Token error!!");
                return;
            }

            // Todoistからタスクを取得する
            var todoist_client = new RestClient("https://api.todoist.com/rest/v1");
            todoist_client.AddDefaultHeader("Authorization", $"Bearer {todoist_token}");
            var todoist_request = new RestRequest("tasks", Method.GET)
                .AddQueryParameter("project_id", "2279337164");
            var todoist_response = todoist_client.Execute(todoist_request);
            if (todoist_response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                log.LogWarning("Http status code error!!");
                return;
            }
            var content = JsonConvert.DeserializeObject<List<TodoistContent>>(todoist_response.Content);
            var message = new StringBuilder();
            message.AppendLine();
            foreach (var item in content.Select((value, index) => new {value, index}))
            {
                message.AppendLine($"[{item.index + 1}] {item.value.content}");
            }

            // Lineにタスクを通知する
            var line_client = new RestClient("https://notify-api.line.me/api");
            line_client.AddDefaultHeader("Authorization", $"Bearer {line_token}");
            var line_request = new RestRequest("notify", Method.POST)
                .AddParameter("message", message);
            var line_response = line_client.Execute(line_request);
            if (line_response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                log.LogWarning("Http status code error!!");
                return;
            }
        }
    }

    public class TodoistContent
    {
        public string id { get; set; }

        public string order { get; set; }

        public string content { get; set; }
    }
}

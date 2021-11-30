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

            var todoist_token = Environment.GetEnvironmentVariable("TODOIST_TOKEN",
                EnvironmentVariableTarget.Process);
            var line_token = Environment.GetEnvironmentVariable("LINE_TOKEN",
                EnvironmentVariableTarget.Process);
            if (string.IsNullOrEmpty(todoist_token) || string.IsNullOrEmpty(line_token))
            {
                log.LogWarning("Token error!!");
            }

            var todoist_client = new TodoistRestClient("https://api.todoist.com/rest/v1", todoist_token);
            var todoist_request = todoist_client.CreateRequest("2279337164");
            var todoist_response = todoist_client.Execute(todoist_request);
            if (todoist_response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                log.LogWarning("Http status code error!!");
            }
            var content = JsonConvert.DeserializeObject<List<TodoistContent>>(todoist_response.Content);
            var message = new StringBuilder();
            message.AppendLine();
            foreach (var item in content.Select((value, index) => new {value, index}))
            {
                message.AppendLine($"[{item.index}] {item.value.content}");
            }

            var line_client = new LineRestClient("https://notify-api.line.me/api", line_token);
            var line_request = line_client.CreateRequest(message.ToString());
            var line_response = line_client.Execute(line_request);
            if (line_response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                log.LogWarning("Http status code error!!");
            }
        }
    }

    public class RestClientBase
    {
        private readonly RestClient _client;

        public RestClientBase(string url, string access_token)
        {
            _client = new RestClient(url);
            _client.AddDefaultHeader("Authorization", $"Bearer {access_token}");
        }

        public virtual IRestRequest CreateRequest(string param)
        {
            return new RestRequest();
        }

        public virtual IRestResponse Execute(IRestRequest request)
        {
            return _client.Execute(request);
        }
    }

    public class TodoistRestClient : RestClientBase
    {
        public TodoistRestClient(string url, string access_token)
            : base(url, access_token)
        {

        }

        public override IRestRequest CreateRequest(string param)
        {
            var request = new RestRequest("tasks", Method.GET)
                .AddQueryParameter("project_id", param);
            return request;
        }
    }

    public class LineRestClient : RestClientBase
    {
        public LineRestClient(string url, string access_token)
            : base (url, access_token)
        {

        }

        public override IRestRequest CreateRequest(string param)
        {
            var request = new RestRequest("notify", Method.POST)
                .AddParameter("message", param);
            return request;
        }
    }

    public class TodoistContent
    {
        public string id { get; set; }

        public string order { get; set; }

        public string content { get; set; }
    }
}

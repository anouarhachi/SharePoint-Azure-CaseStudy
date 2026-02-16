using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json; 
using System.Threading.Tasks;

namespace CaseStudyBackend
{
    public class TodoItem
    {
        public int id { get; set; }
        public string name { get; set; }
        public string source { get; set; }
    }

    public class ReceiveItem
    {
        private readonly ILogger _logger;

        public ReceiveItem(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ReceiveItem>();
        }

        [Function("ReceiveItem")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            string requestBody = await req.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<TodoItem>(requestBody);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            if (data != null)
            {
                _logger.LogInformation($"Processing item ID {data.id} from source: {data.source}");
                await response.WriteStringAsync($"Success: Received Item {data.id} ({data.name})");
            }
            else
            {
                _logger.LogWarning("Received empty or invalid data payload.");
                await response.WriteStringAsync("Error: No valid data received.");
            }

            return response;
        }
    }
}
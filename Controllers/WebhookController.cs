using System.Buffers.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FlashFit.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlashFit.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WebhookController> _logger;
        private const string BaseUrl = "https://witty-mustang-18974.upstash.io/get";
        private const string RedisKey = "AUoeAAIjcDE0ZTcxMmM5MjA4MjI0NTJmOGIwNjg4MTM2ZjU0YjA1N3AxMA";

        public WebhookController(IHttpClientFactory httpClientFactory, ILogger<WebhookController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpPost]
        public async Task < IActionResult> ReceiveWebhook([FromBody] WebhookResponseDto payload)
        {
            var id = payload.Id;
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", RedisKey);

            var response = await client.GetAsync($"{BaseUrl}/get/{id}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Erro ao obter dados do webhook: {response.ReasonPhrase}");
                return StatusCode((int)response.StatusCode, "Erro ao processar o webhook.");
            }
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var outerJson = JsonDocument.Parse(content);
                var resultString = outerJson.RootElement.GetProperty("result").GetString();

                var user = JsonSerializer.Deserialize<UserData>(resultString);

                if (user.User.Id == id)
                {
                    Console.WriteLine("Deu Bom");
                }
                var clientRedis = _httpClientFactory.CreateClient();
                                clientRedis.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", RedisKey);
                var comando = new object[]
                        {
                        "DEL",
                         $"{id}"
                        };

                string comandoJson = JsonSerializer.Serialize(comando);
                var contentDelete = new StringContent(comandoJson, Encoding.UTF8, "application/json");

                var responseRedis = await clientRedis.PostAsync($"{BaseUrl}", contentDelete);

                if (responseRedis.IsSuccessStatusCode)
                {
                    var contentRedis = await responseRedis.Content.ReadAsStringAsync();
                    var userRedis = JsonSerializer.Deserialize<UserData>(contentRedis);

                    if (userRedis != null)
                    {
                        // Process userRedis
                    }
                }

                return Ok(user);
            }
            else
            {
                _logger.LogError($"Erro ao obter usuário: {response.ReasonPhrase}");
                return StatusCode((int)response.StatusCode, "Erro ao obter usuário.");
            }

        }
    }
}

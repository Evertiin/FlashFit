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
    public class UserController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;

        private string BaseUrl => _configuration["Redis:BaseUrl"];
        private string RedisKey => _configuration["Redis:Key"];
        private string BaseUrlAsaas => _configuration["Asaas:BaseUrl"];
        private string BearerAsaas => _configuration["Asaas:BearerToken"];

        private readonly IConfiguration _configuration;

        public UserController(
            IHttpClientFactory httpClientFactory,
            ILogger<UserController> logger,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateUser([FromBody] UserData user)
        {
            try
            {
                var Assasclient = _httpClientFactory.CreateClient();

                if (user.Plano.MetodoPagamento == "Pix" && user.Plano.Id == "FF01")
                {
                    var checkoutRequest = new CheckoutRequest
                    {
                        BillingTypes = new List<string> { "PIX" },
                        ChargeTypes = new List<string> { "DETACHED" },
                        Callback = new Callback
                        {
                            SuccessUrl = "https://flash-fit-front.vercel.app/success",
                            CancelUrl = "https://flash-fit-front.vercel.app/error",
                            ExpiredUrl = "https://example.com/expired"
                        },
                        Items = new List<Item>
                        {
                            new Item
                            {
                                Description = "Plano de Treino personalizado para casa ou academia",
                                Name = "Plano de Treino",
                                Quantity = 1,
                                Value = 9.90m
                            }
                        }
                    };
                    var json = JsonSerializer.Serialize(checkoutRequest, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase // garante que vai sair em camelCase
                    });
                    var contentAsaas = new StringContent(json, Encoding.UTF8, "application/json");
                    Assasclient.DefaultRequestHeaders.Add("User-Agent", "FlashFit");
                    Assasclient.DefaultRequestHeaders.Add("access_token", BearerAsaas);

                    _logger.LogInformation(
                        "Chamada para API Asaas: Endpoint={Endpoint}, MetodoPagamento={MetodoPagamento}, PlanoId={PlanoId}, Body={Body}",
                        BaseUrlAsaas,
                        user.Plano.MetodoPagamento,
                        user.Plano.Id,
                        json
                    );

                    var requestAsaas = await Assasclient.PostAsync($"{BaseUrlAsaas}", contentAsaas);
                    var responseContent = await requestAsaas.Content.ReadAsStringAsync();


                    if (requestAsaas.IsSuccessStatusCode)
                    {
                        var resultado = JsonSerializer.Deserialize<CheckoutResponse>(responseContent);


                        user.User.Id= resultado.Id;

                        var Redisclient = _httpClientFactory.CreateClient();
                        Redisclient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", RedisKey);
                        string usuarioJson = JsonSerializer.Serialize(user);

                        var comando = new object[]
                       {
                          "SET",
                         $"{resultado.Id}",
                         usuarioJson
                        };

                        string comandoJson = JsonSerializer.Serialize(comando);
                        var content = new StringContent(comandoJson, Encoding.UTF8, "application/json");

                        var response = await Redisclient.PostAsync($"{BaseUrl}", content);
                        _logger.LogInformation($"Usuário criado com sucesso: {responseContent}");

                        return Ok(resultado);
                    }
                }

                else if (user.Plano.MetodoPagamento == "Cartão de Crédito/Débito" && user.Plano.Id == "FF01")
                {
                    var checkoutRequest = new CheckoutRequest
                    {
                        BillingTypes = new List<string> { "CREDIT_CARD" },
                        ChargeTypes = new List<string> { "DETACHED" },
                        Callback = new Callback
                        {
                            SuccessUrl = "https://flash-fit-front.vercel.app/success",
                            CancelUrl = "https://flash-fit-front.vercel.app/error",
                            ExpiredUrl = "https://example.com/expired"
                        },
                        Items = new List<Item>
                        {
                            new Item
                            {
                                Description = "Plano de Treino personalizado para casa ou academia",
                                Name = "Plano de Treino",
                                Quantity = 1,
                                Value = 9.90m
                            }
                        }
                    };

                    var json = JsonSerializer.Serialize(checkoutRequest, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase // garante que vai sair em camelCase
                    });
                    var contentAsaas = new StringContent(json, Encoding.UTF8, "application/json");
                    Assasclient.DefaultRequestHeaders.Add("User-Agent", "FlashFit");
                    Assasclient.DefaultRequestHeaders.Add("access_token", BearerAsaas);

                    _logger.LogInformation(
                        "Chamada para API Asaas: Endpoint={Endpoint}, MetodoPagamento={MetodoPagamento}, PlanoId={PlanoId}, Body={Body}",
                        BaseUrlAsaas,
                        user.Plano.MetodoPagamento,
                        user.Plano.Id,
                        json
                    );

                    var requestAsaas = await Assasclient.PostAsync($"{BaseUrlAsaas}", contentAsaas);
                    var responseContent = await requestAsaas.Content.ReadAsStringAsync();


                    if (requestAsaas.IsSuccessStatusCode)
                    {

                        var resultado = JsonSerializer.Deserialize<CheckoutResponse>(responseContent);
                        user.User.Id = resultado.Id;


                        var Redisclient = _httpClientFactory.CreateClient();
                        Redisclient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", RedisKey);
                        string usuarioJson = JsonSerializer.Serialize(user);

                        var comando = new object[]
                        {
                        "SET",
                         $"{resultado.Id}",
                         usuarioJson
                        };

                        string comandoJson = JsonSerializer.Serialize(comando);
                        var content = new StringContent(comandoJson, Encoding.UTF8, "application/json");

                        var response = await Redisclient.PostAsync($"{BaseUrl}", content);
                        _logger.LogInformation($"Usuário criado com sucesso: {responseContent}");

                        return Ok(resultado);

                    }
                    else
                    {
                        _logger.LogError($"Erro ao criar Usuário: {requestAsaas.ReasonPhrase}");
                        return StatusCode((int)requestAsaas.StatusCode, "Erro ao criar usuário.");
                    }
                }

                else if (user.Plano.MetodoPagamento == "Cartão de Crédito/Débito" && user.Plano.Id == "FF02")
                {
                    var checkoutRequest = new CheckoutRequest
                    {
                        BillingTypes = new List<string> { "CREDIT_CARD" },
                        ChargeTypes = new List<string> { "DETACHED" },
                        Callback = new Callback
                        {
                            SuccessUrl = "https://flash-fit-front.vercel.app/success",
                            CancelUrl = "https://flash-fit-front.vercel.app/error",
                            ExpiredUrl = "https://example.com/expired"
                        },
                        Items = new List<Item>
                        {
                            new Item
                            {
                                Description = "Plano gerado com base em seus dados físicos e objetivo",
                                Name = "Plano Alimentar",
                                Quantity = 1,
                                Value = 9.90m
                            }
                        }
                    };
                    var json = JsonSerializer.Serialize(checkoutRequest, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase // garante que vai sair em camelCase
                    });
                    var contentAsaas = new StringContent(json, Encoding.UTF8, "application/json");
                    Assasclient.DefaultRequestHeaders.Add("User-Agent", "FlashFit");
                    Assasclient.DefaultRequestHeaders.Add("access_token", BearerAsaas);
                    _logger.LogInformation(
                        "Chamada para API Asaas: Endpoint={Endpoint}, MetodoPagamento={MetodoPagamento}, PlanoId={PlanoId}, Body={Body}",
                        BaseUrlAsaas,
                        user.Plano.MetodoPagamento,
                        user.Plano.Id,
                        json
                    );
                    var requestAsaas = await Assasclient.PostAsync($"{BaseUrlAsaas}", contentAsaas);
                    var responseContent = await requestAsaas.Content.ReadAsStringAsync();
                    if (requestAsaas.IsSuccessStatusCode)
                    {
                        var resultado = JsonSerializer.Deserialize<CheckoutResponse>(responseContent);
                        user.User.Id = resultado.Id;

                        var Redisclient = _httpClientFactory.CreateClient();
                        Redisclient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", RedisKey);
                        string usuarioJson = JsonSerializer.Serialize(user);

                        var comando = new object[]
                        {
                        "SET",
                         $"{resultado.Id}",
                         usuarioJson
                        };

                        string comandoJson = JsonSerializer.Serialize(comando);
                        var content = new StringContent(comandoJson, Encoding.UTF8, "application/json");

                        var response = await Redisclient.PostAsync($"{BaseUrl}", content);
                        _logger.LogInformation($"Usuário criado com sucesso: {responseContent}");

                        return Ok(resultado);

                    }
                    else
                    {
                        _logger.LogError($"Erro ao criar Usuário: {requestAsaas.ReasonPhrase}");
                        return StatusCode((int)requestAsaas.StatusCode, "Erro ao criar usuário.");
                    }


                }

                else if (user.Plano.MetodoPagamento == "Pix" && user.Plano.Id == "FF02")
                {
                    var checkoutRequest = new CheckoutRequest
                    {
                        BillingTypes = new List<string> { "PIX" },
                        ChargeTypes = new List<string> { "DETACHED" },
                        Callback = new Callback
                        {
                            SuccessUrl = "https://flash-fit-front.vercel.app/success",
                            CancelUrl = "https://flash-fit-front.vercel.app/error",
                            ExpiredUrl = "https://example.com/expired"
                        },
                        Items = new List<Item>
                        {
                            new Item
                            {
                                Description = "Plano gerado com base em seus dados físicos e objetivo",
                                Name = "Plano Alimentar",
                                Quantity = 1,
                                Value = 9.90m
                            }
                        }
                    };
                    var json = JsonSerializer.Serialize(checkoutRequest, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase // garante que vai sair em camelCase
                    });
                    var contentAsaas = new StringContent(json, Encoding.UTF8, "application/json");
                    Assasclient.DefaultRequestHeaders.Add("User-Agent", "FlashFit");
                    Assasclient.DefaultRequestHeaders.Add("access_token", BearerAsaas);
                    _logger.LogInformation(
                        "Chamada para API Asaas: Endpoint={Endpoint}, MetodoPagamento={MetodoPagamento}, PlanoId={PlanoId}, Body={Body}",
                        BaseUrlAsaas,
                        user.Plano.MetodoPagamento,
                        user.Plano.Id,
                        json
                    );
                    var requestAsaas = await Assasclient.PostAsync($"{BaseUrlAsaas}", contentAsaas);
                    var responseContent = await requestAsaas.Content.ReadAsStringAsync();
                    if (requestAsaas.IsSuccessStatusCode)
                    {
                        var resultado = JsonSerializer.Deserialize<CheckoutResponse>(responseContent);
                        user.User.Id = resultado.Id;
                        var Redisclient = _httpClientFactory.CreateClient();
                        Redisclient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", RedisKey);
                        string usuarioJson = JsonSerializer.Serialize(user);
                        var comando = new object[]
                        {
                        "SET",
                         $"{resultado.Id}",
                         usuarioJson
                        };
                        string comandoJson = JsonSerializer.Serialize(comando);
                        var content = new StringContent(comandoJson, Encoding.UTF8, "application/json");
                        var response = await Redisclient.PostAsync($"{BaseUrl}", content);
                        _logger.LogInformation($"Usuário criado com sucesso: {responseContent}");
                        return Ok(resultado);
                    }
                    else
                    {
                        _logger.LogError("Método de pagamento ou plano inválido.");
                        return BadRequest("Método de pagamento ou plano inválido.");

                    }
                }

                else if (user.Plano.MetodoPagamento == "Cartão de Crédito/Débito" && user.Plano.Id == "FF03")
                {
                    var checkoutRequest = new CheckoutRequest
                    {
                        BillingTypes = new List<string> { "CREDIT_CARD" },
                        ChargeTypes = new List<string> { "DETACHED" },
                        Callback = new Callback
                        {
                            SuccessUrl = "https://flash-fit-front.vercel.app/success",
                            CancelUrl = "https://flash-fit-front.vercel.app/error",
                            ExpiredUrl = "https://example.com/expired"
                        },
                        Items = new List<Item>
                        {
                            new Item
                            {
                                Description = "Treino + Dieta personalizados em um só pacote",
                                Name = "Combo Flash Fit",
                                Quantity = 1,
                                Value = 14.90m
                            }
                        }
                    };
                    var json = JsonSerializer.Serialize(checkoutRequest, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase // garante que vai sair em camelCase
                    });
                    var contentAsaas = new StringContent(json, Encoding.UTF8, "application/json");
                    Assasclient.DefaultRequestHeaders.Add("User-Agent", "FlashFit");
                    Assasclient.DefaultRequestHeaders.Add("access_token", BearerAsaas);
                    _logger.LogInformation(
                        "Chamada para API Asaas: Endpoint={Endpoint}, MetodoPagamento={MetodoPagamento}, PlanoId={PlanoId}, Body={Body}",
                        BaseUrlAsaas,
                        user.Plano.MetodoPagamento,
                        user.Plano.Id,
                        json
                    );
                    var requestAsaas = await Assasclient.PostAsync($"{BaseUrlAsaas}", contentAsaas);
                    var responseContent = await requestAsaas.Content.ReadAsStringAsync();
                    if (requestAsaas.IsSuccessStatusCode)
                    {
                        var resultado = JsonSerializer.Deserialize<CheckoutResponse>(responseContent);
                        user.User.Id = resultado.Id;
                        var Redisclient = _httpClientFactory.CreateClient();
                        Redisclient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", RedisKey);
                        string usuarioJson = JsonSerializer.Serialize(user);
                        var comando = new object[]
                        {
                        "SET",
                         $"{resultado.Id}",
                         usuarioJson
                        };
                        string comandoJson = JsonSerializer.Serialize(comando);
                        var content = new StringContent(comandoJson, Encoding.UTF8, "application/json");
                        var response = await Redisclient.PostAsync($"{BaseUrl}", content);
                        _logger.LogInformation($"Usuário criado com sucesso: {responseContent}");
                        return Ok(resultado);
                    }
                }

                else if (user.Plano.MetodoPagamento == "Pix" && user.Plano.Id == "FF03")
                {
                    var checkoutRequest = new CheckoutRequest
                    {
                        BillingTypes = new List<string> { "PIX" },
                        ChargeTypes = new List<string> { "DETACHED" },
                        Callback = new Callback
                        {
                            SuccessUrl = "https://flash-fit-front.vercel.app/success",
                            CancelUrl = "https://flash-fit-front.vercel.app/error",
                            ExpiredUrl = "https://example.com/expired"
                        },
                        Items = new List<Item>
                        {
                            new Item
                            {
                               Description = "Treino + Dieta personalizados em um só pacote",
                                Name = "Combo Flash Fit",
                                Quantity = 1,
                                Value = 14.90m
                            }
                        }
                    };
                    var json = JsonSerializer.Serialize(checkoutRequest, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase // garante que vai sair em camelCase
                    });
                    var contentAsaas = new StringContent(json, Encoding.UTF8, "application/json");
                    Assasclient.DefaultRequestHeaders.Add("User-Agent", "FlashFit");
                    Assasclient.DefaultRequestHeaders.Add("access_token", BearerAsaas);
                    _logger.LogInformation(
                        "Chamada para API Asaas: Endpoint={Endpoint}, MetodoPagamento={MetodoPagamento}, PlanoId={PlanoId}, Body={Body}",
                        BaseUrlAsaas,
                        user.Plano.MetodoPagamento,
                        user.Plano.Id,
                        json
                    );
                    var requestAsaas = await Assasclient.PostAsync($"{BaseUrlAsaas}", contentAsaas);
                    var responseContent = await requestAsaas.Content.ReadAsStringAsync();
                    if (requestAsaas.IsSuccessStatusCode)
                    {
                        var resultado = JsonSerializer.Deserialize<CheckoutResponse>(responseContent);
                        user.User.Id = resultado.Id;
                        var Redisclient = _httpClientFactory.CreateClient();
                        Redisclient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", RedisKey);
                        string usuarioJson = JsonSerializer.Serialize(user);
                        var comando = new object[]
                        {
                        "SET",
                         $"{resultado.Id}",
                         usuarioJson
                        };
                        string comandoJson = JsonSerializer.Serialize(comando);
                        var content = new StringContent(comandoJson, Encoding.UTF8, "application/json");
                        var response = await Redisclient.PostAsync($"{BaseUrl}", content);
                        _logger.LogInformation($"Usuário criado com sucesso: {responseContent}");
                        return Ok(resultado);
                    }
                }
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao criar o usuário.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Erro interno do servidor.");
            }
            return Ok();
        }
        [HttpGet]
        [Route("get/{uiid}")]
        public async Task<IActionResult> GetUser(string uiid)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", RedisKey);

                var response = await client.GetAsync($"{BaseUrl}/get/{uiid}");
                //
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var outerJson = JsonDocument.Parse(content);
                    var resultString = outerJson.RootElement.GetProperty("result").GetString();
                    
                    // 2º passo: desserializar o conteúdo JSON original
                    var resultJson = JsonDocument.Parse(resultString);

                    return Ok(resultJson.RootElement);
                } 
                else
                {
                    _logger.LogError($"Erro ao obter usuário: {response.ReasonPhrase}");
                    return StatusCode((int)response.StatusCode, "Erro ao obter usuário.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao obter o usuário.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Erro interno do servidor.");
            }
        }
    }
}

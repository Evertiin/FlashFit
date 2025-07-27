using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FlashFit.Models;

namespace FlashFit.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        [HttpPost]
        public IActionResult ReceiveWebhook([FromBody] WebhookResponseDto payload)
        {
            var id = payload.Id;

            return Ok(id);
        }
    }
}

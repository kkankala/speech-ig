// Generated with EmptyBot .NET Template version v4.22.0

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using WeatherBot.Bots;

namespace WeatherBot.Controllers;

// This ASP Controller is created to handle a request. Dependency Injection will provide the Adapter and IBot
// implementation at runtime. Multiple different IBot implementations running at different endpoints can be
// achieved by specifying a more specific type for the bot constructor argument.
[Route("/api/webhook")]
[ApiController]
public class WebhookController : ControllerBase
{
    private readonly IBotFrameworkHttpAdapter _adapter;
    private readonly IBot _bot;

    public WebhookController(IBotFrameworkHttpAdapter adapter, IBot bot)
    {
        _adapter = adapter;
        _bot = bot;
    }

    [HttpPost]
    [HttpGet]
    public async Task<IActionResult> PostAsync()
    {
        await (_bot as WebhookBot).ProcessWebhookMessage(Request);
        return Ok();
    }
}


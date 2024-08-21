// Generated with EmptyBot .NET Template version v4.22.0

using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using WeatherBot.Bots;

namespace WeatherBot.Controllers;

// This ASP Controller is created to handle a request. Dependency Injection will provide the Adapter and IBot
// implementation at runtime. Multiple different IBot implementations running at different endpoints can be
// achieved by specifying a more specific type for the bot constructor argument.
[Route("/api/notify")]
[ApiController]
public class NotifyController : ControllerBase
{
    private readonly IBotFrameworkHttpAdapter _adapter;
    private readonly string _appId;
    private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;

    public NotifyController(IBotFrameworkHttpAdapter adapter, ConcurrentDictionary<string, ConversationReference> conversationReferences)
    {
        _adapter = adapter;
        _conversationReferences = conversationReferences;
        _appId = string.Empty;
    }

    [HttpPost]
    [HttpGet]
    public async Task<IActionResult> PostAsync()
    {
        using (var reader = new StreamReader(Request.Body))
        {
            var body = await reader.ReadToEndAsync();
            dynamic webhookMessage = JsonConvert.DeserializeObject(body);
            foreach (var conversationReference in _conversationReferences.Values)
            {
                var activity = MessageFactory.Text("Forwarded: " + (string)webhookMessage.text);
                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, async (turnContext, cancellationToken) =>
                {
                    await turnContext.SendActivityAsync(activity, cancellationToken);
                }, default(CancellationToken));
            }
        }

        // Let the caller know proactive messages have been sent
        return new ContentResult()
        {
            Content = "<html><body><h1>Proactive messages have been sent.</h1></body></html>",
            ContentType = "text/html",
            StatusCode = (int)HttpStatusCode.OK,
        };
        // return Ok();
    }

    private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
    {
        await turnContext.SendActivityAsync("proactive hello");
    }
}


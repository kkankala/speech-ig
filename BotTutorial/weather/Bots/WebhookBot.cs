using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace WeatherBot.Bots;
public class WebhookBot : ActivityHandler
{
    private readonly IBotFrameworkHttpAdapter _adapter;
    private readonly string _botAppId = null; // For local testing, this can be null or the bot's app ID if applicable
    private readonly string _serviceUrl = "http://localhost:3978";
    private readonly string _channelId = "emulator";

    public WebhookBot(IBotFrameworkHttpAdapter adapter)
    {
        _adapter = adapter;
    }

    public async Task ProcessWebhookMessage(HttpRequest request)
    {
        using (var reader = new StreamReader(request.Body))
        {
            var body = await reader.ReadToEndAsync();
            dynamic webhookMessage = JsonConvert.DeserializeObject(body);

            // Create a conversation reference
            var conversationReference = new ConversationReference
            {
                ChannelId = _channelId,
                ServiceUrl = _serviceUrl,
                User = new ChannelAccount("user1", "User"),
                Bot = new ChannelAccount("bot", "Bot"),
                Conversation = new ConversationAccount(id: "convo1")
            };

            var activity = MessageFactory.Text((string)webhookMessage.text);
            activity.Conversation = new ConversationAccount(id: "convo1");
            activity.ChannelId = _channelId;
            activity.From = new ChannelAccount("user1", "User");
            activity.Recipient = new ChannelAccount("bot", "Bot");
            activity.ServiceUrl = _serviceUrl;


            // This is not working.
            // Send the activity to the emulator
            await ((BotAdapter)_adapter).ContinueConversationAsync(
                _botAppId,
                conversationReference,
                async (turnContext, cancellationToken) =>
                {
                    await turnContext.SendActivityAsync(activity, cancellationToken);
                },
                CancellationToken.None);
        }
    }

    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        var replyText = $"You said: {turnContext.Activity.Text}";
        await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
    }
}
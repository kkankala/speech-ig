using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WeatherBot.State;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace WeatherBot.Bots;

public class WelcomeUserBot(UserState userState) : ActivityHandler
{
    // Messages sent to the user.
    private const string WelcomeMessage = "This is a simple Welcome Bot sample. This bot will introduce you " +
                                            "to welcoming and greeting users. You can say 'intro' to see the " +
                                            "introduction card. If you are running this bot in the Bot Framework " +
                                            "Emulator, press the 'Start Over' button to simulate user joining " +
                                            "a bot or a channel";

    private const string InfoMessage = "You are seeing this message because the bot received at least one " +
                                        "'ConversationUpdate' event, indicating you (and possibly others) " +
                                        "joined the conversation. If you are using the emulator, pressing " +
                                        "the 'Start Over' button to trigger this event again. The specifics " +
                                        "of the 'ConversationUpdate' event depends on the channel. You can " +
                                        "read more information at: " +
                                        "https://aka.ms/about-botframework-welcome-user";

    private const string LocaleMessage = "You can use the activity's 'GetLocale()' method to welcome the user " +
                                         "using the locale received from the channel. " +
                                         "If you are using the Emulator, you can set this value in Settings.";

    private const string PatternMessage = "It is a good pattern to use this event to send general greeting" +
                                          "to user, explaining what your bot can do. In this example, the bot " +
                                          "handles 'hello', 'hi', 'help' and 'intro'. Try it now, type 'hi'";

    private readonly BotState _userState = userState;

    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        var welcomeUserStateAccessor = _userState.CreateProperty<WelcomeUserState>(nameof(WelcomeUserState));
        var didBotWelcomeUser = await welcomeUserStateAccessor.GetAsync(turnContext, () => new WelcomeUserState(), cancellationToken);
        if (!didBotWelcomeUser.DidBotWelcomeUser)
        {
            didBotWelcomeUser.DidBotWelcomeUser = true;

            // the channel should sends the user name in the 'From' object
            var userName = turnContext.Activity.From.Name;

            await turnContext.SendActivityAsync("You are seeing this message because this was your first message ever to this bot.", cancellationToken: cancellationToken);
            await turnContext.SendActivityAsync($"It is a good practice to welcome the user and provide personal greeting. For example, welcome {userName}.", cancellationToken: cancellationToken);
        }
        else
        {
            // This example hardcodes specific utterances. You should use LUIS or QnA for more advance language understanding.
            var text = turnContext.Activity.Text.ToLowerInvariant();
            switch (text)
            {
                case "hello":
                case "hi":
                    await turnContext.SendActivityAsync($"You said {text}.", cancellationToken: cancellationToken);
                    break;
                case "intro":
                case "help":
                    await SendIntroCardAsync(turnContext, cancellationToken);
                    break;
                default:
                    await turnContext.SendActivityAsync(WelcomeMessage, cancellationToken: cancellationToken);
                    break;
            }
        }

        // Save any state changes.
        await _userState.SaveChangesAsync(turnContext, cancellationToken: cancellationToken);
    }

    private static async Task SendIntroCardAsync(ITurnContext turnContext, CancellationToken cancellationToken)
    {
        var card = new HeroCard
        {
            Title = "Welcome to Bot Framework!",
            Text = @"Welcome to Welcome Users bot sample! This Introduction card
                         is a great way to introduce your Bot to the user and suggest
                         some things to get them started. We use this opportunity to
                         recommend a few next steps for learning more creating and deploying bots.",
            Images = new List<CardImage>() { new CardImage("https://aka.ms/bf-welcome-card-image") },
            Buttons = new List<CardAction>()
                {
                    new CardAction(ActionTypes.OpenUrl, "Get an overview", null, "Get an overview", "Get an overview", "https://docs.microsoft.com/en-us/azure/bot-service/?view=azure-bot-service-4.0"),
                    new CardAction(ActionTypes.OpenUrl, "Ask a question", null, "Ask a question", "Ask a question", "https://stackoverflow.com/questions/tagged/botframework"),
                    new CardAction(ActionTypes.OpenUrl, "Learn how to deploy", null, "Learn how to deploy", "Learn how to deploy", "https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-deploy-azure?view=azure-bot-service-4.0"),
                }
        };

        var response = MessageFactory.Attachment(card.ToAttachment());
        await turnContext.SendActivityAsync(response, cancellationToken);
    }

    protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
    {
        foreach (var member in membersAdded)
        {
            if (member.Id != turnContext.Activity.Recipient.Id)
            {
                await turnContext.SendActivityAsync($"Hi there - {member.Name}. {WelcomeMessage}", cancellationToken: cancellationToken);
                await turnContext.SendActivityAsync(InfoMessage, cancellationToken: cancellationToken);
                await turnContext.SendActivityAsync($"{LocaleMessage} Current locale is '{turnContext.Activity.GetLocale()}'.", cancellationToken: cancellationToken);
                await turnContext.SendActivityAsync(PatternMessage, cancellationToken: cancellationToken);
            }
        }
    }
}
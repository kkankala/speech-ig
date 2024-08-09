using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace WeatherBot.Bots;

public class ProactiveBot(ConcurrentDictionary<string, ConversationReference> conversationReferences, ConversationState conversationState, UserState userState) : ActivityHandler
{
    private const string WelcomeMessage = "Welcome to the Proactive Bot sample.  Navigate to http://localhost:3978/api/notify to proactively message everyone who has previously messaged this bot.";
    // Dependency injected dictionary for storing ConversationReference objects used in NotifyController to proactively message users
    private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences = conversationReferences;
    private readonly ConversationState _conversationState = conversationState;
    private readonly UserState _userState = userState;

    public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
    {
        await base.OnTurnAsync(turnContext, cancellationToken);

        // Save any state changes that might have occurred during the turn.
        await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
    }

    protected override Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
    {
        AddConversationReference(turnContext.Activity as Activity);

        return base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
    }

    protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
    {
        foreach (var member in membersAdded)
        {
            // Greet anyone that was not the target (recipient) of this message.
            if (member.Id != turnContext.Activity.Recipient.Id)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(WelcomeMessage), cancellationToken);
            }
        }
    }

    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        AddConversationReference(turnContext.Activity as Activity);

        // Echo back what the user said
        await turnContext.SendActivityAsync(MessageFactory.Text($"You sent '{turnContext.Activity.Text}'"), cancellationToken);
    }

    private void AddConversationReference(Activity activity)
    {
        var conversationReference = activity.GetConversationReference();
        _conversationReferences.AddOrUpdate(conversationReference.User.Id, conversationReference, (key, newValue) => conversationReference);
    }
}
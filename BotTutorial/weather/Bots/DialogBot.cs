using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace WeatherBot.Bots;

public class DialogBot<T>(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger) : ActivityHandler where T : Dialog
{
    protected readonly Dialog Dialog = dialog;
    protected readonly BotState ConversationState = conversationState;
    protected readonly BotState UserState = userState;
    protected readonly ILogger Logger = logger;

    public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
    {
        await base.OnTurnAsync(turnContext, cancellationToken);

        // Save any state changes that might have occurred during the turn.
        await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
    }

    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Running dialog with Message Activity.");

        // Run the Dialog with the new message Activity.
        await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
    }
}
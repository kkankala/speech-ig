using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WeatherBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;

namespace WeatherBot.Dialogs;

public class MainDialog : ComponentDialog
{
    private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;

    public MainDialog(UserState userState)
        : base(nameof(MainDialog))
    {
        _userProfileAccessor = userState.CreateProperty<UserProfile>(nameof(UserProfile));

        AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
        {
            ChoiceCardStepAsync,
            ShowCardStepAsync
        }));

        // The initial child Dialog to run.
        InitialDialogId = nameof(WaterfallDialog);
    }

    private async Task<DialogTurnResult> ChoiceCardStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var options = new PromptOptions()
        {
            Prompt = MessageFactory.Text("What card would you like to see? You can click or type the card name"),
            RetryPrompt = MessageFactory.Text("That was not a valid choice, please select a card or number from 1 to 9."),
            Choices = GetChoices(),
        };

        // Prompt the user with the configured PromptOptions.
        return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
    }

    private async Task<DialogTurnResult> ShowCardStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        // Cards are sent as Attachments in the Bot Framework.
        // So we need to create a list of attachments for the reply activity.
        var attachments = new List<Attachment>();

        // Reply to the activity we received with an activity.
        var reply = MessageFactory.Attachment(attachments);

        // Decide which type of card(s) we are going to show the user
        switch (((FoundChoice)stepContext.Result).Value)
        {
            case "Adaptive Card":
                // Display an Adaptive Card
                reply.Attachments.Add(Cards.CreateAdaptiveCardAttachment());
                break;
            case "Animation Card":
                // Display an AnimationCard.
                reply.Attachments.Add(Cards.GetAnimationCard().ToAttachment());
                break;
            case "Audio Card":
                // Display an AudioCard
                reply.Attachments.Add(Cards.GetAudioCard().ToAttachment());
                break;
            case "Hero Card":
                // Display a HeroCard.
                reply.Attachments.Add(Cards.GetHeroCard().ToAttachment());
                break;
            case "OAuth Card":
                // Display an OAuthCard
                reply.Attachments.Add(Cards.GetOAuthCard().ToAttachment());
                break;
            case "Receipt Card":
                // Display a ReceiptCard.
                reply.Attachments.Add(Cards.GetReceiptCard().ToAttachment());
                break;
            case "Signin Card":
                // Display a SignInCard.
                reply.Attachments.Add(Cards.GetSigninCard().ToAttachment());
                break;
            case "Thumbnail Card":
                // Display a ThumbnailCard.
                reply.Attachments.Add(Cards.GetThumbnailCard().ToAttachment());
                break;
            case "Video Card":
                // Display a VideoCard
                reply.Attachments.Add(Cards.GetVideoCard().ToAttachment());
                break;
            default:
                // Display a carousel of all the rich card types.
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                reply.Attachments.Add(Cards.CreateAdaptiveCardAttachment());
                reply.Attachments.Add(Cards.GetAnimationCard().ToAttachment());
                reply.Attachments.Add(Cards.GetAudioCard().ToAttachment());
                reply.Attachments.Add(Cards.GetHeroCard().ToAttachment());
                reply.Attachments.Add(Cards.GetOAuthCard().ToAttachment());
                reply.Attachments.Add(Cards.GetReceiptCard().ToAttachment());
                reply.Attachments.Add(Cards.GetSigninCard().ToAttachment());
                reply.Attachments.Add(Cards.GetThumbnailCard().ToAttachment());
                reply.Attachments.Add(Cards.GetVideoCard().ToAttachment());
                break;
        }

        // Send the card(s) to the user as an attachment to the activity
        await stepContext.Context.SendActivityAsync(reply, cancellationToken);

        // Give the user instructions about what to do next
        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Type anything to see another card."), cancellationToken);

        return await stepContext.EndDialogAsync();
    }

    private IList<Choice> GetChoices()
    {
        return
        [
            new Choice() { Value = "Adaptive Card", Synonyms = ["adaptive"]},
            new Choice() { Value = "Animation Card",Synonyms= ["animation"]},
            new Choice() { Value = "Audio Card", Synonyms = ["audio"] },
            new Choice() { Value = "Hero Card", Synonyms = ["hero"] },
            new Choice() { Value = "OAuth Card", Synonyms = ["oauth"] },
            new Choice() { Value = "Receipt Card", Synonyms = ["receipt"] },
            new Choice() { Value = "Signin Card", Synonyms = ["signin"] },
            new Choice() { Value = "Thumbnail Card", Synonyms = ["thumbnail", "thumb"] },
            new Choice() { Value = "Video Card", Synonyms = ["video"] },
            new Choice() { Value = "All cards", Synonyms = ["all"] },
        ];

    }
}
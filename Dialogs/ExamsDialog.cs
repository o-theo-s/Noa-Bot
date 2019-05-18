using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Noa.Helpers;

namespace Noa.Dialogs
{
    public class ExamsDialog : ComponentDialog
    {
        public ExamsDialog() : base(nameof(ExamsDialog))
        {
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                RedirectStepAsync,
                FinalStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var data = stepContext.Options as CardMetaData;

            if (data.CardInfo.ChildCards.Contains("Βαθμοί"))
                return await HistoryTestsStepAsync(stepContext, cancellationToken);

            return await UpcomingTestsStepAsync(stepContext, cancellationToken);
        }

        private async Task<DialogTurnResult> HistoryTestsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var data = stepContext.Options as CardMetaData;

            var cardFormat = new Dictionary<string, string>(1) { { "course", data.Course } };
            var courseCard = MessageFactory.Attachment(DialogHelper.CreateAdaptiveCardAttachment("test-history.json", cardFormat));
            await stepContext.Context.SendActivityAsync(courseCard);

            return await stepContext.NextAsync(0, cancellationToken);
        }

        private async Task<DialogTurnResult> UpcomingTestsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var data = stepContext.Options as CardMetaData;

            var cardFormat = new Dictionary<string, string>(1) { { "course", data.Course } };
            var courseCard = MessageFactory.Attachment(DialogHelper.CreateAdaptiveCardAttachment("test-upcoming.json", cardFormat));
            await stepContext.Context.SendActivityAsync(courseCard);

            return await stepContext.NextAsync(0, cancellationToken);
        }

        private async Task<DialogTurnResult> RedirectStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptOptions = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Πώς θα ήθελες να συνεχίσεις;"),
                Choices = new List<Choice> { new Choice("Αρχική") }
            };
            return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(0, cancellationToken);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Newtonsoft.Json;
using Noa.Helpers;

namespace Noa.Dialogs
{
    public class MainDialog : InterruptDialog
    {
        public MainDialog() : base(nameof(MainDialog))
        {
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt), DialogHelper.SpecialGreekValidatorAsync));
            AddDialog(new CoursesDialog());
            AddDialog(new ScheduleDialog());
            AddDialog(new ExamsDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ModeStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> ModeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var cardFormat = new Dictionary<string, string>(2)
            {
                { "tomorrow", DateTime.Today.NextWorkingDay().AddDays(1.0).NextWorkingDay().ToString("yyyy-MM-dd") },
                { "schedule_date", DateTime.Today.NextWorkingDay().ToGreekDate() }
            };
            var courseCard = MessageFactory.Attachment(DialogHelper.CreateAdaptiveCardAttachment("main.json", cardFormat));
            await stepContext.Context.SendActivityAsync(courseCard);
            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var data = stepContext.Options as CardMetaData;

            if (data.CardInfo.ChildCards.Contains("Μαθήματα"))
                return await stepContext.BeginDialogAsync(nameof(CoursesDialog), data, cancellationToken);
            if (data.CardInfo.ChildCards.Contains("Πρόγραμμα"))
                return await stepContext.BeginDialogAsync(nameof(ScheduleDialog), data, cancellationToken);
            if (data.CardInfo.ChildCards.Contains("Διαγωνίσματα"))
                return await stepContext.BeginDialogAsync(nameof(ExamsDialog), data, cancellationToken);
            
            return await FinalStepAsync(stepContext, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            int returnCode = 0;
            if (stepContext.Result is int)
                returnCode = (int)stepContext.Result;

            if (returnCode == 0)
                return await stepContext.ReplaceDialogAsync(nameof(WaterfallDialog), null, cancellationToken);
            else
                return await stepContext.RepeatDialogAsync(1, cancellationToken);
        }
    }
}

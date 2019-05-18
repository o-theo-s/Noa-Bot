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
    public class ScheduleDialog : ComponentDialog
    {
        public ScheduleDialog() : base(nameof(ScheduleDialog))
        {
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new WaterfallDialog(Waterfalls.MainWaterfall, new WaterfallStep[]
            {
                IntroStepAsync,
                RedirectStepAsync,
                FinalStepAsync
            }));
            AddDialog(new WaterfallDialog(Waterfalls.DayWaterfall, new WaterfallStep[]
            {
                DayStepAsync,
                IntroStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private class Waterfalls
        {
            public const string MainWaterfall = nameof(WaterfallDialog);
            public const string DayWaterfall = "Day" + nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var data = stepContext.Options as CardMetaData;

            if (data.CardInfo.Action == "Εβδομαδιαίο")
                return await WeekStepAsync(stepContext, cancellationToken);

            return await stepContext.BeginDialogAsync(Waterfalls.DayWaterfall, data, cancellationToken);
        }

        private async Task<DialogTurnResult> DayStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var data = stepContext.Options as CardMetaData;

            if (data.ScheduleDate.Value.DayOfWeek == DayOfWeek.Saturday || data.ScheduleDate.Value.DayOfWeek == DayOfWeek.Sunday)
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Εμφάνιση επόμενης σχολικής ημέρας από {data.ScheduleDate?.ToGreekDate()}..."));

            var cardFormat = new Dictionary<string, string>(2)
            {
                { "tomorrow", DateTime.Today.NextWorkingDay().AddDays(1.0).NextWorkingDay().ToString("yyyy-MM-dd") },
                { "schedule_date", data.ScheduleDate?.NextWorkingDay().ToGreekDate() }
            };
            var courseCard = MessageFactory.Attachment(DialogHelper.CreateAdaptiveCardAttachment("schedule-day.json", cardFormat));
            await stepContext.Context.SendActivityAsync(courseCard);
            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }

        private async Task<DialogTurnResult> WeekStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var cardFormat = new Dictionary<string, string>(1)
            {
                { "schedule_week", DateTime.Today.ToGreekWeekDate() }
            };
            var courseCard = MessageFactory.Attachment(DialogHelper.CreateAdaptiveCardAttachment("schedule-week.json", cardFormat));
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

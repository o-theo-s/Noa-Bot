using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Noa.Helpers;
using static Noa.Dialogs.ChoicesWrapper;
using static Noa.Dialogs.ChoicesWrapper.CoursesChoices;

namespace Noa.Dialogs
{
    public class CoursesDialog : ComponentDialog
    {
        public CoursesDialog() : base(nameof(CoursesDialog))
        {
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt), DialogHelper.SpecialGreekValidatorAsync));
            AddDialog(new WaterfallDialog(Waterfalls.MainWaterfall, new WaterfallStep[]
            {
                SelectModeStepAsync,
                ModeRedirectStepAsync,
                FinalStepAsync
            }));
            AddDialog(new WaterfallDialog(Waterfalls.TheoryWaterfall, new WaterfallStep[]
            {
                Theory_InitialStepAsync,
                RedirectStepAsync,
                FinalStepAsync
            }));
            AddDialog(new WaterfallDialog(Waterfalls.ExercisesWaterfall, new WaterfallStep[]
            {
                Exercises_InitialStepAsync,
                Exercises_HelpStepAsync,
                RedirectStepAsync,
                FinalStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = Waterfalls.MainWaterfall;
        }

        private class Waterfalls
        {
            public const string MainWaterfall = nameof(WaterfallDialog);
            public const string TheoryWaterfall = "Theory" + nameof(WaterfallDialog);
            public const string ExercisesWaterfall = "Exercises" + nameof(WaterfallDialog);
        }

        #region Main Waterfall steps
        private async Task<DialogTurnResult> SelectModeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var data = stepContext.Options as CardMetaData;
            stepContext.Values.Add("Course", data.Course);

            var cardFormat = new Dictionary<string, string>(1) { { "course", data.Course } };
            var courseCard = MessageFactory.Attachment(DialogHelper.CreateAdaptiveCardAttachment("course.json", cardFormat));

            await stepContext.Context.SendActivityAsync(courseCard);
            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }

        private async Task<DialogTurnResult> ModeRedirectStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var data = stepContext.Options as CardMetaData;
            stepContext.Values.Add("Chapter", data.Chapter);

            if (data.CardInfo.Action == "Θεωρία")
                return await stepContext.BeginDialogAsync(Waterfalls.TheoryWaterfall, data, cancellationToken);

            if (data.CardInfo.ChildCards.Contains("Ασκήσεις"))
                return await stepContext.BeginDialogAsync(Waterfalls.ExercisesWaterfall, data, cancellationToken);

            return await FinalStepAsync(stepContext, cancellationToken);
        }
        #endregion


        #region Theory Waterfall steps
        private async Task<DialogTurnResult> Theory_InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var data = stepContext.Options as CardMetaData;
            foreach (var value in stepContext.Stack[1].State["values"] as IDictionary<string, object>)
                stepContext.Values.Add(value);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Βοήθεια για τη θεωρία: Κεφάλαιο {data.Chapter}"));
            return await stepContext.NextAsync(null, cancellationToken);
        }
        #endregion


        #region Exercises Waterfall steps
        private async Task<DialogTurnResult> Exercises_InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var data = stepContext.Options as CardMetaData;
            if (stepContext.Values.Count == 0)
                foreach (var value in stepContext.Stack[1].State["values"] as IDictionary<string, object>)
                    stepContext.Values.Add(value);
            stepContext.Values.TryAdd("HelpMode", data.CardInfo.Action);

            var cardFormat = new Dictionary<string, string>(1)
            {
                { "course", (string)stepContext.Values["Course"] },
                { "help_mode", (string)stepContext.Values["HelpMode"] },
                { "chapter", ((string)stepContext.Values["Chapter"]).Remove(0, 3) }
            };
            var courseCard = MessageFactory.Attachment(DialogHelper.CreateAdaptiveCardAttachment("course-exercises.json", cardFormat));

            await stepContext.Context.SendActivityAsync(courseCard);
            return new DialogTurnResult(DialogTurnStatus.Waiting);
        }

        private async Task<DialogTurnResult> Exercises_HelpStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var data = stepContext.Options as CardMetaData;
            
            //TODO: Check for invalid exercise number

            if (data.Exercise == null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Παρακαλώ συμπλήρωσε τον αριθμό της άσκησης που χρειάζεσαι βοήθεια:"));
                return await stepContext.RepeatDialogAsync(2, cancellationToken);
            }
            stepContext.Values.Add("Exercise", data.Exercise);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"{data.CardInfo.Action} για την" +
                $" {((string)stepContext.Values["HelpMode"]).ToLower().Replace("ες", "η")} άσκηση" +
                $" {stepContext.Values["Exercise"]} στο κεφάλαιο {stepContext.Values["Chapter"]}"));
            return await stepContext.NextAsync(null, cancellationToken);
        }
        #endregion


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

    internal static partial class ChoicesWrapper
    {
        public static class CoursesChoices
        {
            public static class Course
            {
                public static readonly Choice Βιολογία = new Choice("Βιολογία");
                public static readonly Choice Έκθεση = new Choice("Έκθεση");
                public static readonly Choice Φυσική = new Choice("Φυσική");
            }

            public static class HelpMode
            {
                public static readonly Choice Θεωρία = new Choice("Θεωρία") { Synonyms = new List<string>() { "Theory" } };
                public static readonly Choice Ασκήσεις = new Choice("Ασκήσεις") { Synonyms = new List<string>() { "Exercises" } };
                //public static readonly Choice Μεθοδολογία = new Choice("Μεθοδολογία") { Synonyms = new List<string>() { "Methodology" } };
            }

            public static class ExerciseType
            {
                public static readonly Choice Λυμένη = new Choice("Λυμένη") { Synonyms = new List<string>() { "Λυμένες", "Solved" } };
                public static readonly Choice Άλυτη = new Choice("Άλυτη") { Synonyms = new List<string>() { "Άλυτες", "Unsolved" } };
            }
        }
    }
}

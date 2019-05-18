using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Noa.Helpers
{
    internal static class DialogHelper
    {
        public static Attachment CreateAdaptiveCardAttachment(string cardName, Dictionary<string, string> format = null)
        {
            // combine path for cross platform support
            string fullPath = Path.Combine(".", "Cards", cardName);
            var adaptiveCard = File.ReadAllText(fullPath);

            if (format != null)
                foreach (var keyword in format)
                    adaptiveCard = adaptiveCard.Replace("{" + keyword.Key + "}", keyword.Value);

            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }

        public static Task<bool> SpecialGreekValidatorAsync(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Recognized.Succeeded)
                return Task.FromResult(true);

            bool validate = false;
            try
            {
                int i = 0;
                for (; i < promptContext.Options.Choices.Count && !validate; i++)
                    validate |= promptContext.Context.Activity.Text.ToLowerInvariant().Contains(promptContext.Options.Choices[i].Value.ToUnpunctuated().ToLowerInvariant());

                if (validate)
                    promptContext.Recognized.Value = new FoundChoice()
                    {
                        Value = promptContext.Options.Choices[i - 1].Value,
                        Score = promptContext.Context.Activity.Text.Length > promptContext.Options.Choices[i - 1].Value.Length ? 0.8f : 1.0f,
                        Index = promptContext.Context.Activity.Text.ToLowerInvariant().IndexOf(promptContext.Options.Choices[i - 1].Value.ToUnpunctuated().ToLowerInvariant())
                    };
                promptContext.Recognized.Succeeded = validate;
            }
            catch
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(validate);
        }

        public static async Task<DialogTurnResult> RepeatDialogAsync(this DialogContext innerDc, int stepsBack, CancellationToken cancellationToken)
        {
            int stepIndex;

            var dialogInstance = innerDc.Stack.FirstOrDefault(d => d.State.Any(s => s.Key == "dialogs"))?.State["dialogs"] as DialogState;
            if (dialogInstance != null)
            {
                stepIndex = (dialogInstance.DialogStack.First(d => d.Id.Contains(nameof(WaterfallDialog)))?.State["stepIndex"] as int?) ?? 0;

                if (stepsBack >= 2 && stepIndex == 0)
                    return await innerDc.EndDialogAsync(1, cancellationToken);

                dialogInstance.DialogStack.First(d => d.Id.Contains(nameof(WaterfallDialog))).State["stepIndex"] = stepIndex - stepsBack;
                return await innerDc.ContinueDialogAsync(cancellationToken);
            }

            stepIndex = (innerDc.Stack.FirstOrDefault(d => d.Id.Contains(nameof(WaterfallDialog)))?.State["stepIndex"] as int?) ?? 0;

            if (stepsBack >= 2 && stepIndex == 0)
                return await innerDc.ReplaceDialogAsync(nameof(WaterfallDialog), null, cancellationToken);

            innerDc.Stack.First(d => d.Id.Contains(nameof(WaterfallDialog))).State["stepIndex"] = stepIndex - stepsBack;
            return await innerDc.ContinueDialogAsync(cancellationToken);
        }

        public static DateTime NextWorkingDay(this DateTime date)
        {
            if (date.DayOfWeek == DayOfWeek.Saturday)
                return date.AddDays(2);

            if (date.DayOfWeek == DayOfWeek.Sunday)
                return date.AddDays(1);

            return date;
        }
    }
}

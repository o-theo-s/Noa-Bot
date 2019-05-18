using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Noa.Helpers;

namespace Noa.Dialogs
{
    public class InterruptDialog : ComponentDialog
    {
        public InterruptDialog(string id) : base(id)
        {
            //AddDialog(new HelpDialog());
        }

        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default)
        {
            return await InterruptAsync(innerDc, cancellationToken) ?? await base.OnBeginDialogAsync(innerDc, options, cancellationToken);
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken)
        {
            if (innerDc.Context.Activity.Value != null)
            {
                var data = JsonConvert.DeserializeObject<CardMetaData>(innerDc.Context.Activity.Value.ToString());
                var dialogInstance = innerDc.Stack.FirstOrDefault(d => d.State.Any(s => s.Key == "dialogs"))?.State["dialogs"] as DialogState;
                if (dialogInstance != null)
                    dialogInstance.DialogStack.First(d => d.Id.Contains(nameof(WaterfallDialog))).State["options"] = data;
                else
                    innerDc.ActiveDialog.State["options"] = data;
            }

            return await InterruptAsync(innerDc, cancellationToken) ?? await base.OnContinueDialogAsync(innerDc, cancellationToken);
        }

        private async Task<DialogTurnResult> InterruptAsync(DialogContext innerDc, CancellationToken cancellationToken)
        {
            if (innerDc.Context.Activity.Type == ActivityTypes.Message && innerDc.Context.Activity.Value == null)
            {
                var text = innerDc.Context.Activity.Text.ToUnpunctuated().ToLowerInvariant();
                string[] helpSynonyms = { "βοηθεια", "help", "?" };
                string[] backSynonyms = { "πισω", "προηγουμενο", "back" };
                string[] cancelSynonyms = { "ακυρο", "ακυρωση", "αρχικη", "κυριως μενου", "αρχικο μενου", "επαναφορα", "cancel", "quit", "home", "main menu", "reset" };
                string[] hiSynonyms = { "hi", "hello", "γεια", "καλημερα", "καλησπερα", "good morning", "good evening" };

                if (!helpSynonyms.Any(s => text.Contains(s)))
                {
                    if (!backSynonyms.Any(s => text.Contains(s)))
                    {
                        if (!cancelSynonyms.Any(s => text.Contains(s)))
                        {
                            if (!hiSynonyms.Any(s => text.Contains(s)))
                            {
                                await innerDc.Context.SendActivityAsync(MessageFactory.Text("Παρακαλώ χρησιμοποίησε τα κουμπιά στις κάρτες."));
                                innerDc.Context.Activity.Text = "Πίσω";
                                return await InterruptAsync(innerDc, cancellationToken);
                            }
                            await innerDc.Context.SendActivityAsync(MessageFactory.Text("Γειααα! :)"), cancellationToken);
                            return await innerDc.RepeatDialogAsync(1, cancellationToken);
                        }
                        await innerDc.Context.SendActivityAsync(MessageFactory.Text("Επιστροφή στην αρχική..."), cancellationToken);
                        await innerDc.CancelAllDialogsAsync();
                        innerDc.Context.Activity.Text = string.Empty;
                        return await innerDc.BeginDialogAsync(nameof(WaterfallDialog), null, cancellationToken);
                    }
                    await innerDc.Context.SendActivityAsync(MessageFactory.Text("Πίσω..."), cancellationToken);
                    return await innerDc.RepeatDialogAsync(2, cancellationToken);
                }
                await innerDc.Context.SendActivityAsync(MessageFactory.Text("Πίσω..."), cancellationToken);
                return new DialogTurnResult(DialogTurnStatus.Waiting);
            }

            return null;
        }
    }

}

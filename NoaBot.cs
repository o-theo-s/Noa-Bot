// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Noa
{
    public class NoaBot<T> : ActivityHandler where T : Dialog
    {
        protected readonly Dialog Dialog;
        protected readonly BotState ConversationState;
        protected readonly BotState UserState;

        public NoaBot(ConversationState conversationState, UserState userState, T dialog)
        {
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            if (turnContext.Activity.Text == null)
                turnContext.Activity.Text = string.Empty;
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var usersAdded = membersAdded.Where(member => member.Id != turnContext.Activity.Recipient.Id).ToList();

            if (usersAdded.Count == 1)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text($"Γεια σου @{usersAdded.First().Name}! Είμαι ο Νόα, ο νέος σου εικονικός βοηθός για τα μαθήματα!"), cancellationToken);
                await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
            }
            else
            {
                var welcoming = MessageFactory.Text($"Καλωσήρθατε @{usersAdded[0].Name}");
                for (int i = 0; i < usersAdded.Count - 1; i++)
                    welcoming.Text += ", @" + usersAdded[i].Name;
                welcoming.Text += " και @" + usersAdded.Last().Name + "!";

                await turnContext.SendActivityAsync(welcoming, cancellationToken);
                await turnContext.SendActivityAsync(MessageFactory.Text("Είμαι ο Νόα, ο νέος σας εικονικός βοηθός για τα μαθήματα!"));
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Run the Dialog with the new message Activity.
            await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
        }
    }
}

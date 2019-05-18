using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace Noa.Dialogs
{
    internal static partial class ChoicesWrapper
    {
        public static List<Choice> Listify(Type wrapperType)
        {
            var fields = wrapperType.GetFields(BindingFlags.Public | BindingFlags.Static);
            if (fields == null)
                return null;
            var choiceFields = fields.Where(p => p.FieldType == typeof(Choice));

            List<Choice> choices = new List<Choice>(choiceFields.Count());
            foreach (var choiceField in choiceFields)
                choices.Add((Choice)choiceField.GetValue(null));

            return choices;
        }

        public static Choice GetChoiceByName(Type wrapperType, string choiceValue)
        {
            var field = wrapperType.GetField(choiceValue, BindingFlags.Public | BindingFlags.Static);
            if (field == null)
                return null;

            if (field.GetValue(null) is Choice)
                return (Choice)field.GetValue(null);
            else
                return null;
        }
    }
}

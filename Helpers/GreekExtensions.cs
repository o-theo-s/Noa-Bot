using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Noa.Helpers
{
    public static class GreekExtensions
    {
        public static string ToUnpunctuated(this string str)
        {
            string newStr = str;
            newStr = newStr.Replace('ά', 'α');
            newStr = newStr.Replace('έ', 'ε');
            newStr = newStr.Replace('ή', 'η');
            newStr = newStr.Replace('ί', 'ι');
            newStr = newStr.Replace('ύ', 'υ');
            newStr = newStr.Replace('ό', 'ο');
            newStr = newStr.Replace('ώ', 'ω');

            return newStr;
        }

        //TODO
        public static string ToGreeklish(this string str)
        {
            return null;
        }

        public static string DayGreekName(this DateTime date)
        {
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Friday:
                    return "Παρασκευή";
                case DayOfWeek.Monday:
                    return "Δευτέρα";
                case DayOfWeek.Saturday:
                    return "Σάββατο";
                case DayOfWeek.Sunday:
                    return "Κυριακή";
                case DayOfWeek.Thursday:
                    return "Πέμπτη";
                case DayOfWeek.Tuesday:
                    return "Τρίτη";
                case DayOfWeek.Wednesday:
                    return "Τετάρτη";
                default:
                    return null;
            }
        }

        public static string MonthGreekName(this DateTime date)
        {
            switch (date.Month)
            {
                case 1:
                    return "Ιανουαρίου";
                case 2:
                    return "Φεβρουαρίου";
                case 3:
                    return "Μαρτίου";
                case 4:
                    return "Απριλίου";
                case 5:
                    return "Μαΐου";
                case 6:
                    return "Ιουνίου";
                case 7:
                    return "Ιουλίου";
                case 8:
                    return "Αυγούστου";
                case 9:
                    return "Σεπτεμβρίου";
                case 10:
                    return "Οκτωβρίου";
                case 11:
                    return "Νοεμβρίου";
                case 12:
                    return "Δεκεμβρίου";
                default:
                    return null;
            }

        }

        public static string ToGreekDate(this DateTime date)
        {
            return $"{date.DayGreekName()} {date.Day} {date.MonthGreekName()} {date.Year}";
        }

        public static string ToGreekWeekDate(this DateTime date)
        {
            DateTime workingDate = date.NextWorkingDay();
            DateTime monday = workingDate.AddDays(-(double)workingDate.DayOfWeek + 1.0);
            return $"{monday.Day} - {monday.Day + 4} {workingDate.MonthGreekName()} {workingDate.Year}";
        }
    }
}

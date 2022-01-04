using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.FamilyShow
{
    internal static class StringExtensions
    {
        /// <summary>
        ///     Converts string to date time object using DateTime.TryParse.
        ///     Also accepts just the year for dates. 1977 = 1/1/1977.
        /// </summary>
        public static DateTime FriendlyStringToDate(this string? obj)
        {
            if (obj == null)
                return DateTime.MinValue;

            // Append first month and day if just the year was entered.
            if (obj.Length == 4) obj = "1/1/" + obj;

            _ = DateTime.TryParse(obj, out var date);

            return date;
        }
    }
}

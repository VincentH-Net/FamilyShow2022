using System;
using System.Collections;
using System.Globalization;
using Microsoft.FamilyShowLib;

namespace Microsoft.FamilyShow;

/// <summary>
/// Compare the birthdates (month and day) and first names of two people
/// </summary>
public class MonthDayComparer : IComparer
{
    public int Compare(object x, object y)
    {
        Person p1 = x as Person;
        Person p2 = y as Person;

        if (p1 == p2)
            return 0;

        // Check the month first
        if (p1.BirthDate.Value.Month < p2.BirthDate.Value.Month)
            return -1;
        else if (p1.BirthDate.Value.Month > p2.BirthDate.Value.Month)
            return 1;
        else
        {
            // Since the months were the same, now check the day
            if (p1.BirthDate.Value.Day < p2.BirthDate.Value.Day)
                return -1;
            else if (p1.BirthDate.Value.Day > p2.BirthDate.Value.Day)
                return 1;
            else
            {
                // The days are the same so check the first name
                return (String.Compare(p1.FirstName, p2.FirstName, true, CultureInfo.CurrentCulture));
            }
        }
    }
}
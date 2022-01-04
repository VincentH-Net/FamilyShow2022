using System;
using System.Globalization;

namespace Microsoft.FamilyShow.Controls.FamilyData;

/// <summary>
/// Class that parses the filter text.
/// </summary>
public class Filter
{
    // Parsed data from the filter string.
    private string filterText;
    private int? maximumAge;
    private int? minimumAge;
    private DateTime? filterDate;

    /// <summary>
    /// Indicates if the filter is empty.
    /// </summary>
    public bool IsEmpty
    {
        get { return string.IsNullOrEmpty(filterText); }
    }

    /// <summary>
    /// Return true if the filter contains the specified text.
    /// </summary>
    public bool Matches(string text)
    {
        return (filterText != null && text != null && text.ToLower(CultureInfo.CurrentCulture).Contains(filterText));
    }

    /// <summary>
    /// Return true if the filter contains the specified date.
    /// </summary>
    public bool Matches(DateTime? date)
    {
        return (date != null && date.Value.ToShortDateString().Contains(filterText));
    }

    /// <summary>
    /// Return true if the filter contains the year in the specified date.
    /// </summary>
    public bool MatchesYear(DateTime? date)
    {
        return (date != null && date.Value.Year.ToString(CultureInfo.CurrentCulture).Contains(filterText));
    }

    /// <summary>
    /// Return true if the filter contains the month in the specified date.
    /// </summary>
    public bool MatchesMonth(DateTime? date)
    {
        return (date != null && filterDate != null &&
                date.Value.Month == filterDate.Value.Month);
    }

    /// <summary>
    /// Return true if the filter contains the day in the specified date.
    /// </summary>
    public bool MatchesDay(DateTime? date)
    {
        return (date != null && filterDate != null && date.Value.Day == filterDate.Value.Day);
    }

    /// <summary>
    /// Return true if the filter contains the specified age. The filter can 
    /// represent a single age (10), a range (10-20), or an ending (10+).
    /// </summary>
    public bool Matches(int? age)
    {
        if (age == null)
        {
            return false;
        }

        // Check single age.
        if (minimumAge != null && age.Value == minimumAge.Value)
        {
            return true;
        }

        // Check for a range.
        if (minimumAge != null && maximumAge != null && age.Value >= minimumAge && age <= maximumAge)
        {
            return true;
        }

        // Check for an ending age.
        if (minimumAge == null && maximumAge != null && age.Value >= maximumAge)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Parse the specified filter text.
    /// </summary>
    public void Parse(string text)
    {
        // Initialize fields.
        filterText = string.Empty;
        filterDate = null;
        minimumAge = null;
        maximumAge = null;

        // Store the filter text.
        filterText = string.IsNullOrEmpty(text) ? "" : text.ToLower(CultureInfo.CurrentCulture).Trim();

        // Parse date and age.
        ParseDate();
        ParseAge();
    }

    /// <summary>
    /// Parse the filter date.
    /// </summary>
    private void ParseDate()
    {
        DateTime date;
        if (DateTime.TryParse(filterText, out date))
        {
            filterDate = date;
        }
    }

    /// <summary>
    /// Parse the filter age. The filter can represent a
    /// single age (10), a range (10-20), or an ending (10+).
    /// </summary>
    private void ParseAge()
    {
        int age;

        // Single age.
        if (Int32.TryParse(filterText, out age))
        {
            minimumAge = age;
        }

        // Age range.
        if (filterText.Contains("-"))
        {
            string[] list = filterText.Split('-');

            if (Int32.TryParse(list[0], out age))
            {
                minimumAge = age;
            }

            if (Int32.TryParse(list[1], out age))
            {
                maximumAge = age;
            }
        }

        // Ending age.
        if (filterText.EndsWith("+"))
        {
            if (int.TryParse(filterText.Substring(0, filterText.Length - 1), out age))
            {
                maximumAge = age;
            }
        }
    }
}
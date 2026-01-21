using System;

namespace Rh.MessageFormat.Abstractions.Models;

/// <summary>
/// Contains week configuration data for a locale/region.
/// Used for locale-aware week-of-year calculations.
/// </summary>
public readonly struct WeekData
{
    /// <summary>
    /// The first day of the week for this region.
    /// </summary>
    public readonly DayOfWeek FirstDay;

    /// <summary>
    /// The minimum number of days in the first week of the year.
    /// For ISO 8601, this is 4. For US, this is 1.
    /// </summary>
    public readonly int MinDays;

    /// <summary>
    /// Creates a new WeekData instance.
    /// </summary>
    /// <param name="firstDay">The first day of the week.</param>
    /// <param name="minDays">The minimum days in the first week (1-7).</param>
    public WeekData(DayOfWeek firstDay, int minDays)
    {
        FirstDay = firstDay;
        MinDays = Math.Clamp(minDays, 1, 7);
    }

    /// <summary>
    /// Indicates whether this data has been explicitly set.
    /// </summary>
    public bool HasData => MinDays > 0;

    /// <summary>
    /// Creates a WeekData instance for ISO 8601 standard (first day Monday, minDays 4).
    /// </summary>
    public static WeekData Iso8601 => new(DayOfWeek.Monday, 4);

    /// <summary>
    /// Creates a WeekData instance for US standard (first day Sunday, minDays 1).
    /// </summary>
    public static WeekData US => new(DayOfWeek.Sunday, 1);
}

using DaylioData.Models;

namespace DaylioData;

/// <summary>
/// Provides advanced analytics and insights for Daylio tracking datasets.
/// </summary>
public static class DaylioAnalytics
{
    /// <summary>
    /// Gets the count of entries for each distinct mood.
    /// </summary>
    /// <param name="daylioData">The <see cref="DaylioData"/> instance.</param>
    /// <returns>A dictionary mapping mood names to occurrence counts.</returns>
    public static IReadOnlyDictionary<string, int> GetMoodDistribution(this DaylioData daylioData)
    {
        Dictionary<string, int> distribution = new(StringComparer.OrdinalIgnoreCase);
        IEnumerable<DaylioCSVDataModel>? entries = daylioData.DataRepo?.CSVData;
        if (entries is null)
        {
            return distribution;
        }

        foreach (DaylioCSVDataModel entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Mood))
            {
                continue;
            }

            if (distribution.TryGetValue(entry.Mood, out int currentCount))
            {
                distribution[entry.Mood] = currentCount + 1;
            }
            else
            {
                distribution[entry.Mood] = 1;
            }
        }

        return distribution;
    }

    /// <summary>
    /// Gets the most frequently logged activities up to the specified limit.
    /// </summary>
    /// <param name="daylioData">The <see cref="DaylioData"/> instance.</param>
    /// <param name="count">The maximum number of activities to return.</param>
    /// <returns>A list of activity name and frequency pairs, ordered from most to least frequent.</returns>
    public static IReadOnlyList<KeyValuePair<string, int>> GetTopActivities(this DaylioData daylioData, int count = 10)
    {
        Dictionary<string, int> activityCounts = new(StringComparer.OrdinalIgnoreCase);
        IEnumerable<DaylioCSVDataModel>? entries = daylioData.DataRepo?.CSVData;
        if (entries is null || count <= 0)
        {
            return Array.Empty<KeyValuePair<string, int>>();
        }

        foreach (DaylioCSVDataModel entry in entries)
        {
            foreach (string activity in entry.ActivitiesCollection)
            {
                if (activityCounts.TryGetValue(activity, out int currentCount))
                {
                    activityCounts[activity] = currentCount + 1;
                }
                else
                {
                    activityCounts[activity] = 1;
                }
            }
        }

        return activityCounts
            .OrderByDescending(x => x.Value)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Calculates the longest consecutive daily tracking streak in the dataset.
    /// </summary>
    /// <param name="daylioData">The <see cref="DaylioData"/> instance.</param>
    /// <returns>The maximum consecutive days with logged entries.</returns>
    public static int GetLongestStreak(this DaylioData daylioData)
    {
        IEnumerable<DaylioCSVDataModel>? entries = daylioData.DataRepo?.CSVData;
        if (entries is null)
        {
            return 0;
        }

        List<DateOnly> distinctDates = entries
            .Select(x => x.FullDate)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        if (distinctDates.Count == 0)
        {
            return 0;
        }

        int maxStreak = 1;
        int currentStreak = 1;

        for (int i = 1; i < distinctDates.Count; i++)
        {
            if (distinctDates[i].DayNumber == distinctDates[i - 1].DayNumber + 1)
            {
                currentStreak++;
                if (currentStreak > maxStreak)
                {
                    maxStreak = currentStreak;
                }
            }
            else
            {
                currentStreak = 1;
            }
        }

        return maxStreak;
    }

    /// <summary>
    /// Calculates the average mood rating for each day of the week.
    /// </summary>
    /// <param name="daylioData">The <see cref="DaylioData"/> instance.</param>
    /// <returns>A dictionary mapping each day of the week to its average mood rating.</returns>
    public static IReadOnlyDictionary<DayOfWeek, decimal> GetAverageMoodByDayOfWeek(this DaylioData daylioData)
    {
        Dictionary<DayOfWeek, (uint Sum, uint Count)> dayMoods = new();
        Dictionary<DayOfWeek, decimal> results = new();

        if (daylioData.DataRepo?.CSVData is null)
        {
            return results;
        }

        foreach (DaylioCSVDataModel entry in daylioData.DataRepo.CSVData)
        {
            if (daylioData.DataRepo.Moods.TryGetValue(entry.Mood, out short? level) && level.HasValue)
            {
                DayOfWeek day = entry.FullDate.DayOfWeek;
                if (dayMoods.TryGetValue(day, out (uint Sum, uint Count) current))
                {
                    dayMoods[day] = (current.Sum + Convert.ToUInt32(level.Value), current.Count + 1);
                }
                else
                {
                    dayMoods[day] = (Convert.ToUInt32(level.Value), 1);
                }
            }
        }

        foreach (KeyValuePair<DayOfWeek, (uint Sum, uint Count)> pair in dayMoods)
        {
            if (pair.Value.Count > 0)
            {
                results[pair.Key] = (decimal)pair.Value.Sum / pair.Value.Count;
            }
        }

        return results;
    }
}

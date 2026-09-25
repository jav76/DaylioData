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
        IReadOnlyList<DaylioCSVDataModel> entries = daylioData.DataRepo.CSVData;
        if (entries.Count == 0)
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
        IReadOnlyList<DaylioCSVDataModel> entries = daylioData.DataRepo.CSVData;
        if (entries.Count == 0 || count <= 0)
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
        IReadOnlyList<DaylioCSVDataModel> entries = daylioData.DataRepo.CSVData;
        if (entries.Count == 0)
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

        if (daylioData.DataRepo.CSVData.Count == 0)
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

    /// <summary>
    /// Calculates the impact of a specific activity on mood ratings compared to entries without it.
    /// </summary>
    /// <param name="daylioData">The <see cref="DaylioData"/> instance.</param>
    /// <param name="activity">The activity to analyze.</param>
    /// <returns>An <see cref="ActivityMoodImpact"/> or null if the activity has no valid ratings.</returns>
    public static ActivityMoodImpact? GetActivityMoodImpact(this DaylioData daylioData, string activity)
    {
        if (daylioData.DataRepo.CSVData.Count == 0 || string.IsNullOrWhiteSpace(activity))
        {
            return null;
        }

        EnsureDefaultMoodLevels(daylioData);

        uint sumWith = 0;
        int countWith = 0;
        uint sumWithout = 0;
        int countWithout = 0;

        foreach (DaylioCSVDataModel entry in daylioData.DataRepo.CSVData)
        {
            if (!daylioData.DataRepo.Moods.TryGetValue(entry.Mood, out short? moodLevel) || !moodLevel.HasValue)
            {
                continue;
            }

            bool containsActivity = entry.ActivitiesCollection.Any(a =>
                a.Equals(activity, StringComparison.OrdinalIgnoreCase));

            if (containsActivity)
            {
                sumWith += Convert.ToUInt32(moodLevel.Value);
                countWith++;
            }
            else
            {
                sumWithout += Convert.ToUInt32(moodLevel.Value);
                countWithout++;
            }
        }

        if (countWith == 0)
        {
            return null;
        }

        decimal avgWith = (decimal)sumWith / countWith;
        decimal avgWithout = countWithout > 0 ? (decimal)sumWithout / countWithout : avgWith;
        decimal delta = avgWith - avgWithout;

        return new ActivityMoodImpact(activity, avgWith, avgWithout, delta, countWith, countWithout);
    }

    /// <summary>
    /// Calculates mood impact for all tracked activities and ranks them by net mood delta.
    /// </summary>
    /// <param name="daylioData">The <see cref="DaylioData"/> instance.</param>
    /// <param name="minOccurrences">Minimum times an activity must be logged to be included.</param>
    /// <returns>A list of <see cref="ActivityMoodImpact"/> ordered from highest positive to lowest negative delta.</returns>
    public static IReadOnlyList<ActivityMoodImpact> GetAllActivityMoodImpacts(
        this DaylioData daylioData,
        int minOccurrences = 1)
    {
        List<ActivityMoodImpact> impacts = new();

        foreach (string activity in daylioData.DataRepo.Activities)
        {
            ActivityMoodImpact? impact = daylioData.GetActivityMoodImpact(activity);
            if (impact is not null && impact.FrequencyWith >= minOccurrences)
            {
                impacts.Add(impact);
            }
        }

        return impacts.OrderByDescending(x => x.Delta).ToList();
    }

    /// <summary>
    /// Gets the most frequently co-occurring pairs of activities.
    /// </summary>
    /// <param name="daylioData">The <see cref="DaylioData"/> instance.</param>
    /// <param name="count">The maximum number of pairs to return.</param>
    /// <returns>A list of <see cref="ActivityPairOccurrence"/> ordered by frequency.</returns>
    public static IReadOnlyList<ActivityPairOccurrence> GetTopActivityPairs(
        this DaylioData daylioData,
        int count = 10)
    {
        Dictionary<(string First, string Second), int> pairCounts = new();
        IReadOnlyList<DaylioCSVDataModel> entries = daylioData.DataRepo.CSVData;
        if (entries.Count == 0 || count <= 0)
        {
            return Array.Empty<ActivityPairOccurrence>();
        }

        foreach (DaylioCSVDataModel entry in entries)
        {
            IReadOnlyList<string> activities = entry.ActivitiesCollection;
            if (activities.Count < 2)
            {
                continue;
            }

            for (int i = 0; i < activities.Count; i++)
            {
                for (int j = i + 1; j < activities.Count; j++)
                {
                    string act1 = activities[i];
                    string act2 = activities[j];
                    if (act1.Equals(act2, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string first = string.Compare(act1, act2, StringComparison.OrdinalIgnoreCase) <= 0 ? act1 : act2;
                    string second = string.Compare(act1, act2, StringComparison.OrdinalIgnoreCase) <= 0 ? act2 : act1;
                    (string First, string Second) key = (first, second);

                    if (pairCounts.TryGetValue(key, out int current))
                    {
                        pairCounts[key] = current + 1;
                    }
                    else
                    {
                        pairCounts[key] = 1;
                    }
                }
            }
        }

        return pairCounts
            .OrderByDescending(x => x.Value)
            .Take(count)
            .Select(x => new ActivityPairOccurrence(x.Key.First, x.Key.Second, x.Value))
            .ToList();
    }

    /// <summary>
    /// Calculates the mood impact and synergy when two activities are logged together compared to when neither is logged.
    /// </summary>
    /// <param name="daylioData">The <see cref="DaylioData"/> instance.</param>
    /// <param name="activity1">The first activity name.</param>
    /// <param name="activity2">The second activity name.</param>
    /// <returns>An <see cref="ActivityPairImpact"/> or null if the pair has no valid co-occurrences.</returns>
    public static ActivityPairImpact? GetActivityPairImpact(
        this DaylioData daylioData,
        string activity1,
        string activity2)
    {
        if (daylioData.DataRepo.CSVData.Count == 0 ||
            string.IsNullOrWhiteSpace(activity1) ||
            string.IsNullOrWhiteSpace(activity2) ||
            activity1.Equals(activity2, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        EnsureDefaultMoodLevels(daylioData);

        uint sumWithBoth = 0;
        int countWithBoth = 0;
        uint sumWithoutEither = 0;
        int countWithoutEither = 0;

        foreach (DaylioCSVDataModel entry in daylioData.DataRepo.CSVData)
        {
            if (!daylioData.DataRepo.Moods.TryGetValue(entry.Mood, out short? moodLevel) || !moodLevel.HasValue)
            {
                continue;
            }

            bool hasAct1 = entry.ActivitiesCollection.Any(a => a.Equals(activity1, StringComparison.OrdinalIgnoreCase));
            bool hasAct2 = entry.ActivitiesCollection.Any(a => a.Equals(activity2, StringComparison.OrdinalIgnoreCase));

            if (hasAct1 && hasAct2)
            {
                sumWithBoth += Convert.ToUInt32(moodLevel.Value);
                countWithBoth++;
            }
            else if (!hasAct1 && !hasAct2)
            {
                sumWithoutEither += Convert.ToUInt32(moodLevel.Value);
                countWithoutEither++;
            }
        }

        if (countWithBoth == 0)
        {
            return null;
        }

        decimal avgWithBoth = (decimal)sumWithBoth / countWithBoth;
        decimal avgWithoutEither = countWithoutEither > 0
            ? (decimal)sumWithoutEither / countWithoutEither
            : avgWithBoth;
        decimal delta = avgWithBoth - avgWithoutEither;

        string first = string.Compare(activity1, activity2, StringComparison.OrdinalIgnoreCase) <= 0 ? activity1 : activity2;
        string second = string.Compare(activity1, activity2, StringComparison.OrdinalIgnoreCase) <= 0 ? activity2 : activity1;

        return new ActivityPairImpact(first, second, countWithBoth, avgWithBoth, avgWithoutEither, delta);
    }

    /// <summary>
    /// Calculates mood impacts and synergies for all co-occurring activity pairs meeting a minimum occurrence threshold.
    /// </summary>
    /// <param name="daylioData">The <see cref="DaylioData"/> instance.</param>
    /// <param name="minOccurrences">The minimum number of times a pair must co-occur.</param>
    /// <returns>A list of <see cref="ActivityPairImpact"/> ordered from highest to lowest synergy delta.</returns>
    public static IReadOnlyList<ActivityPairImpact> GetAllActivityPairImpacts(
        this DaylioData daylioData,
        int minOccurrences = 2)
    {
        List<ActivityPairImpact> impacts = new();
        IReadOnlyList<ActivityPairOccurrence> topPairs = daylioData.GetTopActivityPairs(int.MaxValue);

        foreach (ActivityPairOccurrence pair in topPairs)
        {
            if (pair.Count < minOccurrences)
            {
                break;
            }

            ActivityPairImpact? impact = daylioData.GetActivityPairImpact(pair.Activity1, pair.Activity2);
            if (impact is not null)
            {
                impacts.Add(impact);
            }
        }

        return impacts.OrderByDescending(x => x.Delta).ToList();
    }

    /// <summary>
    /// Calculates average mood ratings and entry counts grouped by time-of-day periods.
    /// Morning (05:00-11:59), Afternoon (12:00-16:59), Evening (17:00-21:59), Night (22:00-04:59).
    /// </summary>
    /// <param name="daylioData">The <see cref="DaylioData"/> instance.</param>
    /// <returns>A dictionary mapping each period to its <see cref="TimeOfDayMood"/> metrics.</returns>
    public static IReadOnlyDictionary<TimeOfDayPeriod, TimeOfDayMood> GetMoodByTimeOfDay(this DaylioData daylioData)
    {
        Dictionary<TimeOfDayPeriod, TimeOfDayMood> results = new();
        if (daylioData.DataRepo.CSVData.Count == 0)
        {
            return results;
        }

        EnsureDefaultMoodLevels(daylioData);

        Dictionary<TimeOfDayPeriod, (uint Sum, int Count)> aggregations = new()
        {
            { TimeOfDayPeriod.Morning, (0, 0) },
            { TimeOfDayPeriod.Afternoon, (0, 0) },
            { TimeOfDayPeriod.Evening, (0, 0) },
            { TimeOfDayPeriod.Night, (0, 0) }
        };

        foreach (DaylioCSVDataModel entry in daylioData.DataRepo.CSVData)
        {
            if (!daylioData.DataRepo.Moods.TryGetValue(entry.Mood, out short? moodLevel) || !moodLevel.HasValue)
            {
                continue;
            }

            TimeOfDayPeriod period = entry.Time.Hour switch
            {
                >= 5 and < 12 => TimeOfDayPeriod.Morning,
                >= 12 and < 17 => TimeOfDayPeriod.Afternoon,
                >= 17 and < 22 => TimeOfDayPeriod.Evening,
                _ => TimeOfDayPeriod.Night
            };

            (uint Sum, int Count) current = aggregations[period];
            aggregations[period] = (current.Sum + Convert.ToUInt32(moodLevel.Value), current.Count + 1);
        }

        foreach (KeyValuePair<TimeOfDayPeriod, (uint Sum, int Count)> pair in aggregations)
        {
            if (pair.Value.Count > 0)
            {
                decimal avg = (decimal)pair.Value.Sum / pair.Value.Count;
                results[pair.Key] = new TimeOfDayMood(pair.Key, avg, pair.Value.Count);
            }
        }

        return results;
    }

    /// <summary>
    /// Calculates comprehensive tracking streak information including longest and current streaks.
    /// </summary>
    /// <param name="daylioData">The <see cref="DaylioData"/> instance.</param>
    /// <returns>A <see cref="StreakDetails"/> record.</returns>
    public static StreakDetails GetStreakDetails(this DaylioData daylioData)
    {
        int longest = daylioData.GetLongestStreak();
        IReadOnlyList<DaylioCSVDataModel> entries = daylioData.DataRepo.CSVData;
        if (entries.Count == 0)
        {
            return new StreakDetails(0, 0, null, null);
        }

        List<DateOnly> distinctDates = entries
            .Select(x => x.FullDate)
            .Distinct()
            .OrderByDescending(x => x)
            .ToList();

        if (distinctDates.Count == 0)
        {
            return new StreakDetails(0, 0, null, null);
        }

        int currentStreak = 1;
        DateOnly streakEnd = distinctDates[0];
        DateOnly streakStart = streakEnd;

        for (int i = 1; i < distinctDates.Count; i++)
        {
            if (distinctDates[i].DayNumber == distinctDates[i - 1].DayNumber - 1)
            {
                currentStreak++;
                streakStart = distinctDates[i];
            }
            else
            {
                break;
            }
        }

        return new StreakDetails(longest, currentStreak, streakStart, streakEnd);
    }

    /// <summary>
    /// Calculates smoothed daily and rolling average mood ratings across a moving calendar day window.
    /// Normalizes multiple check-ins per day into a daily mean before calculating the rolling window.
    /// </summary>
    /// <param name="daylioData">The <see cref="DaylioData"/> instance.</param>
    /// <param name="windowDays">The number of calendar days in the moving window (default: 7).</param>
    /// <returns>A list of <see cref="DailyRollingMood"/> records in chronological order.</returns>
    public static IReadOnlyList<DailyRollingMood> GetRollingMoodTrends(
        this DaylioData daylioData,
        int windowDays = 7)
    {
        List<DailyRollingMood> results = new();
        if (daylioData.DataRepo.CSVData.Count == 0 || windowDays <= 0)
        {
            return results;
        }

        EnsureDefaultMoodLevels(daylioData);

        Dictionary<DateOnly, (uint Sum, int Count)> dailyAggregates = new();
        foreach (DaylioCSVDataModel entry in daylioData.DataRepo.CSVData)
        {
            if (!daylioData.DataRepo.Moods.TryGetValue(entry.Mood, out short? moodLevel) || !moodLevel.HasValue)
            {
                continue;
            }

            DateOnly date = entry.FullDate;
            if (dailyAggregates.TryGetValue(date, out (uint Sum, int Count) current))
            {
                dailyAggregates[date] = (current.Sum + Convert.ToUInt32(moodLevel.Value), current.Count + 1);
            }
            else
            {
                dailyAggregates[date] = (Convert.ToUInt32(moodLevel.Value), 1);
            }
        }

        if (dailyAggregates.Count == 0)
        {
            return results;
        }

        List<(DateOnly Date, decimal DailyAverage, int EntryCount)> sortedDaily = dailyAggregates
            .OrderBy(x => x.Key)
            .Select(x => (x.Key, (decimal)x.Value.Sum / x.Value.Count, x.Value.Count))
            .ToList();

        for (int i = 0; i < sortedDaily.Count; i++)
        {
            (DateOnly Date, decimal DailyAverage, int EntryCount) currentDay = sortedDaily[i];
            int minDayNumber = currentDay.Date.DayNumber - (windowDays - 1);

            List<(DateOnly Date, decimal DailyAverage, int EntryCount)> window = sortedDaily
                .Where(d => d.Date.DayNumber >= minDayNumber && d.Date.DayNumber <= currentDay.Date.DayNumber)
                .ToList();

            decimal rollingAverage = window.Sum(w => w.DailyAverage) / window.Count;
            results.Add(new DailyRollingMood(
                currentDay.Date,
                currentDay.DailyAverage,
                rollingAverage,
                currentDay.EntryCount));
        }

        return results;
    }

    private static void EnsureDefaultMoodLevels(DaylioData daylioData)
    {
        if (daylioData.DataRepo is not null && !daylioData.DataRepo.Moods.Values.Any(v => v.HasValue))
        {
            daylioData.DataRepo.SetDefaultMoodLevels();
        }
    }
}

/// <summary>
/// Represents the statistical impact of an activity on mood ratings.
/// </summary>
/// <param name="Activity">The activity name.</param>
/// <param name="AverageMoodWith">Average mood score when the activity is logged.</param>
/// <param name="AverageMoodWithout">Average mood score when the activity is not logged.</param>
/// <param name="Delta">The difference (AverageMoodWith - AverageMoodWithout).</param>
/// <param name="FrequencyWith">Number of entries including the activity.</param>
/// <param name="FrequencyWithout">Number of entries excluding the activity.</param>
public record ActivityMoodImpact(
    string Activity,
    decimal AverageMoodWith,
    decimal AverageMoodWithout,
    decimal Delta,
    int FrequencyWith,
    int FrequencyWithout);

/// <summary>
/// Represents the frequency with which two activities are logged together.
/// </summary>
/// <param name="Activity1">First activity name (alphabetically normalized).</param>
/// <param name="Activity2">Second activity name (alphabetically normalized).</param>
/// <param name="Count">Number of entries where both activities were logged.</param>
public record ActivityPairOccurrence(
    string Activity1,
    string Activity2,
    int Count);

/// <summary>
/// Represents the statistical mood synergy and impact of two activities logged together.
/// </summary>
/// <param name="Activity1">First activity name (alphabetically normalized).</param>
/// <param name="Activity2">Second activity name (alphabetically normalized).</param>
/// <param name="CoOccurrenceCount">Number of entries including both activities.</param>
/// <param name="AverageMoodWithBoth">Average mood rating when both activities are present.</param>
/// <param name="AverageMoodWithoutEither">Average mood rating when neither activity is present.</param>
/// <param name="Delta">The difference (AverageMoodWithBoth - AverageMoodWithoutEither).</param>
public record ActivityPairImpact(
    string Activity1,
    string Activity2,
    int CoOccurrenceCount,
    decimal AverageMoodWithBoth,
    decimal AverageMoodWithoutEither,
    decimal Delta);

/// <summary>
/// Represents the four canonical periods of a day.
/// </summary>
public enum TimeOfDayPeriod
{
    Morning,
    Afternoon,
    Evening,
    Night
}

/// <summary>
/// Represents mood metrics for a specific period of the day.
/// </summary>
/// <param name="Period">The time-of-day period.</param>
/// <param name="AverageMood">Average mood rating during this period.</param>
/// <param name="EntryCount">Total number of entries in this period.</param>
public record TimeOfDayMood(
    TimeOfDayPeriod Period,
    decimal AverageMood,
    int EntryCount);

/// <summary>
/// Represents detailed tracking streak information.
/// </summary>
/// <param name="LongestStreak">The longest consecutive daily streak in days.</param>
/// <param name="CurrentStreak">The most recent active consecutive daily streak in days.</param>
/// <param name="StreakStartDate">Start date of the current streak.</param>
/// <param name="StreakEndDate">End date of the current streak.</param>
public record StreakDetails(
    int LongestStreak,
    int CurrentStreak,
    DateOnly? StreakStartDate,
    DateOnly? StreakEndDate);

/// <summary>
/// Represents normalized daily mood and its moving calendar rolling average.
/// </summary>
/// <param name="Date">The calendar date of the entry.</param>
/// <param name="DailyAverageMood">The average mood rating for this specific day.</param>
/// <param name="RollingAverageMood">The rolling average mood rating computed across the active window.</param>
/// <param name="EntryCount">The number of journal entries logged on this day.</param>
public record DailyRollingMood(
    DateOnly Date,
    decimal DailyAverageMood,
    decimal RollingAverageMood,
    int EntryCount);


using System.Text.Json;
using DaylioData.Mcp.Protocol;

namespace DaylioData.Mcp.Handlers;

/// <summary>
/// Handles MCP prompt templates for guided wellness and habit reflections.
/// </summary>
public static class PromptHandler
{
    /// <summary>
    /// Gets definitions for all prompt templates exposed by the MCP server.
    /// </summary>
    /// <returns>A list of <see cref="McpPrompt"/> definitions.</returns>
    public static List<McpPrompt> GetPromptDefinitions()
    {
        return new List<McpPrompt>
        {
            new(
                "weekly_reflection",
                "Guides a weekly mood and habit review, analyzing recent patterns and celebrating streaks.",
                new List<McpPromptArgument>
                {
                    new(
                        "focus_topic",
                        "Optional topic or activity to focus on (e.g. sleep, work, exercise).",
                        false)
                }),
            new(
                "habit_correlation_review",
                "Deep-dive analysis on how specific activities impact your mood and well-being.",
                new List<McpPromptArgument>
                {
                    new(
                        "activity",
                        "Specific activity to analyze (e.g. gym, coding, meditation).",
                        false)
                })
        };
    }

    /// <summary>
    /// Generates a filled prompt template by name.
    /// </summary>
    /// <param name="name">The prompt name.</param>
    /// <param name="args">Optional arguments for the prompt.</param>
    /// <param name="getCurrentData">Function to retrieve the active DaylioData instance.</param>
    /// <returns>A <see cref="PromptGetResult"/>.</returns>
    public static PromptGetResult GetPrompt(
        string name,
        JsonElement? args,
        Func<DaylioData?> getCurrentData)
    {
        string? focusTopic = null;
        if (args is not null && args.Value.TryGetProperty("focus_topic", out JsonElement topicElem))
        {
            focusTopic = topicElem.GetString();
        }

        string? activity = null;
        if (args is not null && args.Value.TryGetProperty("activity", out JsonElement actElem))
        {
            activity = actElem.GetString();
        }

        return name.ToLowerInvariant() switch
        {
            "weekly_reflection" => BuildWeeklyReflectionPrompt(focusTopic),
            "habit_correlation_review" => BuildHabitCorrelationPrompt(activity),
            _ => new PromptGetResult(
                "Unknown Prompt",
                new List<PromptMessage>
                {
                    new(
                        "user",
                        new PromptMessageContent(
                            "text",
                            $"Unknown prompt template: '{name}'"))
                })
        };
    }

    private static PromptGetResult BuildWeeklyReflectionPrompt(string? focusTopic)
    {
        string topicClause = string.IsNullOrWhiteSpace(focusTopic)
            ? string.Empty
            : $" Pay special attention to anything related to '{focusTopic}'.";

        string promptText =
            "You are acting as an empathetic, data-informed personal wellness and habit coach.\n\n" +
            "Please perform a weekly reflection on my Daylio mood tracking data:\n" +
            "1. Use 'query_entries' to examine my entries from the past 7 days.\n" +
            "2. Check 'get_streaks' to see my current tracking streak.\n" +
            "3. Identify my most frequent activities and mood distribution for the week.\n" +
            "4. Highlight any positive moments, accomplishments, or potential stressors mentioned in my notes.\n" +
            "5. Provide 2-3 constructive and gentle observations or suggestions for the upcoming week." +
            topicClause;

        return new PromptGetResult(
            "Weekly Daylio Reflection & Wellness Review",
            new List<PromptMessage>
            {
                new("user", new PromptMessageContent("text", promptText))
            });
    }

    private static PromptGetResult BuildHabitCorrelationPrompt(string? activity)
    {
        string activityTarget = string.IsNullOrWhiteSpace(activity)
            ? "all top activities"
            : $"the activity '{activity}'";

        string promptText =
            "You are acting as a data science and behavioral habit coach.\n\n" +
            $"Please conduct a deep-dive analysis on how {activityTarget} correlates with my mood:\n" +
            "1. Use 'get_activity_mood_impact' to calculate the mood delta and sample frequency.\n" +
            "2. Use 'get_time_of_day_trends' to check when entries are logged.\n" +
            "3. Use 'query_entries' to read the journal notes on days when this activity was logged.\n" +
            "4. Synthesize the findings into clear conclusions: Is this habit energizing or draining? Under what circumstances does it lead to better days?";

        return new PromptGetResult(
            "Habit Mood Correlation Deep Dive",
            new List<PromptMessage>
            {
                new("user", new PromptMessageContent("text", promptText))
            });
    }
}

using System.Text.Json;
using DaylioData.Mcp;

namespace DaylioData.Tests;

public class DaylioMcpTests
{
    private const string SAMPLE_CSV =
        "full_date,date,weekday,time,mood,activities,note_title,note\n" +
        "2023-01-01,2023-01-01,Sunday,09:00,good,running | meditation,Morning,Good run today\n" +
        "2023-01-02,2023-01-02,Monday,10:00,good,running | reading,Quiet day,Read a chapter\n" +
        "2023-01-03,2023-01-03,Tuesday,11:00,rad,running | coding,Productive,Shipped new code\n" +
        "2023-01-04,2023-01-04,Wednesday,12:00,rad,gaming,Break,Played video games\n";

    private static DaylioData CreateSampleData()
    {
        using StringReader reader = new(SAMPLE_CSV);
        return new DaylioData(reader);
    }

    [Fact]
    public void ProcessMessage_Initialize_ReturnsProtocolVersionAndCapabilities()
    {
        McpServer server = new(CreateSampleData());
        string request = "{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"initialize\",\"params\":{\"protocolVersion\":\"2024-11-05\"}}";

        string responseJson = server.ProcessMessage(request);

        using JsonDocument doc = JsonDocument.Parse(responseJson);
        JsonElement root = doc.RootElement;
        Assert.Equal("2.0", root.GetProperty("jsonrpc").GetString());
        Assert.Equal(1, root.GetProperty("id").GetInt32());

        JsonElement result = root.GetProperty("result");
        Assert.Equal("2024-11-05", result.GetProperty("protocolVersion").GetString());
        Assert.Equal("daylio-mcp", result.GetProperty("serverInfo").GetProperty("name").GetString());
        Assert.True(result.TryGetProperty("capabilities", out _));
    }

    [Fact]
    public void ProcessMessage_Ping_ReturnsEmptyResult()
    {
        McpServer server = new(CreateSampleData());
        string request = "{\"jsonrpc\":\"2.0\",\"id\":2,\"method\":\"ping\"}";

        string responseJson = server.ProcessMessage(request);

        using JsonDocument doc = JsonDocument.Parse(responseJson);
        JsonElement root = doc.RootElement;
        Assert.Equal(2, root.GetProperty("id").GetInt32());
        Assert.True(root.TryGetProperty("result", out _));
    }

    [Fact]
    public void ProcessMessage_ToolsList_ReturnsAllConfiguredTools()
    {
        McpServer server = new(CreateSampleData());
        string request = "{\"jsonrpc\":\"2.0\",\"id\":3,\"method\":\"tools/list\"}";

        string responseJson = server.ProcessMessage(request);

        using JsonDocument doc = JsonDocument.Parse(responseJson);
        JsonElement tools = doc.RootElement.GetProperty("result").GetProperty("tools");

        Assert.Equal(10, tools.GetArrayLength());
        List<string> toolNames = new();
        foreach (JsonElement tool in tools.EnumerateArray())
        {
            toolNames.Add(tool.GetProperty("name").GetString()!);
        }

        Assert.Contains("load_dataset", toolNames);
        Assert.Contains("get_summary", toolNames);
        Assert.Contains("query_entries", toolNames);
        Assert.Contains("get_mood_distribution", toolNames);
        Assert.Contains("get_top_activities", toolNames);
        Assert.Contains("get_activity_mood_impact", toolNames);
        Assert.Contains("get_time_of_day_trends", toolNames);
        Assert.Contains("get_streaks", toolNames);
        Assert.Contains("generate_report", toolNames);
        Assert.Contains("get_activity_synergies", toolNames);
    }

    [Fact]
    public void ProcessMessage_ToolsCall_GetSummary_ReturnsAccurateMetrics()
    {
        McpServer server = new(CreateSampleData());
        string request = "{\"jsonrpc\":\"2.0\",\"id\":4,\"method\":\"tools/call\",\"params\":{\"name\":\"get_summary\"}}";

        string responseJson = server.ProcessMessage(request);

        using JsonDocument doc = JsonDocument.Parse(responseJson);
        JsonElement content = doc.RootElement.GetProperty("result").GetProperty("content")[0];
        string text = content.GetProperty("text").GetString()!;

        using JsonDocument summaryDoc = JsonDocument.Parse(text);
        Assert.Equal(4, summaryDoc.RootElement.GetProperty("totalEntries").GetInt32());
        Assert.Equal(4, summaryDoc.RootElement.GetProperty("totalDays").GetInt32());
        Assert.Equal(4, summaryDoc.RootElement.GetProperty("longestStreak").GetInt32());
    }

    [Fact]
    public void ProcessMessage_ToolsCall_QueryEntries_FiltersAccurately()
    {
        McpServer server = new(CreateSampleData());
        string request = "{\"jsonrpc\":\"2.0\",\"id\":5,\"method\":\"tools/call\",\"params\":{\"name\":\"query_entries\",\"arguments\":{\"activity\":\"coding\"}}}";

        string responseJson = server.ProcessMessage(request);

        using JsonDocument doc = JsonDocument.Parse(responseJson);
        JsonElement content = doc.RootElement.GetProperty("result").GetProperty("content")[0];
        string text = content.GetProperty("text").GetString()!;

        using JsonDocument entriesDoc = JsonDocument.Parse(text);
        Assert.Single(entriesDoc.RootElement.EnumerateArray());
    }

    [Fact]
    public void ProcessMessage_ToolsCall_GetActivityMoodImpact_CalculatesImpact()
    {
        McpServer server = new(CreateSampleData());
        string request = "{\"jsonrpc\":\"2.0\",\"id\":6,\"method\":\"tools/call\",\"params\":{\"name\":\"get_activity_mood_impact\",\"arguments\":{\"activity\":\"running\"}}}";

        string responseJson = server.ProcessMessage(request);

        using JsonDocument doc = JsonDocument.Parse(responseJson);
        JsonElement content = doc.RootElement.GetProperty("result").GetProperty("content")[0];
        string text = content.GetProperty("text").GetString()!;

        using JsonDocument impactDoc = JsonDocument.Parse(text);
        Assert.Equal("running", impactDoc.RootElement.GetProperty("activity").GetString());
        Assert.Equal(3, impactDoc.RootElement.GetProperty("frequencyWith").GetInt32());
    }

    [Fact]
    public void ProcessMessage_ResourcesListAndRead_ReturnsCorrectContent()
    {
        McpServer server = new(CreateSampleData());

        string listReq = "{\"jsonrpc\":\"2.0\",\"id\":7,\"method\":\"resources/list\"}";
        string listRes = server.ProcessMessage(listReq);
        using JsonDocument listDoc = JsonDocument.Parse(listRes);
        JsonElement resources = listDoc.RootElement.GetProperty("result").GetProperty("resources");
        Assert.Equal(3, resources.GetArrayLength());

        string readReq = "{\"jsonrpc\":\"2.0\",\"id\":8,\"method\":\"resources/read\",\"params\":{\"uri\":\"daylio://activities\"}}";
        string readRes = server.ProcessMessage(readReq);
        using JsonDocument readDoc = JsonDocument.Parse(readRes);
        JsonElement contents = readDoc.RootElement.GetProperty("result").GetProperty("contents")[0];
        string text = contents.GetProperty("text").GetString()!;

        using JsonDocument actDoc = JsonDocument.Parse(text);
        Assert.True(actDoc.RootElement.GetArrayLength() > 0);
    }

    [Fact]
    public void ProcessMessage_PromptsListAndGet_ReturnsPromptMessages()
    {
        McpServer server = new(CreateSampleData());

        string listReq = "{\"jsonrpc\":\"2.0\",\"id\":9,\"method\":\"prompts/list\"}";
        string listRes = server.ProcessMessage(listReq);
        using JsonDocument listDoc = JsonDocument.Parse(listRes);
        JsonElement prompts = listDoc.RootElement.GetProperty("result").GetProperty("prompts");
        Assert.Equal(2, prompts.GetArrayLength());

        string getReq = "{\"jsonrpc\":\"2.0\",\"id\":10,\"method\":\"prompts/get\",\"params\":{\"name\":\"weekly_reflection\",\"arguments\":{\"focus_topic\":\"running\"}}}";
        string getRes = server.ProcessMessage(getReq);
        using JsonDocument getDoc = JsonDocument.Parse(getRes);
        JsonElement messages = getDoc.RootElement.GetProperty("result").GetProperty("messages");
        Assert.Single(messages.EnumerateArray());
        string msgText = messages[0].GetProperty("content").GetProperty("text").GetString()!;
        Assert.Contains("running", msgText);
    }

    [Fact]
    public void ProcessMessage_UnknownMethod_ReturnsJsonRpcError()
    {
        McpServer server = new(CreateSampleData());
        string request = "{\"jsonrpc\":\"2.0\",\"id\":99,\"method\":\"non_existent_method\"}";

        string responseJson = server.ProcessMessage(request);

        using JsonDocument doc = JsonDocument.Parse(responseJson);
        JsonElement root = doc.RootElement;
        Assert.True(root.TryGetProperty("error", out JsonElement errElem));
        Assert.Equal(-32601, errElem.GetProperty("code").GetInt32());
    }

    [Fact]
    public void ProcessMessage_GetActivitySynergies_ReturnsSynergiesList()
    {
        McpServer server = new(CreateSampleData());
        string request = "{\"jsonrpc\":\"2.0\",\"id\":11,\"method\":\"tools/call\",\"params\":{\"name\":\"get_activity_synergies\",\"arguments\":{\"minOccurrences\":1}}}";

        string responseJson = server.ProcessMessage(request);

        using JsonDocument doc = JsonDocument.Parse(responseJson);
        JsonElement content = doc.RootElement.GetProperty("result").GetProperty("content");
        string text = content[0].GetProperty("text").GetString()!;

        Assert.Contains("activity1", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("activity2", text, StringComparison.OrdinalIgnoreCase);
    }
}

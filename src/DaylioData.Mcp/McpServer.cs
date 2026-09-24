using System.Globalization;
using System.Text.Json;
using DaylioData.Mcp.Handlers;
using DaylioData.Mcp.Protocol;

namespace DaylioData.Mcp;

/// <summary>
/// Core Model Context Protocol (MCP) server managing JSON-RPC 2.0 communication over stdio.
/// </summary>
public class McpServer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private readonly object _stateLock = new();
    private DaylioData? _daylioData;
    private string? _currentFilePath;

    public DaylioData? CurrentData
    {
        get
        {
            lock (_stateLock)
            {
                return _daylioData;
            }
        }
    }

    public string? CurrentFilePath
    {
        get
        {
            lock (_stateLock)
            {
                return _currentFilePath;
            }
        }
    }

    public McpServer(DaylioData? initialData = null, string? initialFilePath = null)
    {
        _daylioData = initialData;
        _currentFilePath = initialFilePath;
    }

    public void SetDataset(DaylioData data, string filePath)
    {
        lock (_stateLock)
        {
            _daylioData = data;
            _currentFilePath = filePath;
        }
    }

    /// <summary>
    /// Starts the server communication loop on the provided input/output streams.
    /// </summary>
    /// <param name="reader">The input reader (typically Console.In).</param>
    /// <param name="writer">The output writer (typically Console.Out).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task RunAsync(
        TextReader reader,
        TextWriter writer,
        CancellationToken cancellationToken = default)
    {
        Console.Error.WriteLine("[daylio-mcp] Server initialized and listening on stdio.");

        while (!cancellationToken.IsCancellationRequested)
        {
            string? line = await reader.ReadLineAsync(cancellationToken);
            if (line is null)
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string responseJson = ProcessMessage(line);
            if (!string.IsNullOrEmpty(responseJson))
            {
                await writer.WriteLineAsync(responseJson.AsMemory(), cancellationToken);
                await writer.FlushAsync(cancellationToken);
            }
        }

        Console.Error.WriteLine("[daylio-mcp] Server shutting down.");
    }

    /// <summary>
    /// Processes a single raw incoming JSON-RPC line and returns the serialized response.
    /// </summary>
    /// <param name="rawJson">The raw JSON string.</param>
    /// <returns>The serialized JSON-RPC response or empty string if notification.</returns>
    public string ProcessMessage(string rawJson)
    {
        JsonRpcRequest? request;
        try
        {
            request = JsonSerializer.Deserialize<JsonRpcRequest>(rawJson, JsonOptions);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[daylio-mcp] Parse error: {ex.Message}");
            JsonRpcResponse errorResponse = new("2.0", null, null, new JsonRpcError(-32700, "Parse error"));
            return JsonSerializer.Serialize(errorResponse, JsonOptions);
        }

        if (request is null)
        {
            JsonRpcResponse errorResponse = new("2.0", null, null, new JsonRpcError(-32600, "Invalid Request"));
            return JsonSerializer.Serialize(errorResponse, JsonOptions);
        }

        // Notifications (no id) do not produce a response
        if (request.Id is null && request.Method.StartsWith("notifications/", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        JsonRpcResponse response = HandleMethod(request);
        return JsonSerializer.Serialize(response, JsonOptions);
    }

    private JsonRpcResponse HandleMethod(JsonRpcRequest request)
    {
        try
        {
            switch (request.Method.ToLowerInvariant())
            {
                case "initialize":
                    return HandleInitialize(request);

                case "ping":
                    return new JsonRpcResponse("2.0", request.Id, new { });

                case "tools/list":
                    return new JsonRpcResponse("2.0", request.Id, new
                    {
                        tools = ToolHandler.GetToolDefinitions()
                    });

                case "tools/call":
                    return HandleToolCall(request);

                case "resources/list":
                    return new JsonRpcResponse("2.0", request.Id, new
                    {
                        resources = ResourceHandler.GetResourceDefinitions()
                    });

                case "resources/read":
                    return HandleResourceRead(request);

                case "prompts/list":
                    return new JsonRpcResponse("2.0", request.Id, new
                    {
                        prompts = PromptHandler.GetPromptDefinitions()
                    });

                case "prompts/get":
                    return HandlePromptGet(request);

                default:
                    return new JsonRpcResponse(
                        "2.0",
                        request.Id,
                        null,
                        new JsonRpcError(-32601, $"Method not found: '{request.Method}'"));
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[daylio-mcp] Internal error executing {request.Method}: {ex}");
            return new JsonRpcResponse(
                "2.0",
                request.Id,
                null,
                new JsonRpcError(-32603, $"Internal error: {ex.Message}"));
        }
    }

    private static JsonRpcResponse HandleInitialize(JsonRpcRequest request)
    {
        string clientProtocol = "2024-11-05";
        if (request.Params.HasValue &&
            request.Params.Value.TryGetProperty("protocolVersion", out JsonElement protoElem))
        {
            string? requestedProto = protoElem.GetString();
            if (!string.IsNullOrWhiteSpace(requestedProto))
            {
                clientProtocol = requestedProto;
            }
        }

        InitializeResult result = new(
            ProtocolVersion: clientProtocol,
            Capabilities: new ServerCapabilities(
                Tools: new { },
                Resources: new { },
                Prompts: new { }),
            ServerInfo: new Implementation("daylio-mcp", "0.1.0"));

        return new JsonRpcResponse("2.0", request.Id, result);
    }

    private JsonRpcResponse HandleToolCall(JsonRpcRequest request)
    {
        if (!request.Params.HasValue ||
            !request.Params.Value.TryGetProperty("name", out JsonElement nameElement))
        {
            return new JsonRpcResponse(
                "2.0",
                request.Id,
                null,
                new JsonRpcError(-32602, "Invalid params: missing 'name'"));
        }

        string? toolName = nameElement.GetString();
        if (string.IsNullOrWhiteSpace(toolName))
        {
            return new JsonRpcResponse(
                "2.0",
                request.Id,
                null,
                new JsonRpcError(-32602, "Invalid params: empty 'name'"));
        }

        JsonElement? arguments = null;
        if (request.Params.Value.TryGetProperty("arguments", out JsonElement argsElement))
        {
            arguments = argsElement;
        }

        ToolCallResult result = ToolHandler.ExecuteTool(
            toolName,
            arguments,
            () => CurrentData,
            SetDataset);

        return new JsonRpcResponse("2.0", request.Id, result);
    }

    private JsonRpcResponse HandleResourceRead(JsonRpcRequest request)
    {
        if (!request.Params.HasValue ||
            !request.Params.Value.TryGetProperty("uri", out JsonElement uriElement))
        {
            return new JsonRpcResponse(
                "2.0",
                request.Id,
                null,
                new JsonRpcError(-32602, "Invalid params: missing 'uri'"));
        }

        string? uri = uriElement.GetString();
        if (string.IsNullOrWhiteSpace(uri))
        {
            return new JsonRpcResponse(
                "2.0",
                request.Id,
                null,
                new JsonRpcError(-32602, "Invalid params: empty 'uri'"));
        }

        ResourceReadResult result = ResourceHandler.ReadResource(uri, () => CurrentData);
        return new JsonRpcResponse("2.0", request.Id, result);
    }

    private JsonRpcResponse HandlePromptGet(JsonRpcRequest request)
    {
        if (!request.Params.HasValue ||
            !request.Params.Value.TryGetProperty("name", out JsonElement nameElement))
        {
            return new JsonRpcResponse(
                "2.0",
                request.Id,
                null,
                new JsonRpcError(-32602, "Invalid params: missing 'name'"));
        }

        string? promptName = nameElement.GetString();
        if (string.IsNullOrWhiteSpace(promptName))
        {
            return new JsonRpcResponse(
                "2.0",
                request.Id,
                null,
                new JsonRpcError(-32602, "Invalid params: empty 'name'"));
        }

        JsonElement? arguments = null;
        if (request.Params.Value.TryGetProperty("arguments", out JsonElement argsElement))
        {
            arguments = argsElement;
        }

        PromptGetResult result = PromptHandler.GetPrompt(promptName, arguments, () => CurrentData);
        return new JsonRpcResponse("2.0", request.Id, result);
    }
}

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DaylioData.Mcp.Protocol;

/// <summary>
/// Standard JSON-RPC 2.0 request payload.
/// </summary>
public record JsonRpcRequest(
    [property: JsonPropertyName("jsonrpc")] string JsonRpc,
    [property: JsonPropertyName("id")] JsonElement? Id,
    [property: JsonPropertyName("method")] string Method,
    [property: JsonPropertyName("params")] JsonElement? Params);

/// <summary>
/// Standard JSON-RPC 2.0 response payload.
/// </summary>
public record JsonRpcResponse(
    [property: JsonPropertyName("jsonrpc")] string JsonRpc,
    [property: JsonPropertyName("id")] JsonElement? Id,
    [property: JsonPropertyName("result")] object? Result = null,
    [property: JsonPropertyName("error")] JsonRpcError? Error = null);

/// <summary>
/// Standard JSON-RPC 2.0 error object.
/// </summary>
public record JsonRpcError(
    [property: JsonPropertyName("code")] int Code,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("data")] object? Data = null);

/// <summary>
/// Information describing an MCP implementation.
/// </summary>
public record Implementation(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("version")] string Version);

/// <summary>
/// Capabilities exposed by the MCP server.
/// </summary>
public record ServerCapabilities(
    [property: JsonPropertyName("tools")] object? Tools,
    [property: JsonPropertyName("resources")] object? Resources,
    [property: JsonPropertyName("prompts")] object? Prompts);

/// <summary>
/// Payload returned from the MCP initialize handshake.
/// </summary>
public record InitializeResult(
    [property: JsonPropertyName("protocolVersion")] string ProtocolVersion,
    [property: JsonPropertyName("capabilities")] ServerCapabilities Capabilities,
    [property: JsonPropertyName("serverInfo")] Implementation ServerInfo);

/// <summary>
/// Definition of a tool exposed via MCP.
/// </summary>
public record McpTool(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("inputSchema")] object InputSchema);

/// <summary>
/// Content block inside a tool execution result.
/// </summary>
public record ToolCallContent(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("text")] string Text);

/// <summary>
/// Result returned from a tool call execution.
/// </summary>
public record ToolCallResult(
    [property: JsonPropertyName("content")] List<ToolCallContent> Content,
    [property: JsonPropertyName("isError")] bool IsError = false);

/// <summary>
/// Definition of a resource exposed via MCP.
/// </summary>
public record McpResource(
    [property: JsonPropertyName("uri")] string Uri,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("mimeType")] string? MimeType);

/// <summary>
/// Text content block of an MCP resource.
/// </summary>
public record ResourceContent(
    [property: JsonPropertyName("uri")] string Uri,
    [property: JsonPropertyName("mimeType")] string? MimeType,
    [property: JsonPropertyName("text")] string Text);

/// <summary>
/// Result returned when reading an MCP resource.
/// </summary>
public record ResourceReadResult(
    [property: JsonPropertyName("contents")] List<ResourceContent> Contents);

/// <summary>
/// An argument for an MCP prompt template.
/// </summary>
public record McpPromptArgument(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("required")] bool Required);

/// <summary>
/// Definition of an MCP prompt template.
/// </summary>
public record McpPrompt(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("arguments")] List<McpPromptArgument>? Arguments);

/// <summary>
/// Text content block for a prompt message.
/// </summary>
public record PromptMessageContent(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("text")] string Text);

/// <summary>
/// Message in an MCP prompt template.
/// </summary>
public record PromptMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] PromptMessageContent Content);

/// <summary>
/// Result returned from an MCP prompt retrieval.
/// </summary>
public record PromptGetResult(
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("messages")] List<PromptMessage> Messages);

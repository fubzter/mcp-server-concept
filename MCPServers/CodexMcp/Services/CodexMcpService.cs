using MCPServers.Shared;
using Microsoft.Extensions.Logging;

namespace CodexMcp.Services;

public class CodexMcpService : BaseHttpService
{
    public CodexMcpService(IConfiguration configuration, HttpClient client, ILogger<CodexMcpService> logger)
        : base(configuration, client, logger)
    {
    }

    public Task<CodexMcpResponse> GetDataAsync(string input)
    {
        var response = new CodexMcpResponse(
            Input: input,
            Message: $"CodexMcp received '{input}'. Replace this placeholder with real business logic when the downstream API is known.",
            TimestampUtc: DateTimeOffset.UtcNow);

        return Task.FromResult(response);
    }
}

public sealed record CodexMcpResponse(string Input, string Message, DateTimeOffset TimestampUtc);

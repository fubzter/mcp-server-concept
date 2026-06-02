using System.ComponentModel;
using ModelContextProtocol.Server;
using CodexMcp.Services;

namespace CodexMcp;

[McpServerToolType]
public class CodexMcpTool
{
    private readonly CodexMcpService _service;

    public CodexMcpTool(CodexMcpService service)
    {
        _service = service;
    }

    [McpServerTool, Description("Returns a test response from the CodexMcp server.")]
    public async Task<object> Echo(
        [Description("Text to include in the CodexMcp response.")] string input)
    {
        return await _service.GetDataAsync(input);
    }
}

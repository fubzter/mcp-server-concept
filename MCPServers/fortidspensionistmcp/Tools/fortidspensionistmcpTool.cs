using System.ComponentModel;
using ModelContextProtocol.Server;
using fortidspensionistmcp.Services;

namespace fortidspensionistmcp;

[McpServerToolType]
public class fortidspensionistmcpTool
{
    private readonly fortidspensionistmcpService _service;

    public fortidspensionistmcpTool(fortidspensionistmcpService service)
    {
        _service = service;
    }

    [McpServerTool, Description(
        "Search records in a CKAN DataStore resource. " +
        "Supports full-text search (q), field filters (JSON object as string), " +
        "field selection, sorting, and pagination. " +
        "Returns a JSON object with 'records', 'total', and 'fields' metadata.")]
    public async Task<string> DatastoreSearch(
        [Description("The resource ID (UUID) to search in, e.g. \"c487202a-1f21-4159-b133-f02787a8ed62\".")]
        string resourceId,
        [Description("Full-text search string across all fields. Optional.")]
        string? q = null,
        [Description("Maximum number of records to return. Defaults to 100.")]
        int limit = 100,
        [Description("Number of records to skip (for pagination). Defaults to 0.")]
        int offset = 0,
        [Description("Comma-separated list of field names to return. Returns all fields if omitted.")]
        string? fields = null,
        [Description("JSON object of field-to-value filters, e.g. {\"status\":\"active\"}. Optional.")]
        string? filters = null,
        [Description("Sort expression, e.g. \"fieldname asc\" or \"fieldname desc\". Optional.")]
        string? sort = null)
    {
        return await _service.DatastoreSearchAsync(resourceId, q, limit, offset, fields, filters, sort);
    }

    [McpServerTool, Description(
        "Search a CKAN DataStore resource using a raw SQL SELECT statement. " +
        "The table name is the resource ID (UUID) enclosed in double quotes. " +
        "Example: SELECT * FROM \"c487202a-1f21-4159-b133-f02787a8ed62\" WHERE title LIKE 'jones' " +
        "Returns a JSON object with 'records' and 'fields' metadata.")]
    public async Task<string> DatastoreSearchSql(
        [Description("Valid SQL SELECT statement targeting a resource UUID as the table name.")]
        string sql)
    {
        return await _service.DatastoreSearchSqlAsync(sql);
    }

    [McpServerTool, Description(
        "Retrieve field schema (names and types) for a CKAN DataStore resource without fetching data records. " +
        "Useful for understanding available columns before constructing queries.")]
    public async Task<string> DatastoreInfo(
        [Description("The resource ID (UUID) to inspect, e.g. \"c487202a-1f21-4159-b133-f02787a8ed62\".")]
        string resourceId)
    {
        return await _service.DatastoreInfoAsync(resourceId);
    }
}

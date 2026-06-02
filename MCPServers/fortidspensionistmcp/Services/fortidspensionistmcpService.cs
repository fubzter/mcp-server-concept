using MCPServers.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Web;

namespace fortidspensionistmcp.Services;

public class fortidspensionistmcpService : BaseHttpService
{
    private readonly string _baseUrl;

    public fortidspensionistmcpService(
        IConfiguration configuration,
        HttpClient client,
        ILogger<fortidspensionistmcpService> logger)
        : base(configuration, client, logger)
    {
        _baseUrl = configuration["fortidspensionistmcpApi:BaseUrl"]
            ?? throw new InvalidOperationException("fortidspensionistmcpApi:BaseUrl is not configured");
    }

    /// <summary>
    /// Search a CKAN DataStore resource using field filters and/or a full-text query.
    /// Maps to /api/3/action/datastore_search.
    /// </summary>
    public async Task<string> DatastoreSearchAsync(
        string resourceId,
        string? q = null,
        int limit = 100,
        int offset = 0,
        string? fields = null,
        string? filters = null,
        string? sort = null)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["resource_id"] = resourceId;
        if (!string.IsNullOrWhiteSpace(q)) query["q"] = q;
        query["limit"] = limit.ToString();
        query["offset"] = offset.ToString();
        if (!string.IsNullOrWhiteSpace(fields)) query["fields"] = fields;
        if (!string.IsNullOrWhiteSpace(filters)) query["filters"] = filters;
        if (!string.IsNullOrWhiteSpace(sort)) query["sort"] = sort;

        var url = $"{_baseUrl}/api/3/action/datastore_search?{query}";
        return await GetAsync(url);
    }

    /// <summary>
    /// Search a CKAN DataStore resource using a SQL SELECT statement.
    /// Maps to /api/3/action/datastore_search_sql.
    /// </summary>
    public async Task<string> DatastoreSearchSqlAsync(string sql)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["sql"] = sql;
        var url = $"{_baseUrl}/api/3/action/datastore_search_sql?{query}";
        return await GetAsync(url);
    }

    /// <summary>
    /// Retrieve metadata about a CKAN DataStore resource (field names and types).
    /// Maps to /api/3/action/datastore_search with limit=0.
    /// </summary>
    public async Task<string> DatastoreInfoAsync(string resourceId)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["resource_id"] = resourceId;
        query["limit"] = "0";
        var url = $"{_baseUrl}/api/3/action/datastore_search?{query}";
        return await GetAsync(url);
    }
}

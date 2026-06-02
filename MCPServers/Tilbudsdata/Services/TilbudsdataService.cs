using MCPServers.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Tilbudsdata.Services;

public class TilbudsdataService : BaseHttpService
{
    private readonly string _baseUrl;
    private readonly string _userId;
    private readonly string _apiKey;

    public TilbudsdataService(IConfiguration configuration, HttpClient client, ILogger<TilbudsdataService> logger)
        : base(configuration, client, logger)
    {
        _baseUrl = configuration["TilbudsdataApi:BaseUrl"]
            ?? throw new InvalidOperationException("TilbudsdataApi:BaseUrl is not configured");
        _baseUrl = _baseUrl.TrimEnd('/');
        _userId = configuration["TilbudsdataApi:UserId"]
            ?? throw new InvalidOperationException("TilbudsdataApi:UserId is not configured");
        _apiKey = configuration["TilbudsdataApi:ApiKey"]
            ?? throw new InvalidOperationException("TilbudsdataApi:ApiKey is not configured");
    }

    public Task<string> GetBrandsAsync(int page = 0)
    {
        var url = BuildSignedUrl("brand/getAll", [new("page", page.ToString())]);
        return GetAsync(url);
    }

    public Task<string> GetCategoriesAsync()
    {
        var url = BuildSignedUrl("category/getAll");
        return GetAsync(url);
    }

    public Task<string> GetChainsAsync(int page = 0)
    {
        var url = BuildSignedUrl("chain/getAll", [new("page", page.ToString())]);
        return GetAsync(url);
    }

    public Task<string> GetProductNamesAsync(int page = 0)
    {
        var url = BuildSignedUrl("productName/getAll", [new("page", page.ToString())]);
        return GetAsync(url);
    }

    public Task<string> SearchOffersAsync(string queryStringWithoutAuth)
    {
        var url = BuildSignedUrlFromRawQuery("offer/search", queryStringWithoutAuth);
        return GetAsync(url);
    }

    public Task<string> CallEndpointAsync(string endpoint, string queryStringWithoutAuth = "")
    {
        var url = BuildSignedUrlFromRawQuery(endpoint, queryStringWithoutAuth);
        return GetAsync(url);
    }

    private string BuildSignedUrl(
        string endpoint,
        IReadOnlyList<KeyValuePair<string, string?>>? parameters = null)
    {
        var queryParts = new List<string>();
        if (parameters is not null)
        {
            foreach (var parameter in parameters)
            {
                if (!string.IsNullOrWhiteSpace(parameter.Value))
                {
                    queryParts.Add($"{Uri.EscapeDataString(parameter.Key)}={Uri.EscapeDataString(parameter.Value)}");
                }
            }
        }

        return BuildSignedUrlFromQueryParts(endpoint, queryParts);
    }

    private string BuildSignedUrlFromRawQuery(string endpoint, string? queryStringWithoutAuth)
    {
        var queryParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(queryStringWithoutAuth))
        {
            var trimmed = queryStringWithoutAuth.Trim().TrimStart('?').Trim('&');
            if (!string.IsNullOrWhiteSpace(trimmed))
            {
                queryParts.AddRange(trimmed.Split('&', StringSplitOptions.RemoveEmptyEntries));
            }
        }

        return BuildSignedUrlFromQueryParts(endpoint, queryParts);
    }

    private string BuildSignedUrlFromQueryParts(string endpoint, List<string> queryParts)
    {
        var path = "/" + endpoint.Trim().Trim('/');
        var expiration = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds().ToString();

        queryParts.Add($"_userId={Uri.EscapeDataString(_userId)}");
        queryParts.Add($"_expiration={expiration}");

        var unsignedPathAndQuery = $"{path}?{string.Join('&', queryParts)}";
        var signature = ComputeSha256Hex(unsignedPathAndQuery + _apiKey);

        return $"{_baseUrl}{unsignedPathAndQuery}&_signature={signature}";
    }

    private static string ComputeSha256Hex(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

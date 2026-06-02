using System.ComponentModel;
using ModelContextProtocol.Server;
using Tilbudsdata.Services;

namespace Tilbudsdata;

[McpServerToolType]
public class TilbudsdataTool
{
    private readonly TilbudsdataService _service;

    public TilbudsdataTool(TilbudsdataService service)
    {
        _service = service;
    }

    [McpServerTool, Description("Get a page of brands from Tilbudsdata.")]
    public Task<string> GetBrands(
        [Description("Zero-based page number.")] int page = 0)
    {
        return _service.GetBrandsAsync(page);
    }

    [McpServerTool, Description("Get all categories from Tilbudsdata.")]
    public Task<string> GetCategories()
    {
        return _service.GetCategoriesAsync();
    }

    [McpServerTool, Description("Get a page of chains from Tilbudsdata.")]
    public Task<string> GetChains(
        [Description("Zero-based page number.")] int page = 0)
    {
        return _service.GetChainsAsync(page);
    }

    [McpServerTool, Description("Get a page of product names from Tilbudsdata.")]
    public Task<string> GetProductNames(
        [Description("Zero-based page number.")] int page = 0)
    {
        return _service.GetProductNamesAsync(page);
    }

    [McpServerTool, Description("Search offers in Tilbudsdata. Pass only endpoint-specific query parameters, without _userId, _expiration, or _signature.")]
    public Task<string> SearchOffers(
        [Description("Query string without auth parameters, for example: page=0&categoryId=123")] string queryStringWithoutAuth = "page=0")
    {
        return _service.SearchOffersAsync(queryStringWithoutAuth);
    }

    [McpServerTool, Description("Call a Tilbudsdata endpoint with signed authentication. Use endpoint paths like brand/getAll, category/getAll, or offer/search.")]
    public Task<string> CallEndpoint(
        [Description("Endpoint path without leading slash, for example: brand/getAll")] string endpoint,
        [Description("Query string without auth parameters, for example: page=0")] string queryStringWithoutAuth = "")
    {
        return _service.CallEndpointAsync(endpoint, queryStringWithoutAuth);
    }
}

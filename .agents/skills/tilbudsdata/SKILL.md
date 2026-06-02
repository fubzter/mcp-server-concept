---
name: tilbudsdata
description: Use when the user asks Codex to work with Danish offer/catalog data from Tilbudsdata, api.tilbudsdata.dk, grocery offers, brands, categories, chains, product names, or when the Tilbudsdata MCP server/tools should be used. This skill tells Codex how to call the tilbudsdata MCP tools and shape queries safely.
---

# Tilbudsdata

Use the `tilbudsdata` MCP server for live data from `api.tilbudsdata.dk`.

## MCP dependency

The MCP server should be configured as:

```toml
[mcp_servers.tilbudsdata]
url = "https://tilbudsdata.orangehill-a3d65116.westeurope.azurecontainerapps.io/mcp"
```

If the tools are not visible, ask the user to restart Codex and check `/mcp`.

## Tool selection

- `GetCategories` - discover category IDs before filtering offers by category.
- `GetChains` - discover supermarket/retail chain IDs before filtering by chain.
- `GetBrands` - discover brand IDs before filtering by brand.
- `GetProductNames` - discover product-name IDs before filtering by product name.
- `SearchOffers` - search offers when the endpoint is `offer/search`; pass only endpoint-specific query parameters.
- `CallEndpoint` - fallback for any documented Tilbudsdata endpoint not covered by a dedicated tool.

## Query rules

For `SearchOffers` and `CallEndpoint`, pass only the business query string, for example:

```text
page=0&categoryId=123
```

Do not include these authentication parameters:

- `_userId`
- `_expiration`
- `_signature`

The MCP server adds and signs those parameters.

Use zero-based paging unless the API response clearly indicates otherwise. If the user asks a broad question, start with discovery tools to find IDs, then call `SearchOffers` with the narrowest useful filters.

## Workflow

1. Identify whether the user needs discovery data or offer search.
2. Use discovery tools first when the user names a category, chain, brand, or product in plain Danish.
3. Map names to IDs from the discovery response.
4. Call `SearchOffers` with a concise query string.
5. Summarize results in Danish by default, including offer name, chain/store, price, unit price when present, and validity dates when present.
6. If the API response is paged and the user asks for more coverage, continue with the next page.

## Examples

- "Find tilbud på kaffe" -> discover product/category if needed, then `SearchOffers`.
- "Hvilke kategorier findes der?" -> `GetCategories`.
- "Find Netto-tilbud i kategori X" -> `GetChains`, `GetCategories`, then `SearchOffers`.
- "Kald endpointet `brand/getAll`" -> `CallEndpoint` with endpoint `brand/getAll` and query `page=0`.

For endpoint details beyond the core workflow, read `references/endpoints.md`.

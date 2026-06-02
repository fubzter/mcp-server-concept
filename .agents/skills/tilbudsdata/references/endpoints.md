# Tilbudsdata Endpoint Notes

Base API: `https://api.tilbudsdata.dk`

The public API documentation states that requests are authenticated with:

- `_userId`
- `_expiration`
- `_signature`

The MCP server computes `_signature` as SHA-256 over the request path/query without `_signature`, followed by the API key. Codex should never ask the user for the API key during normal use; the deployed server reads credentials from Azure Key Vault.

Known endpoints exposed directly by the MCP server:

| MCP tool | API endpoint | Notes |
| --- | --- | --- |
| `GetBrands` | `brand/getAll` | Supports `page`. |
| `GetCategories` | `category/getAll` | Use before category-filtered offer searches. |
| `GetChains` | `chain/getAll` | Supports `page`; use before chain-filtered searches. |
| `GetProductNames` | `productName/getAll` | Supports `page`; use before product-name filtered searches. |
| `SearchOffers` | `offer/search` | Pass query parameters without auth fields. |
| `CallEndpoint` | Any endpoint path | Fallback for documented endpoints. |

When unsure about a filter parameter name, use discovery endpoints first and then call `SearchOffers` with the simplest query. If the response indicates a different parameter name, adapt to the response rather than guessing.

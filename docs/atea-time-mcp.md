# Atea Time MCP

`AteaTimeMcp` is a local MCP server for Atea Time time registration.

It is intentionally local-first: each consultant runs the server on their own
machine, and the server reads that consultant's own Atea Time browser session.
Do not deploy this server as a central Azure Container App unless Atea Time gets
a supported OAuth or service-to-service authentication flow.

## What It Can Do

- Show the signed-in Atea Time identity.
- List open Atea Time cases.
- List current drafts.
- Show week summaries with remaining hours to 7.5 per day.
- Create project drafts.
- Create internal education/activity drafts.
- Delete drafts.
- Submit drafts in a date range.

Write operations are dry-run by default. Tools only write to Atea Time when the
caller sets `confirm=true` after the user has approved the exact action.

## Security Model

The server does not store credentials. It scans local browser storage for the
encrypted `timeapp_userData` record that Atea Time already stores after login,
decrypts it in memory, and uses the bearer token for API calls.

Supported local browser storage locations:

- macOS Codex in-app browser
- macOS Google Chrome default profile
- Windows Google Chrome default profile

Each colleague must log in to Atea Time themselves. Never share tokens or run the
server on behalf of another user.

## Install For A Colleague

### 1. Prerequisites

Install:

- .NET 9 SDK
- Git
- Codex, Claude Desktop, VS Code, or another MCP-capable client

### 2. Clone The Repo

```bash
git clone https://github.com/fubzter/mcp-server-concept.git
cd mcp-server-concept
```

### 3. Build The Server

```bash
dotnet build MCPServers/AteaTimeMcp/AteaTimeMcp.csproj
```

### 4. Log In To Atea Time

Open Atea Time and log in:

```text
https://mobile.atea.com/AteaTimeRegistrationProduction5.0/menu
```

Use the same browser profile that the MCP server can read. On macOS with Codex,
logging in through the Codex in-app browser is supported. Chrome default profile
is also supported.

### 5. Run Locally

```bash
dotnet run --project MCPServers/AteaTimeMcp/AteaTimeMcp.csproj
```

Default endpoint:

```text
http://localhost:4551/mcp
```

## Codex Configuration

Run the registration script:

```powershell
pwsh -NoProfile -File scripts/Register-AteaTimeMcpCodex.ps1
```

Restart Codex after running the script.

The script adds or replaces this block in `~/.codex/config.toml`:

```toml
[mcp_servers.atea_time]
command = "dotnet"
args = [
  "run",
  "--project",
  "/absolute/path/to/mcp-server-concept/MCPServers/AteaTimeMcp/AteaTimeMcp.csproj"
]
startup_timeout_sec = 120
```

For a repo installed in `~/mcp-server-concept`, the `args` path would be:

```text
/Users/<username>/mcp-server-concept/MCPServers/AteaTimeMcp/AteaTimeMcp.csproj
```

## Tool Usage Pattern

Recommended assistant behavior:

1. Read first:
   - `WhoAmI`
   - `ListOpenCases`
   - `ListDrafts`
   - `WeekSummary`
2. For create/delete/submit:
   - run once with `confirm=false`
   - show the exact draft/action summary to the user
   - run again with `confirm=true` only after explicit approval

## Example Prompts

```text
Vis mine åbne Blue Power Partners sager.
```

```text
Lav et ugeoverblik over mine kladder.
```

```text
Opret en kladde på sag 13325-1-143 onsdag 08:30-11:00 med teksten
Uddannelse Github Actions Certificering.
```

```text
Vis de kladder du vil sende, og send dem først når jeg skriver godkend.
```

## Current Tool Names

- `WhoAmI`
- `ListOpenCases`
- `ListDrafts`
- `WeekSummary`
- `CreateProjectDraft`
- `CreateInternalDraft`
- `DeleteDraft`
- `SubmitDrafts`

## Known Limitations

- The server depends on Atea Time's current browser storage and API shape.
- Token discovery supports default browser profiles only.
- Users must be logged in locally; expired sessions require a fresh browser login.
- The internal education activity uses Atea Time's current internal activity
  backing case `13325-1-147 / 001`.
- This is not a central multi-user service. A central version needs a supported
  Atea Time authentication flow per user.

## Maintenance

After changes:

```bash
dotnet build MCPServers/AteaTimeMcp/AteaTimeMcp.csproj
```

Do not add Azure Container App workflow or bicepparam files for this server
unless the authentication model changes away from local browser sessions.

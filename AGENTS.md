## Global Codex Guidance

- Keep user-owned Codex assets in this repository as the sync source. Local
  `~/.codex` files are installed copies unless explicitly edited in place.
- After meaningful multi-step work, use the `retrospective` skill to surface
  only durable updates to skills, docs, conventions, workflow, or shared memory.
- Store cross-project workflow preferences here when the user explicitly asks
  Codex to remember them. Store repo-specific guidance in that repo's
  `AGENTS.md`, and skill workflow changes in the relevant `SKILL.md`.
- Treat this repository as the cross-machine source of truth for Codex memory,
  skills, rules, and settings. When a durable preference should apply across
  chats and projects, update this `AGENTS.md`, run `install.ps1 -SkipConfig`
  when present, then commit and push so the same memory is available on the
  user's other machines after pull/install.
- Automate cross-machine Codex sync as much as practical. Default to pulling
  this repository and running `install.ps1 -SkipConfig` on each machine when
  that script exists; only update `settings/config.toml` when config changes
  are intentional.
- On Windows, prefer registering `register-windows-sync-task.ps1` when present
  so Codex skills and shared guidance sync at user logon before normal
  Codex/VS Code work begins. Do not assume Codex app startup itself runs sync
  hooks.
- On macOS, prefer registering `register-macos-sync-task.ps1` when present so
  the same sync runs via a user LaunchAgent at login before normal Codex/VS
  Code work begins.
- Non-customer projects normally live in the user's GitHub repositories.
  Customer-specific projects normally live in Azure DevOps repositories and
  should be cloned/opened in VS Code on the active machine rather than stored
  in this Codex settings repository.

## CodexMcp Cross-Machine Memory

- The user's active reusable MCP server is `CodexMcp` in this repository.
  Codex should remember and use it as part of the user's Codex workflow across
  machines when the relevant MCP client configuration is available.
- Dev MCP endpoint:
  `https://codexmcp.orangehill-a3d65116.westeurope.azurecontainerapps.io`.
- Dev Azure resources:
  - Subscription: `54963670-1dc4-4dd6-9214-43974790d43f`
  - Tenant: `b3f0b16b-81f9-4c36-a9ba-2b7fc139f0cb`
  - Environment: `mymcpdev`
  - Resource group: `rg-mymcpdev`
  - Key Vault: `mymcpdev`
  - ACR: `fubztermcpacr`
- `CodexMcp` is intentionally self-contained for now. It keeps Entra ID auth
  for the MCP endpoint but does not require a downstream API base URL or API
  key. Replace the placeholder echo behavior only when a real downstream API
  is known.
- Use these repo workflows for future MCP server work:
  - `/setup-deployment` for shared Azure/GitHub Actions infrastructure.
  - `/new-mcp-server` for server scaffolding.
  - `/create-mcp-account` for the MCP app registration and Key Vault secrets.
  - `/create-agent-account` for Copilot/APIM client app registration.
  - `/set-reply-uri` for OAuth redirect URIs.
- Current CodexMcp GitHub Actions workflow:
  `.github/workflows/docker-publish-codexmcp.yml`.
- Current CodexMcp custom connector:
  `Copilot/CustomConnectors/CodexMcp.swagger.json`.
- For this repo, after first dev deploy of any MCP server, read the Container
  App FQDN and update both `EntraIdAuth__PublicUrl` in the dev bicepparam file
  and the custom connector server URL, then push a redeploy.
- The generated custom connector scope must be `mcp.tools`, matching
  `scripts/New-EntraAppRegistration.ps1`.

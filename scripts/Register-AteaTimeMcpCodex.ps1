<#
.SYNOPSIS
    Registers the local AteaTimeMcp server in Codex config.

.DESCRIPTION
    Adds or replaces the [mcp_servers.atea_time] block in ~/.codex/config.toml.
    The server runs locally with dotnet and reads the signed-in user's own Atea
    Time browser session.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$ConfigPath = (Join-Path $HOME '.codex/config.toml'),

    [Parameter(Mandatory = $false)]
    [string]$RepoRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'

$projectPath = Join-Path $RepoRoot 'MCPServers/AteaTimeMcp/AteaTimeMcp.csproj'
if (-not (Test-Path $projectPath)) {
    throw "AteaTimeMcp project not found: $projectPath"
}

$configDir = Split-Path -Parent $ConfigPath
if (-not (Test-Path $configDir)) {
    New-Item -ItemType Directory -Path $configDir -Force | Out-Null
}

$normalizedProjectPath = $projectPath.Replace('\', '/')
$block = @"
[mcp_servers.atea_time]
command = "dotnet"
args = [
  "run",
  "--project",
  "$normalizedProjectPath"
]
startup_timeout_sec = 120
"@

$content = ''
if (Test-Path $ConfigPath) {
    $content = Get-Content -Path $ConfigPath -Raw -Encoding UTF8
}

$pattern = '(?ms)^\[mcp_servers\.atea_time\]\r?\n.*?(?=^\[|\z)'
if ([regex]::IsMatch($content, $pattern)) {
    $content = [regex]::Replace($content, $pattern, $block.TrimEnd() + [Environment]::NewLine)
} else {
    if (-not [string]::IsNullOrWhiteSpace($content) -and -not $content.EndsWith([Environment]::NewLine)) {
        $content += [Environment]::NewLine
    }
    if (-not [string]::IsNullOrWhiteSpace($content)) {
        $content += [Environment]::NewLine
    }
    $content += $block.TrimEnd() + [Environment]::NewLine
}

Set-Content -Path $ConfigPath -Value $content -Encoding UTF8
Write-Host "Registered AteaTimeMcp in $ConfigPath"
Write-Host "Restart Codex before using the atea_time MCP server."

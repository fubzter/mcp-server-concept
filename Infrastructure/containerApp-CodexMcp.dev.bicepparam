using 'containerApp.bicep'

param imageName = 'codexmcp'
param appName = 'codexmcp'
param acrName = 'fubztermcpacr'
param environmentName = 'mymcpdev'
param resourceGroupName = 'rg-mymcpdev'
param keyVaultSecrets = [
  {
    key: 'codexmcpclientid' // Must be lowercase - used in secretRef
    value: 'CodexMcpClientId' // PascalCase - actual Key Vault secret name
  }
  {
    key: 'codexmcpclientsecret' // Must be lowercase - used in secretRef
    value: 'CodexMcpClientSecret' // PascalCase - actual Key Vault secret name
  }
]
param environment = [
  {
    name: 'EntraIdAuth__TenantId'
    value: 'b3f0b16b-81f9-4c36-a9ba-2b7fc139f0cb'
  }
  {
    name: 'EntraIdAuth__ClientId'
    secretRef: 'codexmcpclientid'
  }
  {
    name: 'EntraIdAuth__ClientSecret'
    secretRef: 'codexmcpclientsecret'
  }
  {
    name: 'EntraIdAuth__PublicUrl'
    value: 'https://codexmcp.orangehill-a3d65116.westeurope.azurecontainerapps.io'
  }
  {
    name: 'IsTransportStateless'
    value: 'true'
  }
  // Application Insights connection string is automatically added by the template
]

using 'containerApp.bicep'

param imageName = 'fortidspensionistmcp'
param appName = 'fortidspensionistmcp'
param acrName = 'fubztermcpacr'
param environmentName = 'mymcpdev'
param resourceGroupName = 'rg-mymcpdev'
param keyVaultSecrets = [
  {
    key: 'fortidspensionistmcpclientid' // Must be lowercase - used in secretRef
    value: 'fortidspensionistmcpClientId' // PascalCase - actual Key Vault secret name
  }
  {
    key: 'fortidspensionistmcpclientsecret' // Must be lowercase - used in secretRef
    value: 'fortidspensionistmcpClientSecret' // PascalCase - actual Key Vault secret name
  }
  {
    key: 'fortidspensionistmcptenantid' // Must be lowercase - used in secretRef
    value: 'fortidspensionistmcpTenantId' // PascalCase - actual Key Vault secret name
  }
]
param environment = [
  {
    name: 'EntraIdAuth__TenantId'
    secretRef: 'fortidspensionistmcptenantid'
  }
  {
    name: 'EntraIdAuth__ClientId'
    secretRef: 'fortidspensionistmcpclientid'
  }
  {
    name: 'EntraIdAuth__ClientSecret'
    secretRef: 'fortidspensionistmcpclientsecret'
  }
  {
    name: 'EntraIdAuth__PublicUrl'
    value: 'https://fortidspensionistmcp.orangehill-a3d65116.westeurope.azurecontainerapps.io'
  }
  {
    name: 'fortidspensionistmcpApi__BaseUrl'
    value: 'https://admin.opendata.dk'
  }
  {
    name: 'IsTransportStateless'
    value: 'true'
  }
  // Application Insights connection string is automatically added by the template
]

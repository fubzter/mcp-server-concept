using 'containerApp.bicep'

param imageName = 'tilbudsdata'
param appName = 'tilbudsdata'
param acrName = 'fubztermcpacr'
param environmentName = 'mymcpdev'
param resourceGroupName = 'rg-mymcpdev'
param keyVaultSecrets = [
  {
    key: 'tilbudsdataapikey' // Must be lowercase - used in secretRef
    value: 'TilbudsdataApiKey' // PascalCase - actual Key Vault secret name
  }
  {
    key: 'tilbudsdatauserid' // Must be lowercase - used in secretRef
    value: 'TilbudsdataUserId' // PascalCase - actual Key Vault secret name
  }
  {
    key: 'tilbudsdataclientid' // Must be lowercase - used in secretRef
    value: 'TilbudsdataClientId' // PascalCase - actual Key Vault secret name
  }
  {
    key: 'tilbudsdataclientsecret' // Must be lowercase - used in secretRef
    value: 'TilbudsdataClientSecret' // PascalCase - actual Key Vault secret name
  }
]
param environment = [
  {
    name: 'TilbudsdataApi__ApiKey'
    secretRef: 'tilbudsdataapikey'
  }
  {
    name: 'TilbudsdataApi__UserId'
    secretRef: 'tilbudsdatauserid'
  }
  {
    name: 'TilbudsdataApi__BaseUrl'
    value: 'https://api.tilbudsdata.dk'
  }
  {
    name: 'EntraIdAuth__TenantId'
    value: 'b3f0b16b-81f9-4c36-a9ba-2b7fc139f0cb'
  }
  {
    name: 'EntraIdAuth__ClientId'
    secretRef: 'tilbudsdataclientid'
  }
  {
    name: 'EntraIdAuth__ClientSecret'
    secretRef: 'tilbudsdataclientsecret'
  }
  {
    name: 'EntraIdAuth__PublicUrl'
    value: 'https://tilbudsdata.orangehill-a3d65116.westeurope.azurecontainerapps.io'
  }
  {
    name: 'IsTransportStateless'
    value: 'true'
  }
  // Application Insights connection string is automatically added by the template
]

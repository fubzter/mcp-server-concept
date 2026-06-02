using 'main.bicep'

// Shared registry — keep acrName and acrResourceGroupName identical to dev.bicepparam
param acrName             = 'fubztermcpacr'
param acrResourceGroupName = 'rg-fubztermcpacr'

// Prod-environment resources
param containerAppsEnvName = 'mymcpprod'
param keyVaultName        = 'mymcpprod'
param logAnalyticsName    = 'mymcpprod'
param location            = 'westeurope'
param resourceGroupName   = 'rg-mymcpprod'
param storageAccountName  = 'stmymcpprod'

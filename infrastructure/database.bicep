param location string
param vnetName string
param resourceGroupName string

param maxSizeInBytes int

resource sqlServerResource 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: 'snek-sql-server'
  location: location
  properties: {
    administratorLogin: 'serveradmin'
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Disabled'
    restrictOutboundNetworkAccess: 'Disabled'
  }
}


resource sqlServerConnectionPolicies 'Microsoft.Sql/servers/connectionPolicies@2023-08-01-preview' = {
  parent: sqlServerResource
  name: 'default'
  properties: {
    connectionType: 'Default'
  }
}


resource sqlServerDatabaseResource 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: sqlServerResource
  name: 'snek-sql-db'
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
    capacity: 10
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: maxSizeInBytes
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
    readScale: 'Disabled'
    requestedBackupStorageRedundancy: 'Zone'
    isLedgerOn: false
    availabilityZone: 'NoPreference'
  }
}

resource vnet 'Microsoft.Network/virtualNetworks@2024-01-01' existing = {
  name: vnetName
  scope: resourceGroup(resourceGroupName)
}

resource sqlServerVnetRule 'Microsoft.Sql/servers/virtualNetworkRules@2023-08-01-preview' = {
  parent: sqlServerResource
  name: 'allow-app-to-reach-db-server'
  properties: {
    virtualNetworkSubnetId: vnet.id
    ignoreMissingVnetServiceEndpoint: false
  }
}

output sqlServerId string = sqlServerResource.id
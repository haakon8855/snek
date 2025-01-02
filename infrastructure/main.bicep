targetScope = 'subscription'

param rgName string = 'snek'
param location string = 'norwayeast'

resource resourceGroup 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: rgName
  location: location
}

module vnet 'vnet.bicep' = {
  name: 'vnet'
  scope: resourceGroup
  params: {
    location: location
  }
}

module appService 'app-service.bicep' = {
  name: 'appService'
  scope: resourceGroup
  params: {
    location: location
  }
}

module database 'database.bicep' = {
  name: 'database'
  scope: resourceGroup
  params: {
    maxSizeInBytes: 2147483648
    resourceGroupName: resourceGroup.name
    vnetName: vnet.name
    location: location
  }
}

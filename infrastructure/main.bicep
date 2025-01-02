targetScope = 'subscription'

param rgName string = 'snek'
param location string = 'norwayeast'

resource resourceGroup 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: rgName
  location: location
}

module vnet 'vnet.bicep' = {
  name: 'snek-vnet'
  scope: resourceGroup
  params: {
    location: location
  }
}

module appService 'app-service.bicep' = {
  name: 'snek-game'
  scope: resourceGroup
  params: {
    location: location
  }
}

module database 'database.bicep' = {
  name: 'snek-sql'
  scope: resourceGroup
  params: {
    maxSizeInBytes: 2147483648
    resourceGroupName: resourceGroup.name
    vnetName: vnet.name
    location: location
  }
}

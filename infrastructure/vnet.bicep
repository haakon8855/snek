param location string

var subnetName = 'default'

resource vnet 'Microsoft.Network/virtualNetworks@2024-01-01' = {
  name: 'snek-vnet'
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.0.0.0/16'
      ]
    }
    subnets: [
      {
        name: subnetName
        properties: {
          addressPrefix: '10.0.0.0/24'
          serviceEndpoints: [
            {
              service: 'Microsoft.Sql'
              locations: [
                location
              ]
            }
          ]
          delegations: [
            {
              name: 'delegation'
              properties: {
                serviceName: 'Microsoft.Web/serverfarms'
              }
            }
          ]
          privateEndpointNetworkPolicies: 'Disabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
          defaultOutboundAccess: true
        }
      }
    ]
    virtualNetworkPeerings: []
    enableDdosProtection: false
  }
}

output subnetName string = subnetName
output vnetName string = vnet.name
output subnetId string = '${vnet.id}/subnets/${subnetName}'

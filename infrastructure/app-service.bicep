param location string = resourceGroup().location

var appServicePlanName = 'snek-appsvc-plan'
var appName = 'snek-app'

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'B1'
    tier: 'Basic'
    size: 'B1'
    family: 'B'
    capacity: 1
  }
  kind: 'app'
  properties: {
    perSiteScaling: false
    elasticScaleEnabled: false
    maximumElasticWorkerCount: 1
    isSpot: false
    reserved: false
    isXenon: false
    hyperV: false
    targetWorkerCount: 0
    targetWorkerSizeId: 0
    zoneRedundant: false
  }
}

resource appServiceResource 'Microsoft.Web/sites@2023-12-01' = {
  name: appName
  location: location
  kind: 'app'
  properties: {
    enabled: true
    hostNameSslStates: [
      {
        name: '${appName}.azurewebsites.net'
        sslState: 'Disabled'
        hostType: 'Standard'
      }
      {
        name: '${appName}.scm.azurewebsites.net'
        sslState: 'Disabled'
        hostType: 'Repository'
      }
    ]
    serverFarmId: appServicePlan.id
    reserved: false
    isXenon: false
    hyperV: false
    vnetRouteAllEnabled: true
    vnetImagePullEnabled: false
    vnetContentShareEnabled: false
    siteConfig: {
      numberOfWorkers: 1
      acrUseManagedIdentityCreds: false
      alwaysOn: false
      http20Enabled: false
      functionAppScaleLimit: 0
      minimumElasticInstanceCount: 1
    }
    scmSiteAlsoStopped: false
    clientAffinityEnabled: true
    clientCertEnabled: false
    clientCertMode: 'Required'
    hostNamesDisabled: false
    vnetBackupRestoreEnabled: false
    customDomainVerificationId: 'DAA0094679E7C0E6BC77DB9232E40609723E20E5610CED86D12AA4E76D374025'
    containerSize: 0
    dailyMemoryTimeQuota: 0
    httpsOnly: true
    redundancyMode: 'None'
    publicNetworkAccess: 'Enabled'
    storageAccountRequired: false
    keyVaultReferenceIdentity: 'SystemAssigned'
  }
}

resource sites_snek_game_ftp 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2023-12-01' = {
  parent: appServiceResource
  name: 'ftp'
  properties: {
    allow: true
  }
}

resource sites_snek_game_scm 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2023-12-01' = {
  parent: appServiceResource
  name: 'scm'
  properties: {
    allow: true
  }
}

resource appServiceConfig 'Microsoft.Web/sites/config@2023-12-01' = {
  parent: appServiceResource
  name: 'web'
  properties: {
    numberOfWorkers: 1
    // defaultDocuments: [
    //   'Default.htm'
    //   'Default.html'
    //   'Default.asp'
    //   'index.htm'
    //   'index.html'
    //   'iisstart.htm'
    //   'default.aspx'
    //   'index.php'
    //   'hostingstart.html'
    // ]
    netFrameworkVersion: 'v9.0'
    requestTracingEnabled: false
    remoteDebuggingEnabled: false
    remoteDebuggingVersion: 'VS2022'
    httpLoggingEnabled: false
    acrUseManagedIdentityCreds: false
    logsDirectorySizeLimit: 35
    detailedErrorLoggingEnabled: false
    publishingUsername: '$snek-app'
    scmType: 'GitHubAction'
    use32BitWorkerProcess: true
    webSocketsEnabled: true
    alwaysOn: false
    managedPipelineMode: 'Integrated'
    // virtualApplications: [
    //   {
    //     virtualPath: '/'
    //     physicalPath: 'site\\wwwroot'
    //     preloadEnabled: false
    //   }
    // ]
    loadBalancing: 'LeastRequests'
    // experiments: {
    //   rampUpRules: []
    // }
    autoHealEnabled: false
    vnetRouteAllEnabled: true
    vnetPrivatePortsCount: 0
    publicNetworkAccess: 'Enabled'
    localMySqlEnabled: false
    ipSecurityRestrictions: []
    // ipSecurityRestrictions: [
    //   {
    //     ipAddress: 'Any'
    //     action: 'Allow'
    //     priority: 2147483647
    //     name: 'Allow all'
    //     description: 'Allow all access'
    //   }
    // ]
    ipSecurityRestrictionsDefaultAction: 'Allow'
    // scmIpSecurityRestrictions: [
    //   {
    //     ipAddress: 'Any'
    //     action: 'Allow'
    //     priority: 2147483647
    //     name: 'Allow all'
    //     description: 'Allow all access'
    //   }
    // ]
    scmIpSecurityRestrictionsUseMain: false
    http20Enabled: false
    minTlsVersion: '1.2'
    scmMinTlsVersion: '1.2'
    ftpsState: 'FtpsOnly'
    preWarmedInstanceCount: 0
    elasticWebAppScaleLimit: 0
    functionsRuntimeScaleMonitoringEnabled: false
    minimumElasticInstanceCount: 1
    azureStorageAccounts: {}
  }
}

resource appServiceHostnameBinding 'Microsoft.Web/sites/hostNameBindings@2023-12-01' = {
  parent: appServiceResource
  name: '${appName}.azurewebsites.net'
  properties: {
    siteName: 'snek-app'
    hostNameType: 'Verified'
  }
}

// output appServiceName string = appName
// output appServicePlanName string = appServicePlanName
// output appFdqn string = appServiceResource.properties.defaultHostName
// output appManagedIdentityPrincipalId string = appServiceResource.identity.principalId

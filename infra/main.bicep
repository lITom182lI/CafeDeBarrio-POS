param location string = 'eastus'
param staticWebAppsLocation string = 'eastus2'   // Static Web Apps tiene regiones limitadas
param sqlLocation string = 'centralus'
param projectName string = 'cafebarrio'
param sqlAdminLogin string
@secure()
param sqlAdminPassword string
@secure()
param jwtKey string
param containerImage string = 'ghcr.io/litom182li/cafebarrio-api:latest'
param corsAllowedOrigins string = ''   // se rellena post-deploy con los outputs de las SWA

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: '${projectName}-logs'
  location: location
  properties: { sku: { name: 'PerGB2018' }, retentionInDays: 30 }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${projectName}-insights'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

resource sqlServer 'Microsoft.Sql/servers@2022-11-01-preview' = {
  name: '${projectName}-sql'
  location: sqlLocation
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    minimalTlsVersion: '1.2'
  }
}

resource sqlFirewallAzureServices 'Microsoft.Sql/servers/firewallRules@2022-11-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: { startIpAddress: '0.0.0.0', endIpAddress: '0.0.0.0' }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2022-11-01-preview' = {
  parent: sqlServer
  name: '${projectName}db'
  location: sqlLocation
  sku: { name: 'Basic', tier: 'Basic', capacity: 5 }
}

resource containerEnv 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: '${projectName}-env'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: '${projectName}-api'
  location: location
  properties: {
    managedEnvironmentId: containerEnv.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
        transport: 'http'
        corsPolicy: {
          allowedOrigins: ['*']   // CORS se gestiona en el código .NET, no aquí
          allowedHeaders: ['*']
          allowedMethods: ['*']
        }
      }
      secrets: [
        { name: 'sql-connection', value: 'Server=tcp:${sqlServer.name}.database.windows.net,1433;Database=${sqlDatabase.name};User Id=${sqlAdminLogin};Password=${sqlAdminPassword};Encrypt=True;Connection Timeout=30;' }
        { name: 'jwt-key', value: jwtKey }
      ]
    }
    template: {
      containers: [{
        name: 'api'
        image: containerImage
        resources: { cpu: json('0.5'), memory: '1Gi' }
        env: [
          { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
          { name: 'ConnectionStrings__DefaultConnection', secretRef: 'sql-connection' }
          { name: 'Jwt__Key', secretRef: 'jwt-key' }
          { name: 'Jwt__Issuer', value: 'CafeDeBarrio' }
          { name: 'Jwt__Audience', value: 'CafeDeBarrioClients' }
          { name: 'Cors__AllowedOrigin', value: corsAllowedOrigins }
          { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsights.properties.ConnectionString }
          { name: 'OTEL_EXPORTER_OTLP_ENDPOINT', value: '' }
        ]
      }]
      scale: { minReplicas: 0, maxReplicas: 3 }
    }
  }
}

resource staticWebAppPos 'Microsoft.Web/staticSites@2023-01-01' = {
  name: '${projectName}-pos'
  location: staticWebAppsLocation   // Static Web Apps no disponible en todas las regiones
  sku: { name: 'Free', tier: 'Free' }
  properties: {}
}

resource staticWebAppDashboard 'Microsoft.Web/staticSites@2023-01-01' = {
  name: '${projectName}-dashboard'
  location: staticWebAppsLocation
  sku: { name: 'Free', tier: 'Free' }
  properties: {}
}

output apiUrl string = 'https://${containerApp.properties.configuration.ingress.fqdn}'
output posUrl string = 'https://${staticWebAppPos.properties.defaultHostname}'
output dashboardUrl string = 'https://${staticWebAppDashboard.properties.defaultHostname}'
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output appInsightsConnectionString string = appInsights.properties.ConnectionString

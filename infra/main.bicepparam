using './main.bicep'

param location = 'eastus'
param staticWebAppsLocation = 'eastus2'
param projectName = 'cafebarrio'
param sqlAdminLogin = 'cafeadmin'
param sqlAdminPassword = readEnvironmentVariable('SQL_ADMIN_PASSWORD')
param jwtKey = readEnvironmentVariable('JWT_KEY_PROD')
param containerImage = 'ghcr.io/litom182li/cafebarrio-api:latest'
param corsAllowedOrigins = readEnvironmentVariable('CORS_ALLOWED_ORIGINS', '')

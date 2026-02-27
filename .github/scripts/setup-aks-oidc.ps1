param(
  [Parameter(Mandatory = $true)]
  [string]$SubscriptionId,

  [Parameter(Mandatory = $true)]
  [string]$TenantId,

  [Parameter(Mandatory = $true)]
  [string]$RepoOwner,

  [Parameter(Mandatory = $true)]
  [string]$RepoName,

  [Parameter(Mandatory = $true)]
  [string]$StagingResourceGroup,

  [Parameter(Mandatory = $true)]
  [string]$StagingCluster,

  [Parameter(Mandatory = $true)]
  [string]$ProdResourceGroup,

  [Parameter(Mandatory = $true)]
  [string]$ProdCluster
)

$ErrorActionPreference = "Stop"

if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
  throw "Azure CLI (az) is not installed. Install it first: https://learn.microsoft.com/cli/azure/install-azure-cli"
}

$subjectStaging = "repo:$RepoOwner/$RepoName:ref:refs/heads/staging"
$subjectProd = "repo:$RepoOwner/$RepoName:ref:refs/heads/main"
$appName = "gha-$RepoName-aks-oidc"

Write-Host "Logging in to Azure..." -ForegroundColor Cyan
az login --tenant $TenantId | Out-Null
az account set --subscription $SubscriptionId

Write-Host "Creating or reusing App Registration: $appName" -ForegroundColor Cyan
$appId = az ad app list --display-name $appName --query "[0].appId" -o tsv
if ([string]::IsNullOrWhiteSpace($appId)) {
  $appId = az ad app create --display-name $appName --query appId -o tsv
}

$objectId = az ad app show --id $appId --query id -o tsv

Write-Host "Creating Service Principal if missing..." -ForegroundColor Cyan
$spId = az ad sp list --filter "appId eq '$appId'" --query "[0].id" -o tsv
if ([string]::IsNullOrWhiteSpace($spId)) {
  az ad sp create --id $appId | Out-Null
}

Write-Host "Adding federated credentials for staging/main..." -ForegroundColor Cyan
$stagingCred = @{
  name = "github-staging"
  issuer = "https://token.actions.githubusercontent.com"
  subject = $subjectStaging
  audiences = @("api://AzureADTokenExchange")
} | ConvertTo-Json -Depth 5

$prodCred = @{
  name = "github-main"
  issuer = "https://token.actions.githubusercontent.com"
  subject = $subjectProd
  audiences = @("api://AzureADTokenExchange")
} | ConvertTo-Json -Depth 5

$stagingCredFile = New-TemporaryFile
$prodCredFile = New-TemporaryFile
$stagingCred | Set-Content -Path $stagingCredFile -Encoding UTF8
$prodCred | Set-Content -Path $prodCredFile -Encoding UTF8

try {
  az ad app federated-credential create --id $objectId --parameters "@$stagingCredFile" | Out-Null
} catch {
  Write-Host "Staging federated credential may already exist. Continuing..." -ForegroundColor Yellow
}

try {
  az ad app federated-credential create --id $objectId --parameters "@$prodCredFile" | Out-Null
} catch {
  Write-Host "Prod federated credential may already exist. Continuing..." -ForegroundColor Yellow
}

Remove-Item $stagingCredFile, $prodCredFile -Force -ErrorAction SilentlyContinue

Write-Host "Assigning AKS Cluster User role..." -ForegroundColor Cyan
$stagingScope = "/subscriptions/$SubscriptionId/resourceGroups/$StagingResourceGroup"
$prodScope = "/subscriptions/$SubscriptionId/resourceGroups/$ProdResourceGroup"

try {
  az role assignment create --assignee $appId --role "Azure Kubernetes Service Cluster User Role" --scope $stagingScope | Out-Null
} catch {
  Write-Host "Role assignment for staging may already exist. Continuing..." -ForegroundColor Yellow
}

try {
  az role assignment create --assignee $appId --role "Azure Kubernetes Service Cluster User Role" --scope $prodScope | Out-Null
} catch {
  Write-Host "Role assignment for prod may already exist. Continuing..." -ForegroundColor Yellow
}

Write-Host "\nSet these GitHub repository secrets:" -ForegroundColor Green
Write-Host "AZURE_CLIENT_ID=$appId"
Write-Host "AZURE_TENANT_ID=$TenantId"
Write-Host "AZURE_SUBSCRIPTION_ID=$SubscriptionId"

Write-Host "\nSet these GitHub repository variables:" -ForegroundColor Green
Write-Host "AKS_RESOURCE_GROUP_STAGING=$StagingResourceGroup"
Write-Host "AKS_CLUSTER_STAGING=$StagingCluster"
Write-Host "AKS_RESOURCE_GROUP_PROD=$ProdResourceGroup"
Write-Host "AKS_CLUSTER_PROD=$ProdCluster"

Write-Host "\nDone." -ForegroundColor Green

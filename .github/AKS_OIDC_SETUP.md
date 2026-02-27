# AKS + GitHub OIDC setup

This repo is configured to deploy with GitHub Actions to AKS using OIDC:

- Staging workflow: `.github/workflows/deploy-staging.yml` (branch: `staging`)
- Production workflow: `.github/workflows/deploy-prod.yml` (branch: `main` / `release/*`)

## 1) Create Azure App Registration for GitHub OIDC

Create one app registration and grant AKS access (Azure RBAC) to your AKS resource groups.

Add Federated credentials for GitHub repo:

- Staging subject: `repo:<owner>/<repo>:ref:refs/heads/staging`
- Production subject: `repo:<owner>/<repo>:ref:refs/heads/main`

## 2) Add GitHub repository secrets

- `AZURE_CLIENT_ID`
- `AZURE_TENANT_ID`
- `AZURE_SUBSCRIPTION_ID`

## 3) Add GitHub repository variables

- `AKS_RESOURCE_GROUP_STAGING`
- `AKS_CLUSTER_STAGING`
- `AKS_RESOURCE_GROUP_PROD`
- `AKS_CLUSTER_PROD`

## 4) Domain DNS for julius-mark-genato-ii.com

Point both records to your ingress public IP:

- `A  julius-mark-genato-ii.com -> <ingress-ip>`
- `A  www.julius-mark-genato-ii.com -> <ingress-ip>`

## 5) cert-manager email

Set your email in `AdminTool/k8s/cluster-issuer.yaml`.

## 6) Trigger deployments

- Push to `staging` for staging deployment
- Push to `main` for production deployment

## Region note

For Azure, the nearest commonly used region for the Philippines is usually `southeastasia`.

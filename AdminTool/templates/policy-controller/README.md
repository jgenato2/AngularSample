# Policy Controller Template

Generates a Presentation controller that follows the project pattern:

- Inherits from `ApiControllerBase`
- Uses policy attributes (`[AdminOnly]`, `[SelfOrAdmin]`)
- Delegates orchestration to an Application service
- Uses `FromResult(...)` for consistent error mapping

## Install locally

From repository root:

```powershell
dotnet new install .\AdminTool\templates\policy-controller
```

## Generate a controller

```powershell
dotnet new policy-controller -n Users -o .\AdminTool\server\Presentation\Controllers --resource users --serviceInterface IUsersApplicationService
```

## Notes

- The generated file is a scaffold and uses generic request types (`object`) in create/update methods.
- Replace request types and service method signatures with feature-specific contracts.

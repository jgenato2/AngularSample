# Controller Pattern Template

Use this pattern for all new endpoints in `Presentation/Controllers`:

1. Inherit from `ApiControllerBase`.
2. Use policy attributes instead of inline authorization checks:
   - `[AdminOnly]`
   - `[SelfOrAdmin]`
3. Call Application services only (`I...ApplicationService`).
4. Return responses through `FromResult(...)`.

Skeleton:

```csharp
[ApiController]
[Authorize]
[Route("api/[resource]")]
public class SampleController(ISampleApplicationService service) : ApiControllerBase
{
    [HttpGet]
    [AdminOnly]
    public IActionResult List()
    {
        var result = service.List();
        return Ok(new { items = result });
    }

    [HttpGet("{id}")]
    [SelfOrAdmin]
    public IActionResult GetById(string id)
    {
        var result = service.GetById(id);
        return FromResult(result, item => Ok(new { item }));
    }
}
```

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Presentation.Authorization;

namespace Server.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/[resource]")]
public class SampleController(serviceInterface service) : ApiControllerBase
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

    [HttpPost]
    [AdminOnly]
    public IActionResult Create([FromBody] object request)
    {
        var result = service.Create(request);
        return FromResult(result, item => Created($"/api/[resource]/{item.Id}", new { item }));
    }

    [HttpPut("{id}")]
    [SelfOrAdmin]
    public IActionResult Update(string id, [FromBody] object request)
    {
        var result = service.Update(id, request, User.IsInRole("admin"));
        return FromResult(result, item => Ok(new { item }));
    }

    [HttpDelete("{id}")]
    [AdminOnly]
    public IActionResult Delete(string id)
    {
        var result = service.Delete(id);
        return FromResult(result, _ => Ok(new { ok = true }));
    }
}

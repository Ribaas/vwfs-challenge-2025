using Microsoft.AspNetCore.Mvc;
using Frete.Application.DTOs;
using Frete.Application.Interfaces;
using Frete.Application.Services;

namespace Frete.Api.Controllers;

[ApiController]
[Route("api/pedidos")]
public sealed class PedidosController(IPedidoService service, ILogger<PedidosController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PedidoResponse>>> GetAll(CancellationToken ct)
        => Ok(await service.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PedidoResponse>> GetById(Guid id, CancellationToken ct)
    {
        var pedido = await service.GetByIdAsync(id, ct);
        return pedido is null ? NotFound() : Ok(pedido);
    }

    [HttpPost]
    public async Task<ActionResult<PedidoResponse>> Create([FromBody] PedidoCreateRequest req, CancellationToken ct)
    {
        var result = await service.CreateAsync(req, ct);
        logger.LogInformation("Pedido {Id} criado", result.Id);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PedidoResponse>> Update(Guid id, [FromBody] PedidoUpdateRequest req, CancellationToken ct)
    {
        var result = await service.UpdateAsync(id, req, ct);
        logger.LogInformation("Pedido {Id} atualizado", result.Id);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        logger.LogInformation("Pedido {Id} removido", id);
        return NoContent();
    }
}
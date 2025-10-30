using Microsoft.AspNetCore.Mvc;
using Frete.Application.DTOs;
using Frete.Application.Interfaces;

namespace Frete.Api.Controllers;

[ApiController]
[Route("api/pedidos")]
[Produces("application/json")]
public sealed class PedidosController(IPedidoService service, ILogger<PedidosController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PedidoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PedidoResponse>>> GetAll(CancellationToken ct)
    {
        var pedidos = await service.GetAllAsync(ct);
        return Ok(pedidos);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PedidoResponse>> GetById(Guid id, CancellationToken ct)
    {
        var pedido = await service.GetByIdAsync(id, ct);

        if (pedido is null)
            return NotFound(ErrorResponse.NotFound($"Pedido com ID '{id}' não foi encontrado."));

        return Ok(pedido);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PedidoResponse>> Create([FromBody] PedidoCreateRequest req, CancellationToken ct)
    {
        var result = await service.CreateAsync(req, ct);
        logger.LogInformation("Pedido {Id} criado com sucesso", result.Id);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PedidoResponse>> Update(Guid id, [FromBody] PedidoUpdateRequest req, CancellationToken ct)
    {
        var result = await service.UpdateAsync(id, req, ct);
        logger.LogInformation("Pedido {Id} atualizado com sucesso", result.Id);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        logger.LogInformation("Pedido {Id} removido com sucesso", id);
        return NoContent();
    }
}
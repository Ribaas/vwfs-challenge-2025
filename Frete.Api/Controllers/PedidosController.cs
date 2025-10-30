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
        logger.LogInformation("Requisição para listar todos os pedidos");
        var pedidos = await service.GetAllAsync(ct);
        logger.LogInformation("Listagem de pedidos concluída. Total: {Count} pedido(s)", pedidos.Count());
        return Ok(pedidos);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PedidoResponse>> GetById(Guid id, CancellationToken ct)
    {
        logger.LogInformation("Requisição para buscar pedido {PedidoId}", id);
        var pedido = await service.GetByIdAsync(id, ct);

        if (pedido is null)
        {
            logger.LogWarning("Pedido {PedidoId} não encontrado", id);
            return NotFound(ErrorResponse.NotFound($"Pedido com ID '{id}' não foi encontrado."));
        }

        logger.LogInformation("Pedido {PedidoId} retornado com sucesso", id);
        return Ok(pedido);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PedidoResponse>> Create([FromBody] PedidoCreateRequest req, CancellationToken ct)
    {
        logger.LogInformation("Requisição para criar pedido - Cliente: {ClientId}, Modalidade: {Modalidade}",
            req.ClientId, req.Modalidade);

        var result = await service.CreateAsync(req, ct);

        logger.LogInformation("Pedido {PedidoId} criado com sucesso - Valor: {ValorFrete}",
            result.Id, result.ValorFrete);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PedidoResponse>> Update(Guid id, [FromBody] PedidoUpdateRequest req, CancellationToken ct)
    {
        logger.LogInformation("Requisição para atualizar pedido {PedidoId} - Nova Modalidade: {Modalidade}",
            id, req.Modalidade);

        var result = await service.UpdateAsync(id, req, ct);

        logger.LogInformation("Pedido {PedidoId} atualizado com sucesso - Novo valor: {ValorFrete}",
            result.Id, result.ValorFrete);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        logger.LogInformation("Requisição para deletar pedido {PedidoId}", id);
        await service.DeleteAsync(id, ct);
        logger.LogInformation("Pedido {PedidoId} removido com sucesso", id);
        return NoContent();
    }
}
using Microsoft.AspNetCore.Mvc;
using Frete.Application.DTOs;
using Frete.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace Frete.Api.Controllers;

[ApiController]
[Route("api/pedidos")]
[Produces("application/json")]
[SwaggerTag("Gerenciamento de pedidos e cálculo de frete com diferentes modalidades de entrega")]
public sealed class PedidosController(IPedidoService service, ILogger<PedidosController> logger) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(
        Summary = "Lista todos os pedidos",
        Description = "Retorna uma lista com todos os pedidos cadastrados no sistema, incluindo informações de frete calculado.",
        OperationId = "GetAllPedidos",
        Tags = new[] { "Pedidos" }
    )]
    [SwaggerResponse(200, "Lista de pedidos retornada com sucesso", typeof(IReadOnlyList<PedidoResponse>))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ErrorResponse))]
    [ProducesResponseType(typeof(IReadOnlyList<PedidoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PedidoResponse>>> GetAll(CancellationToken ct)
    {
        logger.LogInformation("Requisição para listar todos os pedidos");
        var pedidos = await service.GetAllAsync(ct);
        logger.LogInformation("Listagem de pedidos concluída. Total: {Count} pedido(s)", pedidos.Count());
        return Ok(pedidos);
    }

    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        Summary = "Busca um pedido por ID",
        Description = "Retorna os detalhes completos de um pedido específico, incluindo o valor do frete calculado de acordo com a modalidade escolhida.",
        OperationId = "GetPedidoById",
        Tags = new[] { "Pedidos" }
    )]
    [SwaggerResponse(200, "Pedido encontrado com sucesso", typeof(PedidoResponse))]
    [SwaggerResponse(400, "ID do pedido inválido", typeof(ErrorResponse))]
    [SwaggerResponse(404, "Pedido não encontrado", typeof(ErrorResponse))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ErrorResponse))]
    [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PedidoResponse>> GetById(
        [SwaggerParameter("ID único do pedido (GUID)", Required = true)] Guid id,
        CancellationToken ct)
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
    [SwaggerOperation(
        Summary = "Cria um novo pedido",
        Description = "Cria um novo pedido com cálculo automático do frete baseado na modalidade escolhida (Normal, Expressa ou Agendada). " +
                      "O sistema calculará o valor do frete considerando peso, distância e taxa fixa de acordo com as regras de negócio de cada modalidade.",
        OperationId = "CreatePedido",
        Tags = new[] { "Pedidos" }
    )]
    [SwaggerResponse(201, "Pedido criado com sucesso. O valor do frete foi calculado automaticamente.", typeof(PedidoResponse))]
    [SwaggerResponse(400, "Dados do pedido inválidos. Verifique os parâmetros enviados.", typeof(ErrorResponse))]
    [SwaggerResponse(409, "Pedido já existe no sistema", typeof(ErrorResponse))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ErrorResponse))]
    [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PedidoResponse>> Create(
        [FromBody, SwaggerRequestBody("Dados do pedido a ser criado", Required = true)] PedidoCreateRequest req,
        CancellationToken ct)
    {
        logger.LogInformation("Requisição para criar pedido - Cliente: {ClientId}, Modalidade: {Modalidade}",
            req.ClientId, req.Modalidade);

        var result = await service.CreateAsync(req, ct);

        logger.LogInformation("Pedido {PedidoId} criado com sucesso - Valor: {ValorFrete}",
            result.Id, result.ValorFrete);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(
        Summary = "Atualiza um pedido existente",
        Description = "Atualiza os dados de um pedido existente e recalcula o valor do frete automaticamente com base nos novos parâmetros fornecidos. " +
                      "Permite alteração da modalidade de entrega e recálculo completo do frete.",
        OperationId = "UpdatePedido",
        Tags = new[] { "Pedidos" }
    )]
    [SwaggerResponse(200, "Pedido atualizado com sucesso. O frete foi recalculado.", typeof(PedidoResponse))]
    [SwaggerResponse(400, "Dados do pedido inválidos", typeof(ErrorResponse))]
    [SwaggerResponse(404, "Pedido não encontrado", typeof(ErrorResponse))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ErrorResponse))]
    [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PedidoResponse>> Update(
        [SwaggerParameter("ID único do pedido a ser atualizado (GUID)", Required = true)] Guid id,
        [FromBody, SwaggerRequestBody("Novos dados do pedido", Required = true)] PedidoUpdateRequest req,
        CancellationToken ct)
    {
        logger.LogInformation("Requisição para atualizar pedido {PedidoId} - Nova Modalidade: {Modalidade}",
            id, req.Modalidade);

        var result = await service.UpdateAsync(id, req, ct);

        logger.LogInformation("Pedido {PedidoId} atualizado com sucesso - Novo valor: {ValorFrete}",
            result.Id, result.ValorFrete);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        Summary = "Remove um pedido",
        Description = "Remove permanentemente um pedido do sistema. Esta operação não pode ser desfeita.",
        OperationId = "DeletePedido",
        Tags = new[] { "Pedidos" }
    )]
    [SwaggerResponse(204, "Pedido removido com sucesso")]
    [SwaggerResponse(400, "ID do pedido inválido", typeof(ErrorResponse))]
    [SwaggerResponse(404, "Pedido não encontrado", typeof(ErrorResponse))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [SwaggerParameter("ID único do pedido a ser removido (GUID)", Required = true)] Guid id,
        CancellationToken ct)
    {
        logger.LogInformation("Requisição para deletar pedido {PedidoId}", id);
        await service.DeleteAsync(id, ct);
        logger.LogInformation("Pedido {PedidoId} removido com sucesso", id);
        return NoContent();
    }
}
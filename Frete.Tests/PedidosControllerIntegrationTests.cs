using System.Net;
using System.Net.Http.Json;
using Frete.Application.DTOs;
using Frete.Domain.Enums;
using FluentAssertions;
using Frete.Api;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Frete.Tests;

public class PedidosControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PedidosControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_ShouldCreatePedido_WhenRequestIsValid()
    {
        // Arrange
        var request = new PedidoCreateRequest(
            ClientId: Guid.NewGuid(),
            Modalidade: ModalidadeFrete.Normal,
            PesoKg: 10m,
            DistanciaKm: 20m,
            TaxaFixa: 5m
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/pedidos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var pedido = await response.Content.ReadFromJsonAsync<PedidoResponse>();
        pedido.Should().NotBeNull();
        pedido!.Id.Should().NotBeEmpty();
        pedido.ClientId.Should().Be(request.ClientId);
        pedido.Modalidade.Should().Be(request.Modalidade);
        pedido.ValorFrete.Should().BeGreaterThan(0);

        // Verify Location header
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/api/pedidos/{pedido.Id}");
    }

    [Fact]
    public async Task Create_ShouldCalculateCorrectFrete_ForNormalModalidade()
    {
        // Arrange
        var request = new PedidoCreateRequest(
            ClientId: Guid.NewGuid(),
            Modalidade: ModalidadeFrete.Normal,
            PesoKg: 5m,
            DistanciaKm: 10m,
            TaxaFixa: 2m
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/pedidos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var pedido = await response.Content.ReadFromJsonAsync<PedidoResponse>();

        // Normal = peso * 0.5 + distancia * 0.1 + taxaFixa
        var expectedFrete = (5m * 0.5m) + (10m * 0.1m) + 2m;
        pedido!.ValorFrete.Should().Be(expectedFrete);
    }

    [Fact]
    public async Task Create_ShouldCalculateCorrectFrete_ForExpressaModalidade()
    {
        // Arrange
        var request = new PedidoCreateRequest(
            ClientId: Guid.NewGuid(),
            Modalidade: ModalidadeFrete.Expressa,
            PesoKg: 5m,
            DistanciaKm: 10m,
            TaxaFixa: 5m
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/pedidos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var pedido = await response.Content.ReadFromJsonAsync<PedidoResponse>();

        // Expressa = peso * 0.5 + distancia * 1 + taxaFixa
        var expectedFrete = (5m * 0.5m) + (10m * 1m) + 5m;
        pedido!.ValorFrete.Should().Be(expectedFrete);
    }

    [Fact]
    public async Task Create_ShouldCalculateCorrectFrete_ForAgendadaModalidade()
    {
        // Arrange
        var request = new PedidoCreateRequest(
            ClientId: Guid.NewGuid(),
            Modalidade: ModalidadeFrete.Agendada,
            PesoKg: 5m,
            DistanciaKm: 10m,
            TaxaFixa: 10m
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/pedidos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var pedido = await response.Content.ReadFromJsonAsync<PedidoResponse>();

        // Agendada = peso * 0.5 + distancia * 0.5 + taxaFixa
        var expectedFrete = (5m * 0.5m) + (10m * 0.5m) + 10m;
        pedido!.ValorFrete.Should().Be(expectedFrete);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenPesoKgIsInvalid()
    {
        // Arrange
        var request = new PedidoCreateRequest(
            ClientId: Guid.NewGuid(),
            Modalidade: ModalidadeFrete.Normal,
            PesoKg: -1m, // Invalid
            DistanciaKm: 20m,
            TaxaFixa: 5m
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/pedidos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Status.Should().Be(400);
        error.Error.Should().Contain("peso");
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenDistanciaKmIsInvalid()
    {
        // Arrange
        var request = new PedidoCreateRequest(
            ClientId: Guid.NewGuid(),
            Modalidade: ModalidadeFrete.Normal,
            PesoKg: 10m,
            DistanciaKm: 0m, // Invalid
            TaxaFixa: 5m
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/pedidos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Status.Should().Be(400);
        error.Error.Should().Contain("distância");
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenTaxaFixaIsNegative()
    {
        // Arrange
        var request = new PedidoCreateRequest(
            ClientId: Guid.NewGuid(),
            Modalidade: ModalidadeFrete.Normal,
            PesoKg: 10m,
            DistanciaKm: 20m,
            TaxaFixa: -5m // Invalid
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/pedidos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Status.Should().Be(400);
        error.Error.Should().Contain("taxa fixa");
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenClientIdIsEmpty()
    {
        // Arrange
        var request = new PedidoCreateRequest(
            ClientId: Guid.Empty, // Invalid
            Modalidade: ModalidadeFrete.Normal,
            PesoKg: 10m,
            DistanciaKm: 20m,
            TaxaFixa: 5m
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/pedidos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Status.Should().Be(400);
    }

    [Fact]
    public async Task GetById_ShouldReturnPedido_WhenPedidoExists()
    {
        // Arrange - Create a pedido first
        var createRequest = new PedidoCreateRequest(
            ClientId: Guid.NewGuid(),
            Modalidade: ModalidadeFrete.Normal,
            PesoKg: 10m,
            DistanciaKm: 20m,
            TaxaFixa: 5m
        );
        var createResponse = await _client.PostAsJsonAsync("/api/pedidos", createRequest);
        var createdPedido = await createResponse.Content.ReadFromJsonAsync<PedidoResponse>();

        // Act
        var response = await _client.GetAsync($"/api/pedidos/{createdPedido!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pedido = await response.Content.ReadFromJsonAsync<PedidoResponse>();
        pedido.Should().NotBeNull();
        pedido!.Id.Should().Be(createdPedido.Id);
        pedido.ClientId.Should().Be(createdPedido.ClientId);
        pedido.Modalidade.Should().Be(createdPedido.Modalidade);
        pedido.ValorFrete.Should().Be(createdPedido.ValorFrete);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenPedidoDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/pedidos/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Status.Should().Be(404);
        error.Error.Should().Contain(nonExistentId.ToString());
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllPedidos_WhenMultiplePedidosExist()
    {
        // Arrange - Create multiple pedidos
        var request1 = new PedidoCreateRequest(Guid.NewGuid(), ModalidadeFrete.Normal, 10m, 20m, 5m);
        var request2 = new PedidoCreateRequest(Guid.NewGuid(), ModalidadeFrete.Expressa, 5m, 10m, 3m);
        var request3 = new PedidoCreateRequest(Guid.NewGuid(), ModalidadeFrete.Agendada, 7m, 15m, 8m);

        await _client.PostAsJsonAsync("/api/pedidos", request1);
        await _client.PostAsJsonAsync("/api/pedidos", request2);
        await _client.PostAsJsonAsync("/api/pedidos", request3);

        // Act
        var response = await _client.GetAsync("/api/pedidos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pedidos = await response.Content.ReadFromJsonAsync<List<PedidoResponse>>();
        pedidos.Should().NotBeNull();
        pedidos!.Count.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task Update_ShouldUpdatePedido_WhenRequestIsValid()
    {
        // Arrange - Create a pedido first
        var createRequest = new PedidoCreateRequest(
            ClientId: Guid.NewGuid(),
            Modalidade: ModalidadeFrete.Normal,
            PesoKg: 10m,
            DistanciaKm: 20m,
            TaxaFixa: 5m
        );
        var createResponse = await _client.PostAsJsonAsync("/api/pedidos", createRequest);
        var createdPedido = await createResponse.Content.ReadFromJsonAsync<PedidoResponse>();

        var updateRequest = new PedidoUpdateRequest(
            Modalidade: ModalidadeFrete.Expressa,
            PesoKg: 15m,
            DistanciaKm: 25m,
            TaxaFixa: 10m
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/pedidos/{createdPedido!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedPedido = await response.Content.ReadFromJsonAsync<PedidoResponse>();
        updatedPedido.Should().NotBeNull();
        updatedPedido!.Id.Should().Be(createdPedido.Id);
        updatedPedido.Modalidade.Should().Be(ModalidadeFrete.Expressa);

        // Expressa = peso * 0.5 + distancia * 1 + taxaFixa
        var expectedFrete = (15m * 0.5m) + (25m * 1m) + 10m;
        updatedPedido.ValorFrete.Should().Be(expectedFrete);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenPedidoDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateRequest = new PedidoUpdateRequest(
            Modalidade: ModalidadeFrete.Expressa,
            PesoKg: 15m,
            DistanciaKm: 25m,
            TaxaFixa: 10m
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/pedidos/{nonExistentId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Status.Should().Be(404);
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenParametersAreInvalid()
    {
        // Arrange - Create a pedido first
        var createRequest = new PedidoCreateRequest(
            ClientId: Guid.NewGuid(),
            Modalidade: ModalidadeFrete.Normal,
            PesoKg: 10m,
            DistanciaKm: 20m,
            TaxaFixa: 5m
        );
        var createResponse = await _client.PostAsJsonAsync("/api/pedidos", createRequest);
        var createdPedido = await createResponse.Content.ReadFromJsonAsync<PedidoResponse>();

        var updateRequest = new PedidoUpdateRequest(
            Modalidade: ModalidadeFrete.Expressa,
            PesoKg: -5m, // Invalid
            DistanciaKm: 25m,
            TaxaFixa: 10m
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/pedidos/{createdPedido!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_ShouldDeletePedido_WhenPedidoExists()
    {
        // Arrange - Create a pedido first
        var createRequest = new PedidoCreateRequest(
            ClientId: Guid.NewGuid(),
            Modalidade: ModalidadeFrete.Normal,
            PesoKg: 10m,
            DistanciaKm: 20m,
            TaxaFixa: 5m
        );
        var createResponse = await _client.PostAsJsonAsync("/api/pedidos", createRequest);
        var createdPedido = await createResponse.Content.ReadFromJsonAsync<PedidoResponse>();

        // Act
        var response = await _client.DeleteAsync($"/api/pedidos/{createdPedido!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify pedido is deleted
        var getResponse = await _client.GetAsync($"/api/pedidos/{createdPedido.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenPedidoDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/pedidos/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task FullCRUDWorkflow_ShouldWorkCorrectly()
    {
        // 1. Create
        var createRequest = new PedidoCreateRequest(
            ClientId: Guid.NewGuid(),
            Modalidade: ModalidadeFrete.Normal,
            PesoKg: 10m,
            DistanciaKm: 20m,
            TaxaFixa: 5m
        );
        var createResponse = await _client.PostAsJsonAsync("/api/pedidos", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdPedido = await createResponse.Content.ReadFromJsonAsync<PedidoResponse>();

        // 2. Get by ID
        var getResponse = await _client.GetAsync($"/api/pedidos/{createdPedido!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Update
        var updateRequest = new PedidoUpdateRequest(
            Modalidade: ModalidadeFrete.Expressa,
            PesoKg: 15m,
            DistanciaKm: 30m,
            TaxaFixa: 8m
        );
        var updateResponse = await _client.PutAsJsonAsync($"/api/pedidos/{createdPedido.Id}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedPedido = await updateResponse.Content.ReadFromJsonAsync<PedidoResponse>();
        updatedPedido!.Modalidade.Should().Be(ModalidadeFrete.Expressa);

        // 4. Get All (should include our pedido)
        var getAllResponse = await _client.GetAsync("/api/pedidos");
        getAllResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var allPedidos = await getAllResponse.Content.ReadFromJsonAsync<List<PedidoResponse>>();
        allPedidos.Should().Contain(p => p.Id == createdPedido.Id);

        // 5. Delete
        var deleteResponse = await _client.DeleteAsync($"/api/pedidos/{createdPedido.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 6. Verify deletion
        var verifyResponse = await _client.GetAsync($"/api/pedidos/{createdPedido.Id}");
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

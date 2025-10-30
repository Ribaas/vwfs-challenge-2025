# Sistema de Cálculo de Frete - Frete API

[![.NET](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-13.0-blue)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Ribaas_vwfs-challenge-2025&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Ribaas_vwfs-challenge-2025)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Ribaas_vwfs-challenge-2025&metric=coverage)](https://sonarcloud.io/summary/new_code?id=Ribaas_vwfs-challenge-2025)


## 👨‍💻 Autor

Desenvolvido por Leonardo Ribas.

---

## 🎯 Sobre o Projeto

O **Frete API** é um sistema de gerenciamento de pedidos com cálculo automático de frete baseado em diferentes modalidades de entrega. O projeto foi desenvolvido seguindo os princípios de **Clean Architecture**, **SOLID** e **TDD (Test-Driven Development)**, demonstrando boas práticas de engenharia de software em .NET.

### Modalidades de Frete

O sistema suporta três modalidades de entrega, cada uma com sua própria fórmula de cálculo:

- **Normal**: `(peso × 0.5) + (distância × 0.1) + taxa fixa`
- **Expressa**: `(peso × 0.5) + (distância × 1.0) + taxa fixa`
- **Agendada**: `(peso × 0.5) + (distância × 0.5) + taxa fixa`

---

## 🏗️ Arquitetura e Princípios

### Test-Driven Development (TDD)

O projeto foi construído seguindo a metodologia **TDD**, onde os testes são escritos **antes** da implementação do código. Esta abordagem garante:

#### 🔴 Red → 🟢 Green → 🔵 Refactor

1. **Red**: Escrever um teste que falha
2. **Green**: Implementar o código mínimo para passar no teste
3. **Refactor**: Melhorar o código mantendo os testes passando

#### Evidências de TDD no Projeto

**1. Cobertura Abrangente de Testes**
```csharp
// FreteParametrosTests.cs - Testes de Value Objects
[Theory]
[InlineData(10, 20, 5)]
[InlineData(0.5, 100, 0)]
public void Constructor_ShouldCreateValidFreteParametros(decimal pesoKg, decimal distanciaKm, decimal taxaFixa)
{
    var parametros = new FreteParametros(pesoKg, distanciaKm, taxaFixa);
    Assert.Equal(pesoKg, parametros.PesoKg);
}
```

**2. Testes em Todas as Camadas**
- ✅ **Domain Layer**: `PedidoTests.cs`, `FreteParametrosTests.cs`
- ✅ **Application Layer**: `PedidoServiceTests.cs`, `FreteStrategyTests.cs`
- ✅ **Infrastructure Layer**: `InMemoryPedidoRepositoryTests.cs`
- ✅ **API Layer**: `PedidosControllerIntegrationTests.cs`

**3. Testes de Integração End-to-End**
```csharp
[Fact]
public async Task FullCRUDWorkflow_ShouldWorkCorrectly()
{
    // Create → Read → Update → Delete
    // Testa o fluxo completo da aplicação
}
```

**4. Data-Driven Testing com Theory**
```csharp
[Theory]
[InlineData(ModalidadeFrete.Normal, 10, 20, 5)]
[InlineData(ModalidadeFrete.Expressa, 5, 5, 5)]
[InlineData(ModalidadeFrete.Agendada, 7, 10, 20)]
public async Task CreateAsync_ShouldCalculateFreteForDifferentModalidades(...)
```

**5. Testes de Casos Extremos e Validações**
```csharp
[Theory]
[InlineData(0)]
[InlineData(-1)]
public void Constructor_ShouldThrowException_WhenPesoKgIsInvalid(decimal peso)
{
    var exception = Assert.Throws<InvalidFreteParametrosException>(...);
}
```

---

### Princípios SOLID

#### 🔹 S - Single Responsibility Principle (SRP)

Cada classe tem uma única responsabilidade bem definida:

```csharp
// FreteParametros - Responsável APENAS por validar parâmetros de frete
public sealed record FreteParametros
{
    public FreteParametros(decimal pesoKg, decimal distanciaKm, decimal taxaFixa)
    {
        if (pesoKg <= 0)
            throw new InvalidFreteParametrosException("O peso deve ser maior que zero.");
        // ... validações
    }
}

// PedidoService - Responsável APENAS por orquestrar operações de pedido
public sealed class PedidoService : IPedidoService
{
    // Delega cálculo de frete para estratégias
    // Delega persistência para repositório
}
```

#### 🔹 O - Open/Closed Principle (OCP) ⭐

**O sistema está aberto para extensão, mas fechado para modificação.**

##### Implementação do Strategy Pattern para OCP

```csharp
// Interface base - FECHADA para modificação
public interface IFreteStrategy
{
    decimal CalcularFrete(FreteParametros parametros);
}

// ABERTO para extensão - Nova estratégia sem modificar código existente
public class NormalFreteStrategy : IFreteStrategy
{
    public decimal CalcularFrete(FreteParametros parametros)
        => parametros.PesoKg * 0.5m + parametros.DistanciaKm * 0.1m + parametros.TaxaFixa;
}

public class ExpressaFreteStrategy : IFreteStrategy
{
    public decimal CalcularFrete(FreteParametros parametros)
        => parametros.PesoKg * 0.5m + parametros.DistanciaKm * 1m + parametros.TaxaFixa;
}
```

##### Como Adicionar Nova Modalidade (ex: "Super Expressa")

**Passo 1**: Adicionar enum (única modificação necessária)
```csharp
public enum ModalidadeFrete
{
    Normal = 1,
    Expressa = 2,
    Agendada = 3,
    SuperExpressa = 4  // NOVA
}
```

**Passo 2**: Criar nova estratégia (extensão)
```csharp
public class SuperExpressaFreteStrategy : IFreteStrategy
{
    public decimal CalcularFrete(FreteParametros parametros)
        => parametros.PesoKg * 1.5m + parametros.DistanciaKm * 2m + parametros.TaxaFixa;
}
```

**Passo 3**: Registrar no DI
```csharp
builder.Services.AddScoped<SuperExpressaFreteStrategy>();
```

**Passo 4**: Atualizar resolver
```csharp
case ModalidadeFrete.SuperExpressa:
    return _serviceProvider.GetRequiredService<SuperExpressaFreteStrategy>();
```

✅ **Nenhum código existente foi modificado**, apenas estendido!

#### 🔹 L - Liskov Substitution Principle (LSP)

Qualquer implementação de `IFreteStrategy` pode substituir outra sem quebrar o sistema:

```csharp
public class PedidoService
{
    public async Task<PedidoResponse> CreateAsync(PedidoCreateRequest req, ...)
    {
        // Aceita QUALQUER implementação de IFreteStrategy
        IFreteStrategy strategy = _resolver.Resolve(req.Modalidade);
        decimal valorFrete = strategy.CalcularFrete(parametros);
    }
}
```

#### 🔹 I - Interface Segregation Principle (ISP)

Interfaces pequenas e focadas:

```csharp
// Interface específica para cálculo de frete
public interface IFreteStrategy
{
    decimal CalcularFrete(FreteParametros parametros);
}

// Interface específica para resolução de estratégia
public interface IFreteStrategyResolver
{
    IFreteStrategy Resolve(ModalidadeFrete modalidade);
}

// Interface específica para operações de pedido
public interface IPedidoService
{
    Task<PedidoResponse> CreateAsync(PedidoCreateRequest req, CancellationToken ct = default);
    // ... métodos relacionados a pedido
}
```

#### 🔹 D - Dependency Inversion Principle (DIP) ⭐

**Módulos de alto nível não dependem de módulos de baixo nível. Ambos dependem de abstrações.**

##### Inversão de Dependências na Prática

```csharp
// ❌ ERRADO - Dependência direta de implementação concreta
public class PedidoService
{
    private readonly InMemoryPedidoRepository _repository;  // Acoplamento forte

    public PedidoService()
    {
        _repository = new InMemoryPedidoRepository();  // Violação DIP
    }
}

// ✅ CORRETO - Dependência de abstração
public class PedidoService
{
    private readonly IPedidoRepository _repository;  // Depende de interface

    public PedidoService(IPedidoRepository repository)  // Injeção de dependência
    {
        _repository = repository;
    }
}
```

##### Arquitetura em Camadas com DIP

```
┌─────────────────────────────────────────┐
│         Frete.Api (Presentation)        │
│  ┌────────────────────────────────────┐ │
│  │    PedidosController               │ │
│  └──────────┬─────────────────────────┘ │
│             │ depende de ↓              │
│             │ IPedidoService            │
└─────────────┼─────────────────────────┬─┘
              │                         │
┌─────────────▼─────────────────────────▼─┐
│      Frete.Application (Use Cases)      │
│  ┌────────────────────────────────────┐ │
│  │      PedidoService                 │ │
│  │  - depende de IPedidoRepository    │ │
│  │  - depende de IFreteStrategyResolver │
│  └────────────────────────────────────┘ │
└──────────────┬──────────────────────────┘
               │ depende de ↓
┌──────────────▼──────────────────────────┐
│       Frete.Domain (Business Logic)     │
│  ┌────────────────────────────────────┐ │
│  │  Interfaces (Abstrações)           │ │
│  │  - IPedidoRepository               │ │
│  │  - IFreteStrategy                  │ │
│  │  - IFreteStrategyResolver          │ │
│  └────────────────────────────────────┘ │
│  ┌────────────────────────────────────┐ │
│  │  Entities & Value Objects          │ │
│  │  - Pedido                          │ │
│  │  - FreteParametros                 │ │
│  └────────────────────────────────────┘ │
└─────────────────────────────────────────┘
               ▲
               │ implementa
┌──────────────┴──────────────────────────┐
│    Frete.Infra (Implementation Details) │
│  ┌────────────────────────────────────┐ │
│  │  InMemoryPedidoRepository          │ │
│  │  (implementa IPedidoRepository)    │ │
│  └────────────────────────────────────┘ │
└─────────────────────────────────────────┘
```

**Benefícios do DIP neste projeto:**

1. **Testabilidade**: Fácil criar mocks de `IPedidoRepository` para testes
2. **Flexibilidade**: Trocar `InMemoryPedidoRepository` por `SqlPedidoRepository` sem alterar `PedidoService`
3. **Desacoplamento**: Domain não conhece detalhes de infraestrutura

```csharp
// Exemplo de configuração DI no Program.cs
builder.Services.AddSingleton<IPedidoRepository, InMemoryPedidoRepository>();
// Fácil trocar para:
// builder.Services.AddScoped<IPedidoRepository, SqlPedidoRepository>();
```

---

### Strategy Pattern

O **Strategy Pattern** é o padrão de projeto principal do sistema, permitindo selecionar dinamicamente o algoritmo de cálculo de frete.

#### Componentes do Pattern

```
┌────────────────────────────────────────────┐
│      IFreteStrategy (Interface)            │
│  + CalcularFrete(FreteParametros): decimal │
└──────────────────┬─────────────────────────┘
                   │
        ┌──────────┼──────────────┐
        │          │              │
┌───────▼──────┐ ┌─▼──────────┐ ┌─▼──────────┐
│   Normal     │ │  Expressa  │ │  Agendada  │
│   Strategy   │ │  Strategy  │ │  Strategy  │
└──────────────┘ └────────────┘ └────────────┘
```

#### Fluxo de Execução

```csharp
// 1. Cliente faz requisição
POST /api/pedidos
{
  "clientId": "...",
  "modalidade": 2,  // Expressa
  "pesoKg": 10,
  "distanciaKm": 50,
  "taxaFixa": 5
}

// 2. Controller → Service
var response = await _service.CreateAsync(request, ct);

// 3. Service resolve estratégia
IFreteStrategy strategy = _resolver.Resolve(request.Modalidade);

// 4. Estratégia calcula frete
decimal valorFrete = strategy.CalcularFrete(parametros);
// Expressa: (10 * 0.5) + (50 * 1.0) + 5 = 60

// 5. Cria pedido com valor calculado
var pedido = new Pedido(Guid.NewGuid(), clientId, valorFrete, modalidade);
```

#### Vantagens do Strategy Pattern

✅ **Eliminação de condicionais complexas**
```csharp
// ❌ SEM Strategy Pattern
public decimal CalcularFrete(ModalidadeFrete modalidade, ...)
{
    if (modalidade == ModalidadeFrete.Normal)
        return peso * 0.5m + distancia * 0.1m + taxa;
    else if (modalidade == ModalidadeFrete.Expressa)
        return peso * 0.5m + distancia * 1m + taxa;
    else if (modalidade == ModalidadeFrete.Agendada)
        return peso * 0.5m + distancia * 0.5m + taxa;
    // Difícil de testar e estender
}

// ✅ COM Strategy Pattern
public decimal CalcularFrete(FreteParametros parametros)
{
    IFreteStrategy strategy = _resolver.Resolve(modalidade);
    return strategy.CalcularFrete(parametros);
    // Fácil de testar cada estratégia isoladamente
}
```

✅ **Testabilidade individual**
```csharp
[Fact]
public void ExpressaStrategy_ShouldCalculateCorrectly()
{
    var strategy = new ExpressaFreteStrategy();
    var parametros = new FreteParametros(5m, 10m, 5m);

    var valor = strategy.CalcularFrete(parametros);

    Assert.Equal(17.5m, valor); // (5*0.5) + (10*1) + 5
}
```

✅ **Extensibilidade sem modificação** (OCP)

---

## 📁 Estrutura do Projeto

### Clean Architecture em 4 Camadas

```
Frete.sln
│
├── 📂 Frete.Domain (Camada de Domínio)
│   ├── Entities/
│   │   └── Pedido.cs                    # Entidade principal
│   ├── ValueObjects/
│   │   └── FreteParametros.cs           # Objeto de valor imutável
│   ├── Enums/
│   │   └── ModalidadeFrete.cs           # Enum de modalidades
│   ├── Interfaces/
│   │   ├── IFreteStrategy.cs            # Contrato de estratégia
│   │   ├── IFreteStrategyResolver.cs    # Contrato de resolver
│   │   └── IPedidoRepository.cs         # Contrato de repositório
│   └── Exceptions/
│       ├── DomainException.cs           # Base de exceções
│       ├── PedidoNotFoundException.cs
│       ├── PedidoAlreadyExistsException.cs
│       └── InvalidFreteParametrosException.cs
│
├── 📂 Frete.Application (Camada de Aplicação)
│   ├── DTOs/
│   │   ├── PedidoCreateRequest.cs       # Request de criação
│   │   ├── PedidoUpdateRequest.cs       # Request de atualização
│   │   ├── PedidoResponse.cs            # Response padrão
│   │   └── ErrorResponse.cs             # Response de erro
│   ├── Interfaces/
│   │   └── IPedidoService.cs            # Contrato de serviço
│   ├── Services/
│   │   ├── PedidoService.cs             # Serviço de aplicação
│   │   └── FreteStrategyResolver.cs     # Resolver de estratégias
│   └── Strategies/
│       ├── NormalFreteStrategy.cs       # Implementação Normal
│       ├── ExpressaFreteStrategy.cs     # Implementação Expressa
│       └── AgendadaFreteStrategy.cs     # Implementação Agendada
│
├── 📂 Frete.Infra (Camada de Infraestrutura)
│   └── Repositories/
│       └── InMemoryPedidoRepository.cs  # Repositório em memória
│
├── 📂 Frete.Api (Camada de Apresentação)
│   ├── Controllers/
│   │   └── PedidosController.cs         # Controller REST
│   ├── Middleware/
│   │   └── GlobalExceptionHandlerMiddleware.cs  # Tratamento global de exceções
│   └── Program.cs                       # Configuração e DI
│
└── 📂 Frete.Tests (Testes)
    ├── FreteParametrosTests.cs          # Testes de Value Object
    ├── PedidoTests.cs                   # Testes de Entity
    ├── FreteStrategyTests.cs            # Testes de Strategies
    ├── FreteStrategyResolverTests.cs    # Testes de Resolver
    ├── InMemoryPedidoRepositoryTests.cs # Testes de Repository
    ├── PedidoServiceTests.cs            # Testes de Service
    └── PedidosControllerIntegrationTests.cs  # Testes de Integração
```

---

## 📡 Endpoints da API

### Criar Pedido
```http
POST /api/pedidos
Content-Type: application/json

{
  "clientId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "modalidade": 1,
  "pesoKg": 10.5,
  "distanciaKm": 50.0,
  "taxaFixa": 5.0
}

Response: 201 Created
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "clientId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "modalidade": 1,
  "valorFrete": 15.25
}
```

### Listar Todos os Pedidos
```http
GET /api/pedidos

Response: 200 OK
[
  {
    "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "clientId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "modalidade": 1,
    "valorFrete": 15.25
  }
]
```

### Buscar Pedido por ID
```http
GET /api/pedidos/{id}

Response: 200 OK / 404 Not Found
```

### Atualizar Pedido
```http
PUT /api/pedidos/{id}
Content-Type: application/json

{
  "modalidade": 2,
  "pesoKg": 12.0,
  "distanciaKm": 60.0,
  "taxaFixa": 10.0
}

Response: 200 OK
```

### Deletar Pedido
```http
DELETE /api/pedidos/{id}

Response: 204 No Content / 404 Not Found
```

---

## 🛠️ Tecnologias Utilizadas

### Framework e Linguagem
- **.NET 9.0**
- **C# 13.0**

### Bibliotecas
- **Swashbuckle.AspNetCore** - Documentação OpenAPI/Swagger
- **xUnit** - Framework de testes
- **Moq** - Biblioteca de mocking
- **FluentAssertions** - Asserções para testes

### Padrões e Práticas
- **Clean Architecture**
- **SOLID Principles**
- **Strategy Pattern**
- **Repository Pattern**
- **Dependency Injection**
- **Test-Driven Development (TDD)**

---

## 📊 Resumo de Qualidade

| Critério | Evidências |
|----------|------------|
| **Clareza e Organização** | Clean Architecture, nomenclatura clara, separação de responsabilidades |
| **SOLID** | Todos os 5 princípios aplicados corretamente |
| **Testes** | 7 arquivos de teste, TDD, unitários + integração |
| **API REST** | RESTful, Swagger, middleware global, DTOs |
| **Boas Práticas C#/.NET** | C# 13, async/await, DI, logging, records |
| **Abstração e Extensão** | Strategy Pattern, OCP, interfaces, DIP |


## ✅ Critérios de Avaliação

### 1️⃣ Clareza e Organização do Código

✅ **Clean Architecture** com separação clara de responsabilidades
```
Domain → Application → Infrastructure → API
```

✅ **Nomenclatura descritiva e consistente**
```csharp
// Classes
public class PedidoService : IPedidoService
public class ExpressaFreteStrategy : IFreteStrategy

// Métodos descritivos
public async Task<PedidoResponse> CreateAsync(...)
public decimal CalcularFrete(FreteParametros parametros)

// Variáveis claras
var parametros = new FreteParametros(pesoKg, distanciaKm, taxaFixa);
IFreteStrategy strategy = _resolver.Resolve(modalidade);
```

✅ **Records imutáveis para DTOs**
```csharp
public record PedidoResponse(Guid Id, Guid ClientId, ModalidadeFrete Modalidade, decimal ValorFrete);
public record ErrorResponse(string Error, int Status, string? Details = null);
```

✅ **Sealed classes** onde apropriado
```csharp
public sealed class PedidoService : IPedidoService
public sealed record Pedido
public sealed record FreteParametros
```

✅ **Logging estruturado** em todas as camadas
```csharp
_logger.LogInformation("Pedido {PedidoId} criado com sucesso", pedido.Id);
_logger.LogWarning("Pedido {PedidoId} não encontrado", id);
```

---

### 2️⃣ Correta Aplicação dos Princípios SOLID

| Princípio | Implementação | Exemplo |
|-----------|---------------|---------|
| **SRP** | ✅ Cada classe tem uma responsabilidade | `FreteParametros` valida parâmetros, `PedidoService` orquestra |
| **OCP** | ✅ Strategy Pattern permite extensão sem modificação | Adicionar nova modalidade sem alterar código existente |
| **LSP** | ✅ Todas as estratégias são substituíveis | Qualquer `IFreteStrategy` funciona no `PedidoService` |
| **ISP** | ✅ Interfaces pequenas e focadas | `IFreteStrategy`, `IPedidoRepository`, `IPedidoService` |
| **DIP** | ✅ Dependências de abstrações via DI | `PedidoService` depende de `IPedidoRepository` |

---

### 3️⃣ Cobertura e Qualidade dos Testes

✅ **7 arquivos de teste** cobrindo todas as camadas

✅ **Testes unitários** com isolamento usando Moq
```csharp
var mockRepository = new Mock<IPedidoRepository>();
var mockResolver = new Mock<IFreteStrategyResolver>();
```

✅ **Testes de integração** com `WebApplicationFactory`
```csharp
public class PedidosControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
```

✅ **Data-driven tests** com `[Theory]` e `[InlineData]`
```csharp
[Theory]
[InlineData(10, 20, 5)]
[InlineData(0.5, 100, 0)]
public void Constructor_ShouldCreateValidFreteParametros(...)
```

✅ **FluentAssertions** para asserções legíveis
```csharp
response.Should().NotBeNull();
result.Id.Should().Be(pedidoId);
```

✅ **Testes de casos extremos e validações**
```csharp
[Fact]
public async Task Create_ShouldReturnBadRequest_WhenPesoKgIsInvalid()
```

✅ **AAA Pattern** (Arrange-Act-Assert) consistente
```csharp
// Arrange
var strategy = new ExpressaFreteStrategy();

// Act
var valor = strategy.CalcularFrete(parametros);

// Assert
Assert.Equal(esperado, valor);
```

---

### 4️⃣ Estrutura da API REST

✅ **RESTful endpoints** seguindo convenções HTTP

| Método | Endpoint | Descrição | Status Code |
|--------|----------|-----------|-------------|
| `GET` | `/api/pedidos` | Lista todos os pedidos | 200 OK |
| `GET` | `/api/pedidos/{id}` | Busca pedido por ID | 200 OK / 404 Not Found |
| `POST` | `/api/pedidos` | Cria novo pedido | 201 Created |
| `PUT` | `/api/pedidos/{id}` | Atualiza pedido | 200 OK / 404 Not Found |
| `DELETE` | `/api/pedidos/{id}` | Remove pedido | 204 No Content / 404 Not Found |

✅ **Swagger/OpenAPI** com documentação completa
```csharp
[SwaggerOperation(
    Summary = "Cria um novo pedido",
    Description = "Calcula o frete baseado na modalidade e cria um pedido",
    OperationId = "CreatePedido",
    Tags = new[] { "Pedidos" }
)]
[SwaggerResponse(201, "Pedido criado com sucesso", typeof(PedidoResponse))]
```

✅ **Content Negotiation** com `application/json`
```csharp
[Produces("application/json")]
```

✅ **Middleware global** para tratamento de exceções
```csharp
public class GlobalExceptionHandlerMiddleware
{
    // Converte exceções de domínio em respostas HTTP apropriadas
    case PedidoNotFoundException => 404
    case PedidoAlreadyExistsException => 409
    case InvalidFreteParametrosException => 400
}
```

✅ **DTOs separados** para Request e Response
```csharp
public record PedidoCreateRequest(Guid ClientId, ModalidadeFrete Modalidade, ...);
public record PedidoResponse(Guid Id, Guid ClientId, ...);
```

✅ **CancellationToken** para operações assíncronas
```csharp
public async Task<ActionResult<PedidoResponse>> Create(
    [FromBody] PedidoCreateRequest req,
    CancellationToken ct)
```

---

### 5️⃣ Boas Práticas em C# e .NET

✅ **C# 13** com recursos modernos
```csharp
// Primary constructors
public sealed class PedidosController(IPedidoService service, ILogger<PedidosController> logger)

// Record types
public record PedidoResponse(Guid Id, Guid ClientId, ModalidadeFrete Modalidade, decimal ValorFrete);

// Pattern matching
switch (exception)
{
    case PedidoNotFoundException notFoundEx:
        errorResponse = ErrorResponse.NotFound(notFoundEx.Message);
        break;
}
```

✅ **Async/Await** em todas as operações I/O
```csharp
public async Task<PedidoResponse> CreateAsync(PedidoCreateRequest req, CancellationToken ct = default)
{
    await _repository.AddAsync(pedido, ct);
}
```

✅ **Nullable Reference Types** habilitado
```csharp
public async Task<PedidoResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
```

✅ **Dependency Injection** nativo do .NET
```csharp
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddSingleton<IPedidoRepository, InMemoryPedidoRepository>();
```

✅ **ILogger** para logging estruturado
```csharp
_logger.LogInformation("Pedido {PedidoId} criado com sucesso", pedido.Id);
```

✅ **ConcurrentDictionary** para thread-safety
```csharp
private readonly ConcurrentDictionary<Guid, Pedido> _db = new();
```

✅ **Validações explícitas** com exceções tipadas
```csharp
if (pesoKg <= 0)
    throw new InvalidFreteParametrosException("O peso deve ser maior que zero.");
```

✅ **Imutabilidade** com records e init
```csharp
public sealed record FreteParametros
{
    public decimal PesoKg { get; init; }
    public decimal DistanciaKm { get; init; }
}
```

---

### 6️⃣ Capacidade de Abstração e Extensão

✅ **Interfaces bem definidas** para cada abstração
```csharp
public interface IFreteStrategy
public interface IFreteStrategyResolver
public interface IPedidoRepository
public interface IPedidoService
```

✅ **Strategy Pattern** permite adicionar novas modalidades sem modificação

✅ **Repository Pattern** permite trocar implementação de persistência
```csharp
// Fácil trocar de InMemory para SQL
builder.Services.AddSingleton<IPedidoRepository, InMemoryPedidoRepository>();
// Para:
builder.Services.AddScoped<IPedidoRepository, SqlPedidoRepository>();
```

✅ **Factory/Resolver Pattern** para criação de estratégias
```csharp
public class FreteStrategyResolver : IFreteStrategyResolver
{
    public IFreteStrategy Resolve(ModalidadeFrete modalidade) { ... }
}
```

✅ **Value Objects** encapsulam validações
```csharp
public sealed record FreteParametros
{
    public FreteParametros(decimal pesoKg, decimal distanciaKm, decimal taxaFixa)
    {
        // Validações no construtor
    }
}
```

✅ **Exceções de domínio** específicas
```csharp
public class PedidoNotFoundException : DomainException
public class InvalidFreteParametrosException : DomainException
```

---
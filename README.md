# Good Hamburger API (Blazor + Minimal API)

## Visão geral
API para montagem de pedidos de hamburguer com regras de combo e desconto.
A aplicação roda em `.NET 10`, usa EF Core com PostgreSQL (ou InMemory fallback) e possui testes unitários/integrados.

## Stack usada
- `.NET 10`
- `Blazor Server` (host da aplicação)
- `ASP.NET Core Minimal API`
- `Entity Framework Core 10`
- `PostgreSQL + Npgsql`
- `xUnit`
- `Docker / Docker Compose`
- `GitHub Actions`
- `Swagger (OpenAPI)`

## Arquitetura e responsabilidades
- `Program.cs`: composição da aplicação, middlewares e endpoints.
- `Services/MenuService.cs`: catálogo de itens do menu.
- `Services/OrderService.cs`: regras de negócio de pedido/desconto e persistência.
- `Data/AppDbContext.cs`: mapeamento EF Core.
- `Data/Entities/*`: entidades persistidas.
- `Contracts/*`: DTOs de request/response e paginação.
- `Middleware/*`: tratamento de erro, API key e headers de segurança.
- `Frontend/Services/*`: clients do Blazor para consumo da API (`MenuApiClient`, `OrdersApiClient`).
- `Frontend/ViewModels/*`: estado de tela no frontend (`CreateOrderViewModel`).

## Regras de negócio
- Pedido deve ter ao menos 1 item.
- Itens duplicados por código não são permitidos.
- Máximo de 1 item por categoria (`Sanduíche`, `Batata`, `Refrigerante`).
- Descontos:
  - Sanduíche + Batata + Refrigerante: `20%`
  - Sanduíche + Refrigerante: `15%`
  - Sanduíche + Batata: `10%`
  - Caso contrário: `0%`

## Fluxo do pedido
### Fluxo de criação de pedido
1. Recebe `itemCodes`.
2. Valida existência de cada item no menu.
3. Valida duplicidade de itens e restrição por categoria.
4. Calcula subtotal.
5. Determina percentual de desconto conforme composição do combo.
6. Calcula valor final (`total`).
7. Persiste o pedido no banco.
8. Retorna DTO de resposta.

## Endpoints
Base: `/api`

| Endpoint | Descrição | Status esperados | Retorno |
|---|---|---|---|
| `GET /menu` | Lista itens do cardápio | `200` | `MenuItemResponse[]` |
| `GET /orders?page=1&pageSize=10` | Lista pedidos paginados | `200`, `400` | `PaginatedResponse<OrderResponse>` |
| `GET /orders/{id}` | Busca pedido por id | `200`, `404` | `OrderResponse` |
| `POST /orders` | Cria novo pedido | `201`, `400` | `OrderResponse` + header `Location` |
| `PUT /orders/{id}` | Atualiza pedido | `200`, `400`, `404` | `OrderResponse` |
| `DELETE /orders/{id}` | Remove pedido | `204`, `404` | sem corpo |

### Exemplo de request (`POST/PUT`)
```json
{
  "itemCodes": ["XBURGER", "FRIES", "SODA"]
}
```

## Como rodar localmente
```bash
dotnet restore
cd "Desafio Técnico - Good Hamburguer"
dotnet run
```

## Documentação da API (Swagger)
Com a aplicação em execução em ambiente de desenvolvimento, acesse:

- `https://localhost:<porta>/swagger`

## Frontend Blazor
Rotas principais da interface:

- `/` Home
- `/cardapio` Consulta de itens
- `/pedidos` Criação, edição, exclusão e listagem paginada

No frontend, as telas consomem os endpoints da API por clients dedicados, mantendo as regras de negócio centralizadas no backend.

## Como rodar com Docker
```bash
docker compose up --build
```

## Como rodar migrations
Pré-requisito: `dotnet-ef` instalado.

```bash
dotnet ef database update --project "Desafio Técnico - Good Hamburguer/Desafio Técnico - Good Hamburguer.csproj" --startup-project "Desafio Técnico - Good Hamburguer/Desafio Técnico - Good Hamburguer.csproj"
```

Para criar nova migration:
```bash
dotnet ef migrations add NomeDaMigration --project "Desafio Técnico - Good Hamburguer/Desafio Técnico - Good Hamburguer.csproj" --startup-project "Desafio Técnico - Good Hamburguer/Desafio Técnico - Good Hamburguer.csproj" --output-dir Data/Migrations
```

## Testes automatizados
- **Testes unitários** (`Desafio Técnico - Good Hamburguer.Tests.Unit`):
  - regras de desconto;
  - validações de domínio.
- **Testes de integração** (`Desafio Técnico - Good Hamburguer.Tests.Integration`):
  - fluxo completo de CRUD;
  - validação de erros da API via middleware.

## Como executar os testes
```bash
dotnet test "Desafio Técnico - Good Hamburguer.Tests.Unit/Desafio Técnico - Good Hamburguer.Tests.Unit.csproj"
dotnet test "Desafio Técnico - Good Hamburguer.Tests.Integration/Desafio Técnico - Good Hamburguer.Tests.Integration.csproj"
```

## Segurança aplicada
- Validação de entrada no domínio.
- `ProblemDetails` para padronização de erro.
- Sanitização de erro interno (`500` sem detalhe técnico).
- `HTTPS redirection` + `HSTS` em produção.
- Headers de segurança (`X-Content-Type-Options`, `X-Frame-Options`, etc.).
- API key opcional por `X-Api-Key`.
- Rate limit global.
- CORS configurável por `Cors:AllowedOrigins`.
- Limite de payload para rotas de API (`32 KB`).

## CI/CD
Workflow em `.github/workflows/ci.yml` com:
- restore
- build
- testes unitários
- testes de integração
- validação de build Docker

## Configuração
`appsettings.json`:
- `ConnectionStrings:DefaultConnection`
- `Security:ApiKey`
- `Cors:AllowedOrigins`

Esses valores podem ser sobrescritos por variáveis de ambiente:
- `ConnectionStrings__DefaultConnection`
- `Security__ApiKey`

## Decisões técnicas
- Uso de middleware para tratamento de exceções, autenticação por API key e headers de segurança para evitar duplicação nos endpoints e garantir padronização da API.
- DTOs de resposta para desacoplamento entre contrato HTTP e modelo de domínio.
- `InMemory` como fallback quando `DefaultConnection` não está configurada, facilitando execução local e testes rápidos sem dependência externa.
- `OrderService` centraliza regras de negócio para consistência funcional entre endpoints.

## Melhorias futuras
- Observabilidade (OpenTelemetry + dashboard).
- Autenticação/autorização com JWT.
- Idempotência para `POST /orders`.
- Versionamento da API.
- Testes de carga para política de rate limiting.

## Governança de Git (workflow recomendado)
- Fluxo operacional documentado em `.github/CONTRIBUTING.md`.
- Template de PR em `.github/pull_request_template.md`.
- Mensagem de commit padronizada em `.gitmessage.txt`.

Comandos base:

```bash
git fetch --all --prune --tags
git checkout main
git pull --rebase origin main
git checkout -b feature/nome-curto
```

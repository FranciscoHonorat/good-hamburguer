# Copilot Instructions

## Project Guidelines
- Para o frontend Blazor deste projeto: manter regras de negócio no backend/API, separar páginas/estado/acesso à API em serviços dedicados, centralizar parsing de erros ProblemDetails, e garantir UX com loading/error/success e prevenção de ações duplicadas.
- Manter `Pedidos.razor` como orquestrador e quebrar UI em componentes objetivos (`OrderForm`, `OrdersTable`, `Pagination`) sem overengineering.

## Version Control Practices
- Preferir fluxo de Git bem documentado, utilizando fetch/rebase para manter um histórico limpo.
- Adotar versionamento semântico rigoroso para controle de versões, garantindo clareza nas mudanças e compatibilidade.
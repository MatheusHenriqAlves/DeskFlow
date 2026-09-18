# Arquitetura do DeskFlow

## Responsabilidades

| Projeto | Responsabilidade | Referências |
| --- | --- | --- |
| Domain | Entidades, enums e Smart Priority | Nenhum outro projeto |
| Application | Serviços de aplicação, DTOs e contratos de acesso a dados e segurança | Domain |
| Infrastructure | Repositórios EF Core, PostgreSQL, migrations, seed, JWT e hash de senhas | Application |
| Api | Endpoints, autorização HTTP, tratamento de erros e composição | Application, Infrastructure |

Os controllers chamam os serviços de Application. Os serviços usam as interfaces de
Application; Infrastructure fornece as implementações. Não expor DbContext, DbSet
ou IQueryable nos contratos da Application.

ITicketRepository e IUserRepository compartilham o mesmo DeskFlowDbContext por
requisição. IUnitOfWork salva as alterações em conjunto, preservando a transação
do chamado com seu histórico. A Application aplica a restrição de visibilidade
por proprietário e as regras de transição de status. A API mantém as restrições
de perfil dos endpoints, inclusive exclusão exclusiva de Admin.

## Banco e migrations

O contexto, a migration existente e o snapshot ficam em
`backend/DeskFlow.Infrastructure`. A migration mantém o identificador
`202608210001_InitialCreate`; esta refatoração não adiciona operações de esquema.
As strings de tipo no snapshot acompanham o namespace das entidades em Domain.

A inicialização da API continua aplicando migrations e executando o seed somente
quando não existem usuários. O formato dos hashes de senha e dos tokens JWT foi
preservado. Configurações locais continuam na API e no Docker Compose.

Para futuras migrations, com a ferramenta dotnet-ef instalada e compatível:

```powershell
dotnet ef migrations add NomeDaMigration --project backend/DeskFlow.Infrastructure --startup-project backend/DeskFlow.Api --output-dir Migrations
```

Revise as operações geradas antes de aplicar uma migration. O ponto de restauração
Git protege código; para mudanças de esquema, mantenha também backup do PostgreSQL.

## Validação

```powershell
dotnet build DeskFlow.sln
dotnet test DeskFlow.sln --no-build
docker compose build api
```

Os testes cobrem prioridade, criação e acesso a chamados, fluxo de atendimento,
compatibilidade de senhas, login, isolamento das camadas e descoberta/coerência
da migration. Os testes com EF InMemory não substituem a validação do PostgreSQL
e dos endpoints HTTP no ambiente Docker.

Após atualizar a API, confira login, listagem, criação, comentários, mudanças de
status e categoria, histórico, métricas e exclusão como administrador.

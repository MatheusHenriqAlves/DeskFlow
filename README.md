# DeskFlow

DeskFlow é uma central de chamados (Help Desk) full-stack desenvolvida em .NET e React. O sistema conta com gerenciamento por perfil de acesso, regras automatizadas para cálculo de prioridade explicável, histórico de alterações, sistema de comentários, filtros avançados, painel de métricas, autenticação via JWT e persistência de dados em PostgreSQL.

## Funcionalidades

- **Usuário**: Criação e acompanhamento de chamados, inclusão de comentários, visualização de histórico de auditoria e status em tempo real.
- **Técnico**: Visualização da fila geral, atribuição/atendimento de chamados, alteração de status e categoria, comentários e resolução de tickets.
- **Admin**: Gerenciamento completo de usuários e chamados, além de consulta a métricas do sistema.
- **Sistema**: Regra automatizada para cálculo de prioridade, registro imutável de histórico/auditoria, validação de requisições, controle de acesso baseado em funções (RBAC) e persistência relacional.

## Tecnogias Utilizadas

- **Backend**: C# / ASP.NET Core 10
- **Banco de Dados**: PostgreSQL
- **ORM**: Entity Framework Core + Npgsql
- **Autenticação**: JSON Web Tokens (JWT)
- **Frontend**: React 19 + TypeScript + Vite
- **Testes**: xUnit
- **Containerização**: Docker / Docker Compose
- **Documentação da API**: OpenAPI / Swagger

## Arquitetura

O backend segue os princípios da Clean Architecture, estando dividido em quatro camadas principais:

- **DeskFlow.Domain**: Entidades de domínio, enums e regras de negócio puras (incluindo a lógica de cálculo de prioridade), sem dependências externas.
- **DeskFlow.Application**: Casos de uso (Use Cases), DTOs, validações e interfaces para persistência e segurança. Depende exclusivamente do projeto Domain.
- **DeskFlow.Infrastructure**: Implementação do Entity Framework Core, acesso ao PostgreSQL, migrations, repositórios, serviços JWT e hashing de senhas. Implementa as interfaces da camada Application.
- **DeskFlow.Api**: Endpoints/Controllers, middlewares, configuração da autenticação HTTP e injeção de dependências.

As dependências entre os projetos seguem o fluxo de fora para dentro: `Application → Domain`, `Infrastructure → Application`, e `Api → Application / Infrastructure`.

## Smart Priority (Cálculo de Prioridade)

O motor de priorização do DeskFlow avalia o texto do chamado com base em uma pontuação combinada de **Impacto + Urgência + Contexto + Risco de Segurança**. Cada ticket processado armazena o valor final da prioridade, o score numérico e a justificativa técnica correspondente.

### Pontuação e Níveis
- **Low**: Score < 4
- **Medium**: Score entre 4 e 6
- **High**: Score entre 7 e 9
- **Critical**: Score >= 10

### Regras de Avaliação
- Relatos de ataques em andamento, infecção por ransomware, invasões, vazamentos de dados ou perda comprovada de informações recebem pontuação mínima de risco 10 (`Critical`).
- Suspeitas de incidentes e falhas em mecanismos de proteção recebem pontuação mínima de risco 7 (`High`), podendo atingir `Critical` conforme o impacto e a urgência reportados.
- Paralisações que afetam um setor inteiro possuem piso `High`. Bloqueios de acesso que afetam toda a organização possuem piso `Critical`.
- O algoritmo ignora termos genéricos ou menções preventivas/educacionais (como "treinamento de segurança"), garantindo que a presença da palavra isolada não eleve a criticidade indevidamente.

Na inicialização da aplicação, os chamados com status `Open` ou `InProgress` são reavaliados. Se houver alteração no score, na prioridade ou na justificativa, um registro do tipo `PriorityReassessed` é adicionado ao histórico de auditoria do ticket. Reavaliações sem alteração de resultado não geram duplicidade no histórico. Chamados nos status `Resolved` e `Closed` mantêm o histórico e a classificação originais.

---

## Execução da Aplicação

### Via Docker Compose (Recomendado)

1. Crie o arquivo de variáveis de ambiente a partir do modelo:
   ```bash
   cp .env.example .env
   ```
2. Inicie os containers com o build das aplicações:
   ```bash
   docker compose up --build
   ```

**Endereços da Aplicação:**
- **Frontend**: `http://localhost:5173`
- **API (Backend)**: `http://localhost:8080`
- **OpenAPI / Documentação**: `http://localhost:8080/openapi/v1.json`

**Credenciais Padrão para Testes:**
- **Admin**: `admin@deskflow.local` | Senha: `Admin@123!`
- **Técnico**: `tech@deskflow.local` | Senha: `Tech@123!`
- **Usuário**: `user@deskflow.local` | Senha: `User@123!`

---

### Execução Local (Sem Docker)

#### 1. Banco de Dados
Com uma instância do PostgreSQL ativa na máquina local, configure as credenciais da conexão e a chave do JWT via .NET User Secrets no projeto da API:

```bash
cd backend/DeskFlow.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=deskflow;Username=postgres;Password=SUA_SENHA_AQUI"
dotnet user-secrets set "Jwt:Key" "SUA_CHAVE_SECRETA_JWT_COM_NO_MINIMO_32_CARACTERES"
```

#### 2. Backend
```bash
cd backend/DeskFlow.Api
dotnet restore
dotnet run
```
As migrations do Entity Framework Core são aplicadas automaticamente durante a inicialização da API.

#### 3. Frontend
```bash
cd frontend/deskflow-web
npm install
npm run dev
```

Caso necessário apontar para outra porta ou endereço de API, crie um arquivo `.env.local` dentro da pasta `frontend/deskflow-web`:
```text
VITE_API_URL=http://localhost:5087
```

---

## Execução de Testes

### Testes do Backend (xUnit)
```bash
dotnet test DeskFlow.sln
```

### Build de Produção do Frontend
```bash
cd frontend/deskflow-web
npm install
npm run build
```

---

## Principais endpoints

| Método | Endpoint | Acesso |
|---|---|---|
| POST | `/api/auth/register` | Público |
| POST | `/api/auth/login` | Público |
| GET | `/api/tickets` | Autenticado |
| POST | `/api/tickets` | Autenticado |
| GET | `/api/tickets/{id}` | Dono/Técnico/Admin |
| POST | `/api/tickets/{id}/comments` | Dono/Técnico/Admin |
| GET | `/api/tickets/{id}/history` | Dono/Técnico/Admin |
| POST | `/api/tickets/{id}/assign` | Técnico/Admin |
| PUT | `/api/tickets/{id}/status` | Técnico/Admin |
| PUT | `/api/tickets/{id}/category` | Técnico/Admin |
| DELETE | `/api/tickets/{id}` | Admin |
| GET | `/api/users` | Admin |
| GET | `/api/metrics` | Admin |

Filtros de tickets: `search`, `status`, `priority`, `category`, `assigned`, `page`, `pageSize`.

## Segurança e Boas Práticas

- **Criptografia de Senhas**: Armazenamento utilizando `PasswordHasher<TUser>` com algoritmo com salt e hash seguro.
- **Autenticação e Autorização**: Tokens JWT validados por emissor, audiência, expiração e assinatura digital, integrados ao RBAC (`User`, `Technician`, `Admin`).
- **Segurança da API**: Políticas de CORS configuradas por ambiente e isolamento de segredos em variáveis de ambiente e cofres de segredos.

---

## Integração Contínua (CI)

O repositório possui uma pipeline de CI configurada via GitHub Actions (`.github/workflows/ci.yml`), responsável por executar compilação, restauração de pacotes, testes unitários no backend e o build do frontend a cada *push* ou *pull request* direcionado às branches principais.

---

## Estrutura

```text
DeskFlow/
├── backend/
│   ├── DeskFlow.Api/
│   ├── DeskFlow.Domain/
│   ├── DeskFlow.Application/
│   ├── DeskFlow.Infrastructure/
│   └── DeskFlow.Tests/
├── frontend/
│   └── deskflow-web/
├── docker-compose.yml
├── .env.example
├── DeskFlow.sln
└── README.md
```
---

## Melhorias planejadas para o projeto

- Anexos em object storage
- Notificações por e-mail/tempo real
- SLA por prioridade
- Refresh tokens
- Observabilidade distribuída
- Testes E2E com Playwright

## Como rodar localmente

Pré-requisitos: .NET SDK 10, PostgreSQL e Node.js 24.15 ou superior na linha 24 (ou Node 26+). Configure a conexão e o JWT conforme a seção de execução local acima.

Na raiz do repositório (onde está `DeskFlow.sln`):

```bash
dotnet restore DeskFlow.sln
dotnet build DeskFlow.sln --configuration Release
dotnet test DeskFlow.sln --configuration Release
dotnet run --project backend/DeskFlow.Api
```

Em outro terminal, também a partir da raiz:

```bash
cd frontend/deskflow-web
npm install
npm run lint
npm run format:check
npm run test -- --run
npm run build
npm run dev
```

No PowerShell, use `npm.cmd` se a política de execução bloquear `npm.ps1`.
`npm run format` aplica a formatação; `npm run test:watch` executa os testes em modo contínuo.
O CI utiliza `npm ci` para instalar exatamente o lockfile e executa lint, formatação, testes e build.

O frontend usa TypeScript 6.0.3, compatível com a versão estável de typescript-eslint adotada. React 19 e Vite 8 são mantidos. A configuração do Vite é carregada nativamente pelo Node e os testes usam workers em threads.

## Capturas de tela

### Login

<!-- TODO: screenshot -->

### Dashboard

<!-- TODO: screenshot -->

### Detalhes do chamado

<!-- TODO: screenshot -->

### Administração de usuários

<!-- TODO: screenshot -->
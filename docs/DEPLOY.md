# Deploy do DeskFlow

O DeskFlow está preparado para deploy em qualquer plataforma que aceite containers Docker e PostgreSQL gerenciado.

## Componentes

1. PostgreSQL
2. `backend/DeskFlow.Api` (porta interna 8080)
3. `frontend/deskflow-web` (Nginx, porta interna 80)

## Variáveis da API

```text
ConnectionStrings__DefaultConnection=Host=...;Port=5432;Database=...;Username=...;Password=...
Jwt__Key=<segredo longo e aleatório>
Jwt__Issuer=DeskFlow
Jwt__Audience=DeskFlow.Web
AllowedOrigins__0=https://SEU-FRONTEND
Seed__AdminPassword=<senha temporária forte>
Seed__TechnicianPassword=<senha temporária forte>
Seed__UserPassword=<senha temporária forte>
```

## Frontend

No build do frontend defina:

```text
VITE_API_URL=https://SUA-API
```

## Ordem recomendada

1. Provisione PostgreSQL.
2. Publique a API com as variáveis acima.
3. Confirme `GET /api/health`.
4. Publique o frontend apontando `VITE_API_URL` para a API pública.
5. Atualize `AllowedOrigins__0` com a URL pública do frontend.
6. Faça login com o admin inicial e troque as credenciais de demonstração para qualquer ambiente compartilhado.

## Validação pós-deploy

- `/api/health` retorna HTTP 200.
- `/openapi/v1.json` abre corretamente.
- login funciona no frontend.
- criação de ticket persiste após reiniciar a API.
- técnico consegue assumir e resolver ticket.
- admin consegue visualizar métricas e alterar perfis.
- usuário comum não acessa `/api/users` nem `/api/metrics`.

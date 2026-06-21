# EventGate — Front-end (React + TypeScript)

SPA que consome a API EventGate. Duas frentes:

- **Público** — lista de eventos, detalhe + inscrição (recebe o código de acesso), autoatendimento LGPD (consultar/excluir por código + e-mail).
- **Equipe** — login (JWT), painel de portaria (validar código, impede reuso), criar evento (Organizer), estatísticas de presença.

## Stack

Vite + React 18 + TypeScript + React Router. Sem dependências de UI — CSS próprio, tema escuro.

## Rodar

```bash
npm install
npm run dev      # http://localhost:5173
```

A URL da API vem de `VITE_API_URL` (`.env`). Padrão `http://localhost:5080` (dotnet run).
Para Docker compose, troque para `http://localhost:8080`.

> A API precisa estar no ar e com CORS liberando `http://localhost:5173`
> (já configurado em `Program.cs`, seção `Cors:AllowedOrigins`).

## Estrutura

```
src/
├── api/         # client fetch (Bearer + tratamento de erro) e tipos dos DTOs
├── auth/        # AuthContext (JWT em localStorage, perfil)
├── components/  # Layout, ProtectedRoute, Field
└── pages/       # Events, EventDetail, MyData (LGPD), Login, GatePanel, CreateEvent, Stats
```

Build de produção: `npm run build` (typecheck + bundle em `dist/`).

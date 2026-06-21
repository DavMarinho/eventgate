# EventGate v2 — Plano de Implementação

> Documento de escopo. Descreve **tudo** que será construído nesta evolução, as
> **linguagens e tecnologias** de cada parte, o modelo de dados, os fluxos, a
> segurança/LGPD e o passo a passo de execução. Serve como contrato do que vamos
> entregar antes de escrever o código.

---

## 1. Visão geral

Plataforma de **inscrição e validação de presença** para um evento realizado na
**USP, aberto ao público**. A pessoa se inscreve online enviando nome, e-mail,
**foto**, data de nascimento e **curso** (lista da USP ou "Outro"). O sistema gera
um **código de acesso único**, monta um **QR Code** e envia por **e-mail**. No dia,
a equipe na portaria **lê o QR**, confere a **foto e os dados** da pessoa na tela e
**confirma a entrada** (o código não pode ser reusado). A equipe ainda tem um
**dashboard** com gráficos de participação por curso e por semestre.

Princípio condutor: **segurança e LGPD by design**. Coletamos só o necessário
(sem CPF), a foto serve apenas para conferência na portaria e nunca é pública, e
dados pessoais nunca trafegam na URL.

---

## 2. Linguagens e tecnologias

| Camada | Linguagem | Tecnologia / Biblioteca |
|---|---|---|
| Back-end (API) | **C# 14 / .NET 10** | ASP.NET Core 10 (Web API, Controllers) |
| Acesso a dados | C# + **T-SQL** | Entity Framework Core 10 + SQL Server |
| Autenticação | C# | JWT (Bearer) + autorização por perfil (RBAC) |
| Geração de QR | C# | **QRCoder** (gera PNG do código) |
| Envio de e-mail | C# | **Brevo** (HTTP API via `HttpClient`) |
| Front-end | **TypeScript** | React 18 + Vite + React Router |
| QR no navegador | TypeScript | `qrcode` (desenhar) · `@zxing/browser` (ler com a câmera) |
| Gráficos | TypeScript | **Chart.js** (via `react-chartjs-2`) |
| Estilo | CSS puro | Tema escuro próprio, sem framework de UI |
| Testes | C# | xUnit + Moq |
| Infra | YAML / Dockerfile | Docker + Docker Compose |
| CI | YAML | GitHub Actions |
| Documentação da API | — | Swagger / OpenAPI |

---

## 3. Arquitetura

Mantém a **arquitetura em camadas** já existente (Clean Architecture), com
dependências apontando para **abstrações** (interfaces na Application):

```
src/EventGate.Api/
├── Domain/            # Entidades e enums (User, Event, Registration, Course, AuditLog)
├── Application/       # Casos de uso, DTOs, interfaces (não conhece banco nem web)
├── Infrastructure/    # EF, repositórios, segurança, QR, e-mail (Brevo), seed
└── Api/               # Controllers + middleware
tests/EventGate.Tests/ # Testes unitários
web/                   # Front-end React + TypeScript (SPA)
```

---

## 4. Modelo de dados (mudanças da v2)

### 4.1 Nova entidade: `Course`
Lista **controlada** de cursos (seedada com cursos da USP). Garante que o
dashboard agrupe corretamente.

| Campo | Tipo | Nota |
|---|---|---|
| Id | Guid | PK |
| Name | string | Ex.: "Engenharia de Computação" |
| Code | string? | Sigla/identificador opcional |
| IsActive | bool | Permite ocultar sem apagar |

### 4.2 `Registration` (campos novos)
A inscrição deixa de ser só nome+e-mail e passa a ter:

| Campo | Tipo | Obrigatório | Nota |
|---|---|---|---|
| ParticipantName | string | sim | já existia |
| ParticipantEmail | string | sim | já existia |
| AccessCode | string (único) | sim | já existia |
| **PhotoData** | byte[] (varbinary) | sim | foto, guardada no banco |
| **PhotoContentType** | string | sim | ex.: `image/jpeg` |
| **BirthDate** | DateOnly | sim | data de nascimento |
| **CourseId** | Guid? | condicional | FK quando curso da lista |
| **CourseOther** | string? | condicional | texto livre quando "Outro" |
| **Semester** | int? | só se curso da lista | 1–12 |
| Status | enum | sim | Registered / CheckedIn / Cancelled |
| ConsentAccepted / ConsentAcceptedAt | bool / data | sim | LGPD |

> Regra: ou `CourseId` **ou** `CourseOther` é preenchido (nunca os dois, nunca
> nenhum). `Semester` só faz sentido com `CourseId`.

**Sem CPF.** Removido do escopo por decisão de minimização de dados.

---

## 5. Fluxos

### 5.1 Inscrição (público)
1. Front carrega a lista de cursos (`GET /api/courses`) para o autocomplete.
2. Pessoa preenche nome, e-mail, foto, nascimento, curso (lista ou "Outro"),
   semestre, e aceita o consentimento (LGPD).
3. Envio via **`multipart/form-data`** (por causa da foto).
4. API valida, gera **AccessCode** único, salva a inscrição.
5. API monta o **QR** (QRCoder) e **enfileira** o e-mail (Brevo) — assíncrono.
6. Resposta imediata com o código; o front mostra o QR e botão **baixar**.

### 5.2 E-mail de confirmação (Brevo, assíncrono)
- Disparado por um worker em segundo plano (`BackgroundService` + fila em memória).
- Se o e-mail falhar, **a inscrição não falha** (o código já está salvo); o erro é
  logado e pode ser reenviado.
- Conteúdo: nome, evento, **código** e **QR anexado** (PNG inline via `cid`).

### 5.3 Portaria — leitura do QR (equipe, 2 passos)
```
QR (contém só o AccessCode)
  → equipe escaneia com a câmera (@zxing/browser)
  → GET /api/checkin/lookup?code=...   (staff)  → card: FOTO + nome + curso + nascimento + e-mail
  → equipe confere a foto com a pessoa
  → POST /api/checkin/validate          (staff)  → marca presença, impede reuso
```
- O **lookup** apenas exibe os dados; **não** consome o código.
- O **validate** confirma a entrada e bloqueia o reuso.
- Toda leitura é **autenticada (staff), com rate limit e auditoria** (quem
  consultou qual código, quando).

### 5.4 Dashboard (equipe)
- `GET /api/dashboard/events/{id}/by-course` → inscritos/presentes por curso.
- `GET /api/dashboard/events/{id}/by-semester` → distribuição por semestre.
- Agregação feita em **SQL (`GROUP BY`)**, não em memória.
- Front desenha: **barra** (top cursos + "Outros") e **rosca** (semestre da maioria),
  além dos KPIs (inscritos, presentes, pendentes, taxa de presença).

---

## 6. Endpoints (mapa completo)

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| POST | `/api/auth/login` | Público | Login da equipe → JWT |
| POST | `/api/auth/register-staff` | Organizer | Cria conta de equipe |
| GET | `/api/courses` | Público | Lista cursos (autocomplete) |
| POST | `/api/courses` | Organizer | Cria curso |
| POST | `/api/events` | Organizer | Cria evento |
| GET | `/api/events` | Público | Lista eventos |
| GET | `/api/events/{id}` | Público | Detalhe do evento |
| POST | `/api/events/{eventId}/registrations` *(multipart)* | Público | Inscrição (com foto) → gera código + e-mail/QR |
| GET | `/api/registrations/{id}/photo` | Staff | Foto da inscrição (nunca pública) |
| GET | `/api/registrations/events/{eventId}` | Staff | Lista de inscritos + busca |
| POST | `/api/registrations/me` | Público | LGPD: titular acessa seus dados |
| DELETE | `/api/registrations/me` | Público | LGPD: titular exclui seus dados |
| GET | `/api/checkin/lookup?code=` | Staff | Passo 1: dados para conferência |
| POST | `/api/checkin/validate` | Staff | Passo 2: confirma entrada, impede reuso |
| GET | `/api/checkin/events/{eventId}/stats` | Staff | Estatísticas de presença |
| GET | `/api/dashboard/events/{id}/by-course` | Staff | Agregado por curso |
| GET | `/api/dashboard/events/{id}/by-semester` | Staff | Agregado por semestre |

Perfis: **Organizer** (cria evento/curso/equipe + tudo de validação) e
**Validator** (só portaria/lookup/validate/stats).

---

## 7. Front-end (telas)

| Rota | Acesso | Conteúdo |
|---|---|---|
| `/` | Público | Lista de eventos |
| `/events/:id` | Público | Detalhe + **formulário de inscrição** (foto, curso, semestre, consentimento) |
| (sucesso) | Público | **QR Code** + botão baixar + código |
| `/my-data` | Público | LGPD: consultar/excluir por código + e-mail |
| `/login` | Público | Login da equipe |
| `/gate` | Staff | **Portaria**: scanner de câmera → card lookup → confirmar |
| `/events/:id/registrations` | Staff | **Lista de inscritos** com busca |
| `/events/:id/dashboard` | Staff | **Dashboard** com gráficos |
| `/events/new` | Organizer | Criar evento |

Bibliotecas novas no front: `qrcode`, `@zxing/browser`, `react-chartjs-2` + `chart.js`.

---

## 8. Segurança e LGPD

**Segurança**
- Senhas com PBKDF2-HMAC-SHA256 + salt (já implementado).
- JWT + RBAC (menor privilégio: validador só valida).
- AccessCode com RNG criptográfico, sem caracteres ambíguos, unicidade em 2 camadas.
- Rate limiting nos endpoints públicos **e no lookup**.
- Cabeçalhos de segurança + tratamento global de erros (sem vazar detalhes).
- **Foto** servida só para equipe autenticada, nunca em endpoint público.
- **Upload de foto** validado: tipo (`jpg`/`png`), tamanho máximo, e (opcional) resize.

**LGPD**
- Consentimento obrigatório e registrado; o texto cita **foto e curso**.
- Minimização: **sem CPF**.
- Direito de acesso e de exclusão por autoatendimento (código + e-mail, no corpo).
- Dados pessoais nunca na URL.
- Trilha de auditoria das validações e dos lookups.

**Segredos**
- A **API key da Brevo** e a **chave JWT** ficam em **User Secrets / variáveis de
  ambiente**, **nunca** no `appsettings.json` versionado. Os valores no repositório
  são placeholders de desenvolvimento.

---

## 9. Cursos da USP (seed)

Será criado um seed com um **conjunto representativo de cursos de graduação da USP**
(de várias unidades). Como o evento é **aberto**, o formulário tem a opção
**"Outro"** (texto livre) para quem não está na lista — o dashboard agrupa esses
casos num balde **"Outros/Externos"**. A equipe (Organizer) pode adicionar novos
cursos via `POST /api/courses`.

---

## 10. Upgrades "profissionais" aplicados

- ✅ **E-mail assíncrono** (fila em segundo plano, tolerante a falha).
- ✅ **Lista de inscritos** com busca (painel da equipe).
- ⏸️ Serilog / Health checks / versionamento de API — fora desta entrega (adicionar
  só se for para produção real).

---

## 11. Como rodar (após implementado)

### API
```bash
cd EventGate
dotnet tool install --global dotnet-ef          # 1x
dotnet ef migrations add EventV2 --project src/EventGate.Api
# segredos de dev (NÃO versionar):
dotnet user-secrets set "Brevo:ApiKey" "SUA_KEY" --project src/EventGate.Api
dotnet user-secrets set "Brevo:SenderEmail" "voce@dominio.com" --project src/EventGate.Api
dotnet run --project src/EventGate.Api
```

### Front
```bash
cd EventGate/web
npm install
npm run dev      # http://localhost:5173
```
`VITE_API_URL` aponta para a API (ex.: `http://localhost:5080`).

Login seed: `admin@eventgate.local` / `Admin@123`.

---

## 12. Checklist de implementação

**Back-end**
- [ ] Entidade `Course` + seed de cursos USP
- [ ] `Registration`: foto, nascimento, curso/semestre (e migration)
- [ ] DTOs novos (inscrição multipart, lookup, dashboard, curso, lista de inscritos)
- [ ] `CoursesController` + serviço
- [ ] Inscrição multipart com upload e validação de foto
- [ ] `GET /registrations/{id}/photo` (staff)
- [ ] `GET /checkin/lookup` (staff) + auditoria
- [ ] `DashboardService` + endpoints por curso/semestre
- [ ] `IQrCodeGenerator` (QRCoder)
- [ ] `IEmailSender` (Brevo) + fila assíncrona (`BackgroundService`)
- [ ] Lista de inscritos com busca
- [ ] Testes (lookup, dashboard, validação de inscrição, email sender mockado)

**Front-end**
- [ ] Formulário de inscrição: upload de foto + preview, autocomplete de curso, semestre
- [ ] Tela de sucesso: QR + baixar
- [ ] Portaria: scanner de câmera + card de lookup + confirmar
- [ ] Página de dashboard com gráficos (Chart.js)
- [ ] Lista de inscritos com busca
- [ ] Tipos e client da API atualizados

---

## 13. Fora de escopo (por agora)

- Criptografia de PII em repouso (foto/e-mail) e job de anonimização/retenção.
- Deploy em nuvem (Azure App Service + Azure SQL).
- Notificações por SMS/WhatsApp.
- Multi-evento com permissões granulares por organizador.

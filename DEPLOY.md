# EventGate — Guia de Deploy

Passo a passo para colocar o EventGate em produção (API + banco + front), com
segurança e e-mail funcionando. Os valores no repositório são **placeholders de
desenvolvimento** — troque todos antes de qualquer uso real.

---

## 1. Pré-requisitos

- **Docker + Docker Compose** (caminho mais simples — sobe API + SQL Server juntos), **ou**
- **.NET 10 SDK** + uma instância de **SQL Server** (local ou Azure SQL).
- **Node 20+** (para gerar o build do front).
- Um domínio com **HTTPS** (obrigatório para a câmera da portaria — ver §5).
- Uma conta **Brevo** (e-mail) — plano gratuito cobre ~300 e-mails/dia.

---

## 2. Subir com Docker Compose (API + SQL Server)

A migration inicial já está versionada (`src/EventGate.Api/Migrations`), então o
schema é criado automaticamente no primeiro start (`MigrateAsync`).

```bash
# na raiz do projeto
export BREVO_API_KEY="sua-chave-brevo"
export BREVO_SENDER_EMAIL="no-reply@seudominio.com"   # precisa ser remetente verificado na Brevo
docker compose up --build
```

- API: `http://localhost:8080` (Swagger em `/swagger`, só em Development).
- O `docker-compose.yml` já liga API ↔ SQL Server e injeta as variáveis do Brevo.

> Em produção real, **não** rode em `Development`. Ajuste `ASPNETCORE_ENVIRONMENT=Production`
> no compose (o Swagger fica desativado por padrão fora de Development).

---

## 3. Trocar os segredos (obrigatório)

Nunca versione segredos reais. Os placeholders a substituir:

| Segredo | Onde (dev) | Como definir em produção |
|---|---|---|
| `Jwt:Key` | `appsettings.json` / compose | variável `Jwt__Key` (mín. 32 chars, aleatória) |
| Senha do SQL | compose (`MSSQL_SA_PASSWORD`) | variável de ambiente / secret |
| `Brevo:ApiKey` | vazio | variável `Brevo__ApiKey` (User Secrets / secret manager) |

Gerar uma chave JWT forte:

```bash
openssl rand -base64 48
```

Local (sem Docker), use **User Secrets** para não tocar no `appsettings`:

```bash
dotnet user-secrets set "Jwt:Key" "<chave-forte>" --project src/EventGate.Api
dotnet user-secrets set "Brevo:ApiKey" "<sua-key>" --project src/EventGate.Api
dotnet user-secrets set "Brevo:SenderEmail" "no-reply@seudominio.com" --project src/EventGate.Api
```

---

## 4. Configurar a Brevo (e-mail)

1. Crie a conta em brevo.com e gere uma **API key** (SMTP & API → API Keys).
2. **Verifique o remetente** (domínio ou e-mail) — a Brevo recusa enviar de
   remetente não verificado.
3. Defina `Brevo__ApiKey` e `Brevo__SenderEmail` (remetente verificado).

> Sem a key configurada, a API **não quebra**: ela apenas registra um aviso e não
> envia o e-mail (a inscrição é concluída normalmente, com código + QR na tela).

---

## 5. HTTPS (obrigatório para a câmera da portaria)

Navegadores só liberam a câmera em `localhost` ou sob **HTTPS**. Para o scanner de
QR funcionar nos celulares da equipe no dia do evento, sirva tudo sob HTTPS.

Forma simples: um **reverse proxy** (Caddy ou Nginx) na frente, com TLS automático.

Exemplo com Caddy (TLS automático via Let's Encrypt):

```
api.seudominio.com {
    reverse_proxy localhost:8080
}
app.seudominio.com {
    root * /var/www/eventgate-web/dist
    file_server
    try_files {path} /index.html
}
```

---

## 6. Front-end (build e hospedagem)

```bash
cd web
echo "VITE_API_URL=https://api.seudominio.com" > .env.production
npm ci
npm run build      # gera web/dist
```

Sirva `web/dist` como site estático (Caddy/Nginx/Vercel/Netlify). É uma SPA —
configure o fallback para `index.html` (qualquer rota → `index.html`).

---

## 7. CORS

A API libera as origens em `Cors:AllowedOrigins`. Adicione o domínio do front em
produção (variável de ambiente):

```
Cors__AllowedOrigins__0=https://app.seudominio.com
```

---

## 8. Primeiro acesso

Login do organizador seed (criado no primeiro start):

```
e-mail: admin@eventgate.local
senha:  Admin@123
```

> **Troque essa senha** assim que possível: entre, crie um novo organizador com
> senha forte na aba **Equipe → Adicionar equipe**, e use a nova conta.

A partir daí: crie o evento, cadastre os palestrantes (aba Equipe → Palestrantes),
adicione os validadores e divulgue a Home.

---

## 9. Checklist de produção

- [ ] `ASPNETCORE_ENVIRONMENT=Production`
- [ ] `Jwt:Key` forte e secreta (não versionada)
- [ ] Senha do SQL trocada
- [ ] `Brevo:ApiKey` + remetente verificado
- [ ] HTTPS na API e no front (câmera + segurança)
- [ ] `Cors:AllowedOrigins` com o domínio de produção
- [ ] Senha do admin seed trocada (ou conta substituída)
- [ ] Backup do banco configurado

---

## Notas de segurança/LGPD ainda em aberto (ver `PLANO.md` §13)

- Criptografia de PII em repouso (foto/e-mail) e job de anonimização/retenção.
- O provider **SQLite** é só para desenvolvimento (`Database__Provider=Sqlite`) e
  carrega um aviso de vulnerabilidade na dependência nativa — **não use em produção**.


# RpgRooms — Blazor Server (.NET 9)

Website para **salas/campanhas de RPG** com **chat em tempo real**, **recrutamento controlado pelo GM** e **limite rígido de 50 jogadores**.

---

## ✨ Funcionalidades
- Criar **campanhas** (nome + descrição). O criador é automaticamente o **GM** (dono).
- **Recrutamento**: somente o **GM** liga/desliga; **fecha automaticamente** ao atingir **50 membros**.
- **Solicitações de participação**: jogadores pedem para entrar; **GM aprova/recusa**.
- **Remoção de jogadores**: somente o GM.
- **Finalização**: somente o GM; campanha vai para estado **Finalized** e o **chat vira somente leitura**.
- **Chat em tempo real (SignalR)** por campanha, com envio usando **nome real** ou **nome de personagem**.

> Regras críticas também são **validadas no servidor** (não só na UI).

---

## 🧱 Stack
- **Blazor Server (.NET 9)** + **ASP.NET Core Identity**
- **Entity Framework Core 9** + **SQLite** (arquivo `rpgrooms.db`)
- **SignalR** para chat
- Minimal APIs para endpoints REST

---

## ✅ Requisitos
- **.NET SDK 9.0.304** (ou compatível com .NET 9)
- Windows 10+ / Linux / macOS (desenvolvido e testado em Windows)

---

## 🗂️ Estrutura de Pastas
```
RpgRooms/
├─ .gitignore
├─ RpgRooms.Core/            # Domínio + Aplicação
│  ├─ Domain/Entities/*.cs
│  ├─ Domain/Enums/*.cs
│  ├─ Application/DTOs/*.cs
│  ├─ Application/Interfaces/*.cs
│  └─ Application/Services/*.cs
├─ RpgRooms.Infrastructure/  # EF Core + Identity
│  └─ Data/AppDbContext.cs
├─ RpgRooms.Web/             # Blazor Server + Identity UI + SignalR + APIs
│  ├─ Program.cs
│  ├─ appsettings.json
│  ├─ appsettings.Development.json
│  ├─ Authorization/*
│  ├─ Data/IdentitySeeder.cs
│  ├─ Endpoints/CampaignEndpoints.cs
│  ├─ Hubs/CampaignChatHub.cs
│  ├─ Pages/*.razor, *Host.cshtml
│  ├─ Shared/*.razor
│  └─ wwwroot/css|js
└─ RpgRooms.Tests/           # xUnit
```

---

## 🚀 Passo a Passo (Dev)
```bash
# 1. (Opcional) clonar o repositório e entrar na pasta
git clone https://github.com/<usuario>/RpgRooms.git
cd RpgRooms

# 2. Restaurar dependências
dotnet restore

# 3. (Opcional) compilar o projeto inteiro
dotnet build

# 4. Executar os testes
dotnet test

# 5. Rodar a aplicação web
cd RpgRooms.Web
dotnet run
```
- Acesse o endereço que o console indicar (ex.: `http://localhost:5000`).
- **Login dev**: `admin` / `admin` (gerado pelo seeder **apenas em Development**).
- O banco **SQLite** será criado como `rpgrooms.db` no diretório de execução.

> Para habilitar HTTPS em desenvolvimento:
> ```bash
> dotnet dev-certs https --trust
> ```

---

## ⚙️ Configuração
**Arquivo**: `RpgRooms.Web/appsettings.json`
```json
{
  "ConnectionStrings": {
    "Default": "Data Source=rpgrooms.db"
  }
}
```
- Por padrão usa **SQLite** via `UseSqlite` (em `Program.cs`).

### Usar SQL Server (opcional)
1) Adicione o pacote:
```powershell
dotnet add RpgRooms.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer
```
2) Troque no `Program.cs`:
```csharp
// builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(cs));
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(cs));
```
3) Atualize a connection string em `appsettings.json`, por ex.:
```json
"ConnectionStrings": {
  "Default": "Server=localhost;Database=RpgRooms;User Id=sa;Password=SuaSenhaForte;TrustServerCertificate=True"
}
```

> **Observação**: o projeto atual utiliza `Database.EnsureCreated()` para simplificar o dev. Para ambientes reais, prefira **migrations** (veja abaixo).

---

## 🗃️ Banco de Dados: EnsureCreated vs Migrations
- O projeto **usa `EnsureCreated()`** e um **seeder** (`admin/admin`) para acelerar o start em Dev.
- Para alternar para **migrations**:
  1. Instale a ferramenta EF (se ainda não tiver):
     ```powershell
     dotnet tool install --global dotnet-ef
     ```
  2. Adicione o pacote de design (no projeto da **Infra** que contém o `AppDbContext`):
     ```powershell
     dotnet add RpgRooms.Infrastructure package Microsoft.EntityFrameworkCore.Design
     ```
  3. Crie a migration e aplique o banco (startup project = Web):
     ```powershell
     dotnet ef migrations add Initial -p RpgRooms.Infrastructure -s RpgRooms.Web
     dotnet ef database update -p RpgRooms.Infrastructure -s RpgRooms.Web
     ```
  4. Em `Program.cs`, substitua `EnsureCreated()` por `Migrate()`:
     ```csharp
     db.Database.Migrate();
     ```

---

## 🔐 Identidade e Usuários
- **ASP.NET Core Identity** com **UI padrão**.
- Em Dev, as regras de senha são afrouxadas (compilação `DEBUG`) para aceitar `admin/admin`.
- Para **produção**:
  - Forte política de senha.
  - Remova o seeder ou use outro mecanismo de criação de admin.
  - Avalie `RequireConfirmedAccount = true`.

---

## 🔌 Endpoints REST (Minimal APIs)
Base: `/api/campaigns` (auth obrigatória salvo onde indicado)

- `POST /api/campaigns` — cria campanha (Auth)
  - body: `{ "name": "nome", "description": "desc" }`
- `PUT /api/campaigns/{id}/recruitment/toggle` — **GM only**; liga/desliga recrutamento
- `PUT /api/campaigns/{id}/finalize` — **GM only**; finaliza e torna o chat read-only
- `POST /api/campaigns/{id}/join-requests` — cria solicitação (se recrutando)
  - body: `{ "message": "opcional" }`
- `PUT /api/campaigns/{id}/join-requests/{reqId}/approve` — **GM only**
- `PUT /api/campaigns/{id}/join-requests/{reqId}/reject` — **GM only**
- `DELETE /api/campaigns/{id}/members/{targetUserId}` — **GM only**; remove jogador
- `GET /api/campaigns` — **anônimo permitido**; filtros: `search`, `recruitingOnly`, `ownerUserId`, `status`
- `GET /api/campaigns/{id}` — **anônimo permitido**; detalhes
- `POST /api/campaigns/{id}/characters` — cria personagem
  - **GM** pode definir `userId` no corpo para criar para outro jogador
  - jogadores comuns sempre usarão o próprio `userId` independentemente do enviado

### Exemplo (PowerShell + `curl`)
```powershell
curl -X POST https://localhost:5001/api/campaigns ^
  -H "Content-Type: application/json" ^
  -b cookies.txt -c cookies.txt ^
  -d "{ "name": "Minha Campanha", "description": "Sessões aos sábados" }"
```

> Use o fluxo de login (`/Identity/Account/Login`) no browser para obter o cookie antes de chamar as APIs autenticadas.

---

## 📡 SignalR — Chat da Campanha
**Hub**: `/hubs/campaign-chat`

- **Server methods** (invocados pelo cliente):
  - `JoinCampaignGroup(Guid campaignId)` — entra no grupo da campanha (precisa ser **membro** ou **GM**).
  - `SendMessage(Guid campaignId, string displayName, string content, bool sentAsCharacter)` — envia mensagem.
- **Client events** (recebidos do servidor):
  - `ReceiveMessage(ChatMessageDto)`
  - `SystemNotice(string)`

**Payload `ChatMessageDto`**:
```json
{
  "id": "GUID",
  "displayName": "Trevor Galhart",
  "content": "Ataque certeiro!",
  "sentAsCharacter": true
}
```

O front já inclui `wwwroot/js/chat.js` que gerencia a conexão.

---

## 🖥️ Fluxo de Uso (UI)
1. Acesse `http://localhost:5000` (ou a porta exibida).
2. **Login** (admin/admin) ou registre um novo usuário.
3. **Crie uma campanha** em `/campaigns/create`.
4. Como **GM**, **ative o recrutamento** (na página da campanha).
5. Outro usuário **solicita participação**; **GM aprova/recusa**.
6. Use o **chat** na página da campanha; marque “Enviar como personagem” se quiser enviar com o nome do personagem.
7. **Finalize** quando acabar; o chat fica **somente leitura**.

---

## 🧪 Testes
```powershell
dotnet test
```
- Inclui teste garantindo o **cap de 50** jogadores.

---

## 🛠️ Troubleshooting
- **Porta em uso (10048/AddressInUse)**: feche o processo que ocupa a porta ou rode com outra URL:
  ```powershell
  set ASPNETCORE_URLS=http://localhost:5058
  dotnet run
  ```
- **Login falhou**: apague `rpgrooms.db` para resetar o banco (perde dados) e rode novamente para reseedar `admin/admin`.
- **Chat não conecta**: verifique se o script do SignalR carrega (CDN liberado), firewall local e se o hub `/hubs/campaign-chat` está acessível.
- **HTTPS dev**: rode `dotnet dev-certs https --trust` e acesse `https://localhost:5001` (porta pode variar).

---

## 🏭 Produção (resumo)
- Troque para **migrations** (`Migrate()`), remova o seeder dev e aplique políticas de senha fortes.
- Use **SQL Server** ou outro provedor gerenciado.
- Configure **reverse proxy** (IIS, Nginx) e **ASPNETCORE_URLS**.
- Publique:
  ```powershell
  dotnet publish RpgRooms.Web -c Release -o publish
  ```

---

## 📌 Roadmap sugerido
- UI mais rica (Bootstrap/Tailwind), paginação e busca avançada.
- Lista de membros com apelido de personagem, e ajustes pelo próprio jogador.
- Logs/Audit detalhados e telas de administração.
- Notificações de moderação no chat.
- Testes adicionais (autorização, chat, fluxos de erro).

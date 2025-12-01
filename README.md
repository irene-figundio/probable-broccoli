# Backend WorkBotAI

API REST in .NET 8 con Entity Framework Core per gestione multi-tenant.

---

## 🚧 Stato

**DA SVILUPPARE** - Backend non ancora implementato.

---

## 📋 Roadmap

### Fase 1: Setup Progetto (Giorno 1-2)
- [ ] Creare progetto .NET 8 Web API
- [ ] Configurare Entity Framework Core
- [ ] Generare Models da database
- [ ] Configurare DbContext
- [ ] Implementare JWT Authentication
- [ ] API `/api/auth/login`
- [ ] API `/api/users` (CRUD)
- [ ] API `/api/tenants` (CRUD)

### Fase 2: API Complete (Giorno 3-4)
- [ ] API Appointments (CRUD)
- [ ] API Services (CRUD)
- [ ] API Staff (CRUD)
- [ ] API Customers (CRUD)
- [ ] API Resources (CRUD)
- [ ] API Payments (CRUD)
- [ ] API Subscriptions (CRUD)
- [ ] Implementare filtro TenantID automatico
- [ ] Validazione input
- [ ] Gestione errori centralizzata

### Fase 3: Funzionalità Avanzate (Giorno 5-6)
- [ ] Sistema permessi granulari
- [ ] Gestione abbonamenti completa
- [ ] Integrazione pagamenti (Stripe opzionale)
- [ ] Export dati (Excel/PDF)
- [ ] Notifiche email
- [ ] Logging e monitoring

---

## 🏗️ Architettura Prevista

```
WorkBotAI.API/
├── Controllers/        # API endpoints
├── Services/          # Business logic
├── Models/            # Entity classes (da DB)
├── DTOs/              # Data Transfer Objects
├── Middlewares/       # JWT, TenantFilter, ExceptionHandler
├── Data/              # DbContext
└── Program.cs         # Configuration
```

---

## 🔧 Stack Tecnologico

- **.NET:** 8.0
- **ORM:** Entity Framework Core
- **Auth:** JWT Bearer Token
- **Validation:** FluentValidation
- **Logging:** Serilog
- **API Docs:** Swagger/OpenAPI

---

## 🚀 Setup (Quando sarà pronto)

### Prerequisiti

- .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0
- Database WorkBotAI già creato (vedi `/database`)

### Installazione

```bash
cd backend/WorkBotAI.API

# Restore pacchetti
dotnet restore

# Build progetto
dotnet build

# Run development
dotnet run
```

Backend disponibile su:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- Swagger: http://localhost:5000/swagger

---

## 🔑 Configurazione

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=WorkBotAI;User Id=sa;Password=WorkBot@2024!;TrustServerCertificate=true;"
  },
  "Jwt": {
    "Key": "your-secret-key-min-32-chars",
    "Issuer": "WorkBotAI",
    "Audience": "WorkBotAI-Clients",
    "ExpiryMinutes": 60
  }
}
```

⚠️ **Non committare password reali!** Usa User Secrets o variabili ambiente.

---

## 📡 API Endpoints (Pianificati)

### Authentication
```
POST   /api/auth/login          # Login utente
POST   /api/auth/register       # Registrazione (solo SuperAdmin)
POST   /api/auth/refresh        # Refresh token
POST   /api/auth/logout         # Logout
```

### Users
```
GET    /api/users               # Lista utenti (filtrato per tenant)
GET    /api/users/{id}          # Dettaglio utente
POST   /api/users               # Crea utente
PUT    /api/users/{id}          # Aggiorna utente
DELETE /api/users/{id}          # Elimina utente (soft delete)
```

### Tenants (Solo SuperAdmin)
```
GET    /api/tenants             # Lista tenant
GET    /api/tenants/{id}        # Dettaglio tenant
POST   /api/tenants             # Crea tenant
PUT    /api/tenants/{id}        # Aggiorna tenant
DELETE /api/tenants/{id}        # Disattiva tenant
```

### Appointments
```
GET    /api/appointments                    # Lista appuntamenti
GET    /api/appointments/{id}               # Dettaglio appuntamento
POST   /api/appointments                    # Crea appuntamento
PUT    /api/appointments/{id}               # Aggiorna appuntamento
DELETE /api/appointments/{id}               # Cancella appuntamento
GET    /api/appointments/customer/{custId}  # Appuntamenti cliente
```

### Services
```
GET    /api/services            # Lista servizi tenant
GET    /api/services/{id}       # Dettaglio servizio
POST   /api/services            # Crea servizio
PUT    /api/services/{id}       # Aggiorna servizio
DELETE /api/services/{id}       # Elimina servizio
```

### Staff
```
GET    /api/staff               # Lista staff tenant
GET    /api/staff/{id}          # Dettaglio staff
POST   /api/staff               # Crea staff
PUT    /api/staff/{id}          # Aggiorna staff
DELETE /api/staff/{id}          # Elimina staff
```

### Customers
```
GET    /api/customers           # Lista clienti tenant
GET    /api/customers/{id}      # Dettaglio cliente
POST   /api/customers           # Crea cliente
PUT    /api/customers/{id}      # Aggiorna cliente
DELETE /api/customers/{id}      # Elimina cliente
```

### Payments
```
GET    /api/payments            # Lista pagamenti
POST   /api/payments            # Registra pagamento
GET    /api/payments/appointment/{id}  # Pagamenti appuntamento
```

### Subscriptions
```
GET    /api/subscriptions           # Lista abbonamenti
GET    /api/subscriptions/tenant/{id}  # Abbonamento tenant
POST   /api/subscriptions           # Crea abbonamento
PUT    /api/subscriptions/{id}      # Aggiorna abbonamento
```

---

## 🔐 Autenticazione

### JWT Token

**Login Request:**
```json
POST /api/auth/login
{
  "username": "admin@workbotai.com",
  "password": "Admin@123"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiresIn": 3600,
  "user": {
    "id": 1,
    "username": "admin@workbotai.com",
    "role": "TenantAdmin",
    "tenantId": "abc-123-def-456"
  }
}
```

**Uso Token:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

---

## 🏢 Multi-Tenant Filter

Ogni richiesta API automaticamente filtrata per `TenantID` dell'utente loggato.

**Middleware automatico:**
```csharp
// Ogni query EF Core automaticamente include:
.Where(x => x.TenantID == currentUser.TenantID)
```

**SuperAdmin bypassa filtro** (vede tutti i tenant).

---

## 🧪 Testing

```bash
# Unit tests
dotnet test

# Run con coverage
dotnet test /p:CollectCoverage=true
```

---

## 📦 Deploy

### Development
```bash
dotnet run
```

### Production

**Railway/Render:**
1. Connetti repository GitHub
2. Configura variabili ambiente
3. Deploy automatico da `main` branch

**Docker:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY bin/Release/net8.0/publish/ /app
WORKDIR /app
EXPOSE 5000
ENTRYPOINT ["dotnet", "WorkBotAI.API.dll"]
```

---

## 📚 Risorse

- [.NET 8 Docs](https://docs.microsoft.com/dotnet)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [JWT Authentication](https://jwt.io)
- [ASP.NET Core Web API](https://docs.microsoft.com/aspnet/core/web-api)

---

**Sviluppo previsto:** Dicembre 2024  
**Deploy produzione:** Gennaio 2025

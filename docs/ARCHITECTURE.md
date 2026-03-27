# BootManager V2 - Technische Architectuur & Procesdocumentatie

**Laatste update:** maart 2026  
**Status:** Werk in uitvoering (feature/NetwerkData/Interpretation)  
**Target Framework:** .NET 8  
**Platform:** Blazor WebAssembly / ASP.NET Core

---

## Inhoudsopgave

1. [Overzicht](#overzicht)
2. [Oplossingsstructuur](#oplossingsstructuur)
3. [Laagindeling (Layered Architecture)](#laagindeling-layered-architecture)
4. [Kernentiteiten & Value Objects](#kernentiteiten--value-objects)
5. [Dataflow & Processen](#dataflow--processen)
6. [Dependency Injection & Services](#dependency-injection--services)
7. [Repositories & Data Access](#repositories--data-access)
8. [API-structuur](#api-structuur)
9. [Communicatie met externe systemen](#communicatie-met-externe-systemen)
10. [Best Practices & Conventies](#best-practices--conventies)
11. [Toekomstige Uitbreidingen](#toekomstige-uitbreidingen)
12. [Troubleshooting & Logging](#troubleshooting--logging)

---

## Overzicht

BootManager V2 is een .NET 8-applicatie gebouwd met **Blazor** voor het beheren van boot-operaties. De applicatie verzamelt, interpreteert en opslaat network-data van maritieme sensoren (batterij, diepte, beweging, wind) en biedt een backend-API voor data-integratie.

### Kernfunctionaliteiten

- ✅ Eigenaar-beheer en authenticatie (Email/Password + JWT)
- ✅ Boot-registratie en -beheer
- ✅ Netwerkbericht-ingestion (binary payloads)
- ✅ Semantische interpretatie per berichttype
- ✅ Sensor-metingen (batterij, diepte, beweging, wind)
- ✅ Data-persistentie via Entity Framework Core (SQL Server/PostgreSQL)
- ✅ RESTful API-endpoints
- ✅ Blazor UI voor dashboard en instellingen

### Technische Stack

| Component | Technologie |
|-----------|-------------|
| Framework | .NET 8 |
| Web | Blazor WebAssembly / ASP.NET Core |
| ORM | Entity Framework Core 8 |
| Database | SQL Server / PostgreSQL |
| Authentication | JWT Bearer tokens |
| API-style | RESTful (JSON) |
| Dependency Injection | Microsoft.Extensions.DependencyInjection |

---

## Oplossingsstructuur

```
BootManagerV2/
├── BootManager.Core/
│   ├── Entities/
│   ├── Interfaces/
│   └── ValueObjects/
│
├── BootManager.Application/
│   ├── Authentication/
│   ├── OwnerRegistration/
│   ├── NetworkMessages/
│   ├── NetworkMessageParsing/
│   ├── NetworkMessageInterpretation/
│   ├── BatteryMeasurements/
│   ├── DepthMeasurements/
│   ├── MotionMeasurements/
│   ├── WindMeasurements/
│   └── DependencyInjection.cs
│
├── BootManager.Infrastructure/
│   ├── Repositories/
│   ├── Configurations/
│   ├── Migrations/
│   └── DatabaseContext.cs
│
├── BootManager.Web/
│   ├── Controllers/
│   ├── Components/
│   ├── Services/
│   └── Program.cs
│
├── src/BootManager.Tools.Simulator/
│   └── Scenarios/
│
├── src/BootManager.Tools.Ingest/
│   └── Data ingest utilities
│
├── BootManager.UnitTests/
├── BootManager.IntegrationTests/
├── docs/
│   └── ARCHITECTURE.md (dit bestand)
│
└── BootManager.sln
```

---

## Laagindeling (Layered Architecture)

BootManager volgt een **4-laags architectuur** met strikte scheidingslijnen:

```
┌─────────────────────────────────┐
│  Presentatielaag                │
│  (BootManager.Web)              │
│  - Controllers                  │
│  - Blazor Components            │
│  - Authorization Middleware     │
└──────────┬──────────────────────┘
           │ HTTP/REST
┌──────────▼──────────────────────┐
│  Applicatielaag                 │
│  (BootManager.Application)      │
│  - Services (IOwnerLogin, etc)  │
│  - Interpreters (MessageType)   │
│  - DTOs                         │
│  - Contracts/Interfaces         │
└──────────┬──────────────────────┘
           │ IRepository<T>
┌──────────▼──────────────────────┐
│  Infrastructuurlaag             │
│  (BootManager.Infrastructure)   │
│  - EfRepository<T>              │
│  - EF Core Configurations       │
│  - DatabaseContext              │
│  - Migrations                   │
└──────────┬──────────────────────┘
           │ DbSet<T>
┌──────────▼──────────────────────┐
│  Kernlaag                       │
│  (BootManager.Core)             │
│  - Entities (Owner, Boat, etc)  │
│  - IRepository<T> interface     │
│  - Value Objects                │
│  - Domain Logic                 │
└─────────────────────────────────┘
```

### 1. **Presentatielaag (BootManager.Web)**

**Verantwoordelijkheid:** HTTP-API eindpunten, Blazor-webinterface, request-handling

**Inhoud:**
- `Controllers/` - ASP.NET Core API-controllers (slim, delegates aan services)
- `Components/` - Blazor-componenten voor UI
- `Program.cs` - Startup configuratie, middleware setup, DI-registratie
- Authorization-headers en JWT-validation

**Dataflow:**
```
HTTP Request → Route → Controller → Service → Repository → Response
```

**Voorbeeld - AuthController:**
```csharp
[HttpPost("login")]
public async Task<ActionResult<OwnerLoginResponseDto>> Login(
    OwnerLoginRequestDto request)
{
    var result = await _ownerLoginService.AuthenticateAsync(
        request.Email, request.Password);
    return Ok(result);
}
```

### 2. **Applicatielaag (BootManager.Application)**

**Verantwoordelijkheid:** Business logic, data-transformatie, orchestratie, interpreters

**Inhoud:**

#### Feature-structuur (exemplarisch):
```
Authentication/
├── Contracts/
│   └── IOwnerLoginService.cs
├── DTOs/
│   ├── OwnerLoginRequestDto.cs
│   └── OwnerLoginResponseDto.cs
└── Services/
    ├── OwnerLoginService.cs
    ├── OwnerRegistrationService.cs
    └── OwnerRecoveryService.cs
```

#### Interpreters (Core Component):
```
NetworkMessageInterpretation/
├── Contracts/
│   └── INetworkMessageInterpreter<T>.cs
├── DTOs/
│   ├── BatteryMessageInterpretationDto.cs
│   ├── MotionMessageInterpretationDto.cs
│   └── ...
└── Services/
    ├── BatteryMessageInterpreterService.cs
    ├── MotionMessageInterpreterService.cs
    └── ...
```

**Kernpatroon - Interpreter Generic Interface:**
```csharp
public interface INetworkMessageInterpreter<T> where T : class
{
    /// <summary>
    /// Interpreteert binaire payload naar semantische DTO
    /// </summary>
    T Interpret(IEnumerable<byte> payload);
}
```

**Dependency Injection Registratie (DependencyInjection.cs):**
```csharp
services.AddScoped<INetworkMessageService, NetworkMessageService>();
services.AddScoped<IBatteryMeasurementService, BatteryMeasurementService>();

services.AddTransient<INetworkMessageInterpreter<BatteryMessageInterpretationDto>, 
    BatteryMessageInterpreterService>();
services.AddTransient<INetworkMessageInterpreter<MotionMessageInterpretationDto>, 
    MotionMessageInterpreterService>();
```

### 3. **Infrastructuurlaag (BootManager.Infrastructure)**

**Verantwoordelijkheid:** Database-persistentie, EF Core-konfiguratie, data-mapping

**Inhoud:**
- `DatabaseContext.cs` - DbContext van EF Core
- `Repositories/EfRepository.cs` - Generieke repository implementatie
- `Configurations/` - EF Core EntityTypeConfiguration<T>
- `Migrations/` - EF Core database-migraties

**Repository-implementatie:**
```csharp
public class EfRepository<T> : IRepository<T> where T : class
{
    private readonly DatabaseContext _context;
    private readonly DbSet<T> _dbSet;
    
    public async Task AddAsync(T entity)
    {
        _dbSet.Add(entity);
        await SaveChangesAsync();
    }
    
    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }
    
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
    // ... overige CRUD-operaties
}
```

**Database-registratie (Program.cs):**
```csharp
services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlServer(connection));
    
services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
```

### 4. **Kernlaag (BootManager.Core)**

**Verantwoordelijkheid:** Domeinlogica, entiteiten, interfaces

**Interfaces:**
```csharp
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(Guid id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task SaveChangesAsync();
}
```

**Entiteiten:** Owner, Boat, NetworkMessage, BatteryMeasurement, etc.

---

## Kernentiteiten & Value Objects

### Domeinentiteiten

#### **Owner** (Eigenaar van boot)
```
Owner
├── Id (Guid, PK)
├── Email (string, unique, indexed)
├── PasswordHash (string)
├── FirstName (string)
├── LastName (string)
├── Created (DateTime)
├── LastModified (DateTime)
└── Boats (ICollection<Boat>, Navigation)
    └── OneToMany relationship
```

**Lifecycle:** Registratie → Login → Settings Update → Logout

---

#### **Boat** (Boot)
```
Boat
├── Id (Guid, PK)
├── OwnerId (Guid, FK → Owner)
├── Name (string)
├── CallSign (string, unique)
├── Created (DateTime)
├── LastModified (DateTime)
├── Owner (Owner, Navigation)
├── NetworkMessages (ICollection<NetworkMessage>)
├── BatteryMeasurements (ICollection<BatteryMeasurement>)
├── DepthMeasurements (ICollection<DepthMeasurement>)
├── MotionMeasurements (ICollection<MotionMeasurement>)
└── WindMeasurements (ICollection<WindMeasurement>)
```

**Relationships:**
- 1:N naar Owner (Many boats per owner)
- 1:N naar Measurements (Many measurements per boat)

---

#### **NetworkMessage** (Onbewerkt netwerkbericht)
```
NetworkMessage
├── Id (Guid, PK)
├── BoatId (Guid, FK → Boat)
├── RawPayload (byte[], binary data)
├── MessageType (enum: Battery|Depth|Motion|Wind)
├── Timestamp (DateTime)
├── Boat (Boat, Navigation)
└── [Type-specific properties]
    ├── BatteryVoltage (mV) [if Battery]
    ├── DepthReading (m) [if Depth]
    ├── COG (radialen) + SOG (knopen) [if Motion]
    └── WindSpeed + WindDir [if Wind]
```

**Lifecycle:**
```
Created (POST) → Parsed → Interpreted → Measurement stored
```

---

#### **BatteryMeasurement**
```
BatteryMeasurement
├── Id (Guid, PK)
├── BoatId (Guid, FK)
├── Voltage (uint16, mV)
├── Timestamp (DateTime)
└── Boat (Boat, Navigation)
```

**Source:** `INetworkMessageInterpreter<BatteryMessageInterpretationDto>`

---

#### **MotionMeasurement**
```
MotionMeasurement
├── Id (Guid, PK)
├── BoatId (Guid, FK)
├── COG (uint16, radialen 0-65535 = 0-360°)
├── SOG (uint16, centiknopen)
├── Timestamp (DateTime)
└── Boat (Boat, Navigation)
```

**Source:** `INetworkMessageInterpreter<MotionMessageInterpretationDto>`

---

### Value Objects

#### **MessagePayload**
- Binaire payload van netwerkberichten
- Conversie: Little-endian per berichttype
- Immutable na creatie

#### **CoordinateRadial** (Motion)
- Type: `uint16`
- Bereik: 0-65535 (=0-360 graden)
- Eenheid: Radialen (1/65536 graden per unit)

#### **Velocity** (Motion)
- Type: `uint16`
- Bereik: 0-65535
- Eenheid: Centiknoten (cm/knoop)

#### **ElectricalPotential** (Battery)
- Type: `uint16`
- Bereik: 0-65535
- Eenheid: Millivolts (mV)

---

## Dataflow & Processen

### 1. **Registratie & Authenticatie Flow**

#### **Registratie:**
```
User Registration Request
    ↓
POST /api/auth/register
    ↓
AuthController.Register()
    ↓
IOwnerRegistrationService.RegisterAsync()
    │
    ├─ Validate email (unique)
    ├─ Hash password (BCrypt)
    ├─ Create Owner entity
    └─ Repository.AddAsync()
    ↓
Database: INSERT Owner
    ↓
201 Created + JWT Token
```

#### **Login:**
```
User Login Request (Email, Password)
    ↓
POST /api/auth/login
    ↓
AuthController.Login()
    ↓
IOwnerLoginService.AuthenticateAsync()
    │
    ├─ Fetch Owner by email
    ├─ Verify password hash
    ├─ Generate JWT (exp: 1 hour)
    └─ Return token + refresh token
    ↓
200 OK + { token, refreshToken }
    ↓
Client stores token in Authorization header
```

**Betrokken bestanden:**
- `BootManager.Web/Controllers/AuthController.cs`
- `BootManager.Application/Authentication/Services/OwnerLoginService.cs`
- `BootManager.Application/OwnerRegistration/Services/OwnerRegistrationService.cs`

---

### 2. **NetworkMessage Ingestion & Interpretation Pipeline** (KERN)

Dit is het kernproces voor maritieme sensor-data verwerking:

```
┌─────────────────────────────────────────────────────┐
│  EXTERNE BRON (NMEA/Simulator/Hardware)             │
│  Raw binary payload                                  │
└────────────┬────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────┐
│  INGESTION FASE                                     │
│  POST /api/networkMessages                          │
│  Controller: NetworkMessagesController              │
│  - Receives: { boatId, payload, messageType }       │
└────────────┬────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────┐
│  PARSING FASE                                       │
│  INetworkMessageParserService.ParseAsync()          │
│  - Creates NetworkMessage entity                    │
│  - Stores raw payload in DB                         │
│  - Returns: NetworkMessage + messageType            │
└────────────┬────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────┐
│  INTERPRETATION FASE (KEY BUSINESS LOGIC)           │
│  Route to correct INetworkMessageInterpreter<T>     │
│                                                      │
│  IF Battery:                                        │
│  ├─ BatteryMessageInterpreterService.Interpret()   │
│  ├─ Payload: 2 bytes → uint16 (little-endian)      │
│  └─ Output: BatteryMessageInterpretationDto        │
│                                                      │
│  IF Motion:                                         │
│  ├─ MotionMessageInterpreterService.Interpret()    │
│  ├─ Payload: 4 bytes                               │
│  │  └─ Bytes[0-1]: COG (uint16 LE)                 │
│  │  └─ Bytes[2-3]: SOG (uint16 LE)                 │
│  └─ Output: MotionMessageInterpretationDto         │
│                                                      │
│  IF Depth/Wind: (similar pattern)                  │
│                                                      │
└────────────┬────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────┐
│  STORAGE FASE                                       │
│  IMeasurementService.CreateAsync()                 │
│  - Maps DTO → Measurement entity                    │
│  - Repository.AddAsync()                           │
│  - Commit to database                              │
│                                                      │
│  Stored entities:                                   │
│  ├─ BatteryMeasurement (if Battery)                │
│  ├─ MotionMeasurement (if Motion)                  │
│  ├─ DepthMeasurement (if Depth)                    │
│  └─ WindMeasurement (if Wind)                      │
└────────────┬────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────┐
│  RESPONSE                                           │
│  200 OK + MeasurementResponseDto                    │
└─────────────────────────────────────────────────────┘
```

**Kritieke implementatiedetails:**

#### **BatteryMessageInterpreterService**
```csharp
public BatteryMessageInterpretationDto Interpret(IEnumerable<byte> payload)
{
    var bytes = payload.ToArray();
    if (bytes.Length != 2)
        throw new InvalidOperationException("Battery payload moet 2 bytes zijn");
    
    // Little-endian conversie
    ushort voltage = BitConverter.ToUInt16(bytes, 0);
    
    return new BatteryMessageInterpretationDto { Voltage = voltage };
}
```

#### **MotionMessageInterpreterService**
```csharp
public MotionMessageInterpretationDto Interpret(IEnumerable<byte> payload)
{
    var bytes = payload.ToArray();
    if (bytes.Length != 4)
        throw new InvalidOperationException("Motion payload moet 4 bytes zijn");
    
    // Little-endian conversie
    ushort cog = BitConverter.ToUInt16(bytes, 0);  // Bytes 0-1: COG radialen
    ushort sog = BitConverter.ToUInt16(bytes, 2);  // Bytes 2-3: SOG centiknopen
    
    return new MotionMessageInterpretationDto 
    { 
        COG = cog,
        SOG = sog 
    };
}
```

**Betrokken bestanden:**
- `BootManager.Web/Controllers/NetworkMessagesController.cs`
- `BootManager.Application/NetworkMessages/Services/NetworkMessageService.cs`
- `BootManager.Application/NetworkMessageInterpretation/Services/*.cs`
- `BootManager.Application/[MeasurementType]/Services/`

---

### 3. **Data Retrieval Flow**

```
GET /api/batteryMeasurements?boatId={id}
    ↓
BatteryMeasurementsController.GetByBoat()
    ↓
IBatteryMeasurementService.GetByBoatAsync()
    ↓
Repository.GetAllAsync()
    ↓
EF Core Query: SELECT * FROM BatteryMeasurements WHERE BoatId = {id}
    ↓
Database → DbSet<BatteryMeasurement>
    ↓
Map to DTO
    ↓
200 OK + List<BatteryMeasurementDto>
```

---

## Dependency Injection & Services

### DI-Container Setup

**Registratie-flow (Program.cs):**

```csharp
// 1. Add DbContext
services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

// 2. Add generieke Repository
services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

// 3. Add Application Services
services.AddApplicationServices();
```

**DependencyInjection.cs (Application Layer):**

```csharp
public static IServiceCollection AddApplicationServices(
    this IServiceCollection services)
{
    // Authentication Services
    services.AddScoped<IOwnerRegistrationService, OwnerRegistrationService>();
    services.AddScoped<IOwnerLoginService, OwnerLoginService>();
    
    // NetworkMessage Services
    services.AddScoped<INetworkMessageService, NetworkMessageService>();
    services.AddScoped<INetworkMessageParserService, NetworkMessageParserService>();
    
    // Interpreters (Transient: stateless, één per use)
    services.AddTransient<INetworkMessageInterpreter<BatteryMessageInterpretationDto>, 
        BatteryMessageInterpreterService>();
    services.AddTransient<INetworkMessageInterpreter<MotionMessageInterpretationDto>, 
        MotionMessageInterpreterService>();
    
    // Measurement Services
    services.AddScoped<IBatteryMeasurementService, BatteryMeasurementService>();
    services.AddScoped<IMotionMeasurementService, MotionMeasurementService>();
    
    return services;
}
```

### Lifetime Policies

| Lifetime | Gebruik | Wanneer | Voorbeeld |
|----------|---------|--------|----------|
| **Singleton** | 1 instance per app | App-wide state, thread-safe | Configuration, Logger factory |
| **Scoped** | 1 instance per request | HTTP-request context | Services, DbContext |
| **Transient** | Nieuwe instance telkens | Stateless, lightweight | Interpreters |

### Service Constructor Injection

```csharp
public class BatteryMeasurementService : IBatteryMeasurementService
{
    private readonly IRepository<BatteryMeasurement> _repository;
    
    public BatteryMeasurementService(IRepository<BatteryMeasurement> repository)
    {
        _repository = repository;  // Inject via constructor
    }
}
```

---

## Repositories & Data Access

### Generieke Repository Pattern

**Interface (Core):**
```csharp
namespace BootManager.Core.Interfaces;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(Guid id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task SaveChangesAsync();
}
```

**Implementatie (Infrastructure):**
```csharp
namespace BootManager.Infrastructure.Repositories;

public class EfRepository<T> : IRepository<T> where T : class
{
    private readonly DatabaseContext _context;
    private readonly DbSet<T> _dbSet;
    
    public EfRepository(DatabaseContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }
    
    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }
    
    public async Task AddAsync(T entity)
    {
        _dbSet.Add(entity);
        await SaveChangesAsync();
    }
    
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
```

### Registratie & Use

```csharp
// Registratie (Program.cs)
services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

// Gebruik in Service
public BatteryMeasurementService(IRepository<BatteryMeasurement> repo)
{
    _repository = repo;
}

// CRUD-operatie
await _repository.AddAsync(new BatteryMeasurement { ... });
```

### Voordelen

✅ DRY (Don't Repeat Yourself) - geen CRUD-code per entiteit  
✅ Testbaarheid - makkelijk mockable via interface  
✅ Consistentie - uniforme data-access pattern  
✅ Flexibiliteit - eenvoudig uit te breiden

---

## API-structuur

### Base URL

```
https://[server]/api/
```

### Authentication Endpoints

| Method | Endpoint | Request | Response | Beschrijving |
|--------|----------|---------|----------|-------------|
| **POST** | `/auth/register` | `{ email, password, firstName, lastName }` | `{ token, refreshToken }` | Registreer nieuwe eigenaar |
| **POST** | `/auth/login` | `{ email, password }` | `{ token, refreshToken }` | Login met credentials |
| **POST** | `/auth/refresh` | `{ refreshToken }` | `{ token }` | Verlenging van token |
| **POST** | `/auth/recover` | `{ email }` | `{ message }` | Wachtwoord herstellen |

### NetworkMessage Endpoints

| Method | Endpoint | Request | Response | Beschrijving |
|--------|----------|---------|----------|-------------|
| **POST** | `/networkMessages` | `{ boatId, payload, messageType }` | `MeasurementResponseDto` | Voeg bericht toe + interpret |
| **GET** | `/networkMessages` | Query params: `boatId`, `messageType` | `List<NetworkMessageDto>` | Lijst alle berichten |
| **GET** | `/networkMessages/{id}` | - | `NetworkMessageDto` | Specifiek bericht |

### Measurement Endpoints

#### Battery
| Method | Endpoint | Beschrijving |
|--------|----------|-------------|
| **POST** | `/batteryMeasurements` | Voeg batterij-meting toe |
| **GET** | `/batteryMeasurements` | Haal alle metingen op (gefilterd: `?boatId=`) |
| **GET** | `/batteryMeasurements/{id}` | Specifieke meting |

#### Motion
| Method | Endpoint | Beschrijving |
|--------|----------|-------------|
| **POST** | `/motionMeasurements` | Voeg beweging-meting toe |
| **GET** | `/motionMeasurements` | Haal alle metingen op |
| **GET** | `/motionMeasurements/{id}` | Specifieke meting |

#### Depth, Wind
(identiek patroon per type)

### Request/Response Voorbeelden

#### Login Request
```json
POST /api/auth/login
Content-Type: application/json

{
  "email": "captain@vessel.nl",
  "password": "SecurePass123!"
}
```

#### Login Response
```json
200 OK

{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "abc123xyz789...",
  "expiresIn": 3600
}
```

#### NetworkMessage Ingest (Motion)
```json
POST /api/networkMessages
Authorization: Bearer <token>
Content-Type: application/json

{
  "boatId": "550e8400-e29b-41d4-a716-446655440000",
  "messageType": "Motion",
  "payload": [0x45, 0x12, 0xCD, 0xAB]
}
```

#### MotionMeasurement Response
```json
201 Created

{
  "id": "660e8400-e29b-41d4-a716-446655440001",
  "boatId": "550e8400-e29b-41d4-a716-446655440000",
  "cog": 4677,
  "sog": 43981,
  "timestamp": "2026-03-15T14:30:00Z"
}
```

---

## Communicatie met externe systemen

### NMEA-Data Ingest

**Huidige aanpak:** Manual/programmatic POST naar `/api/networkMessages`

**Flow:**
```
NMEA Source (GPS, Wireless Modem, etc.)
    ↓
[Convert to binary payload format]
    ↓
HTTP POST /api/networkMessages
    {
        boatId: <guid>,
        messageType: <type>,
        payload: <bytes>
    }
    ↓
Parser validates + creates NetworkMessage
    ↓
Interpreter processes payload per type
    ↓
Measurement stored in database
```

### Simulator (BootManager.Tools.Simulator)

**Doel:** Scenario-based NMEA-data generatie en API-test

**Inhoud:**
- Scenario-templates (realistic sensor-sequences)
- Payload generation per message-type
- HTTP-client voor API-calls

**Use-case:**
```
BootManager.Tools.Simulator
    └─ Runs scenario: "Sailing in force-3 wind"
    └─ Generates Motion/Wind/Battery messages
    └─ POSTs to /api/networkMessages
    └─ Validates responses
```

### Toekomstige Integraties

1. **Message Queue:** RabbitMQ, Azure Service Bus (high-volume ingest)
2. **Real-time WebSocket:** Blazor UI live updates
3. **IoT Hub:** Azure IoT Hub integratie
4. **Logging Service:** Application Insights

---

## Best Practices & Conventies

### Naamgeving

| Element | Conventie | Voorbeeld |
|---------|-----------|----------|
| Projects | `BootManager.*` | `BootManager.Core`, `BootManager.Web` |
| Namespaces | Feature-based hierarchie | `BootManager.Application.Authentication` |
| Services | `I[Name]Service` (interface) + `[Name]Service` (class) | `IOwnerLoginService`, `OwnerLoginService` |
| DTOs | `[Entity][Purpose]Dto` | `MotionMessageInterpretationDto` |
| Repositories | `IRepository<T>` (generic) | `IRepository<BatteryMeasurement>` |
| Controllers | `[PluralEntity]Controller` | `NetworkMessagesController` |
| Features | Singular entity name | `Authentication/`, `BatteryMeasurements/` |

### Code-organisatie - Feature-Layout

```
BootManager.Application/
├── [Feature]/
│   ├── Services/
│   │   └── I[Feature]Service.cs + [Feature]Service.cs
│   ├── DTOs/
│   │   └── [Entity][Purpose]Dto.cs
│   └── Contracts/
│       └── Interfaces (optional, if shared)
```

**Voorbeeld Authentication:**
```
Authentication/
├── Services/
│   ├── IOwnerLoginService.cs
│   ├── OwnerLoginService.cs
│   ├── IOwnerRegistrationService.cs
│   ├── OwnerRegistrationService.cs
│   ├── IOwnerRecoveryService.cs
│   └── OwnerRecoveryService.cs
├── DTOs/
│   ├── OwnerLoginRequestDto.cs
│   ├── OwnerLoginResponseDto.cs
│   └── OwnerRegistrationRequestDto.cs
```

### SOLID-principes

#### **S** - Single Responsibility Principle
```csharp
// ✅ Goed: Één reden om te veranderen
public class MotionMessageInterpreterService 
    : INetworkMessageInterpreter<MotionMessageInterpretationDto>
{
    // Enkel motion-payload interpreteren
}

// ❌ Slecht: Meerdere verantwoordelijkheden
public class AllInterpreterService
{
    // Battery + Motion + Depth + Wind = te veel
}
```

#### **D** - Dependency Inversion Principle
```csharp
// ✅ Goed: Injecteer interface
public class BatteryMeasurementService(IRepository<BatteryMeasurement> repo)
{
    await repo.AddAsync(entity);
}

// ❌ Slecht: Direct instantiëren
public class BatteryMeasurementService
{
    var repo = new EfRepository<BatteryMeasurement>();  // Slecht!
}
```

#### **O** - Open/Closed Principle
```csharp
// ✅ Goed: Open voor extensie, gesloten voor aanpassing
// Voeg WaveMessageInterpreterService toe zonder bestaande code aan te passen
services.AddTransient<INetworkMessageInterpreter<WaveMessageInterpretationDto>, 
    WaveMessageInterpreterService>();
```

### Coding Style

- **Constructor Injection** Always - geen static instances
- **Async/Await** - async repositories, async controllers
- **Null-checks** - use `?.` operator, null-coalescing `??`
- **Exception Handling** - custom exceptions in Application layer
- **Logging** - ILogger via DI, log at appropriate levels (Info/Warning/Error)

### XML-documentatie

```csharp
/// <summary>
/// Interpreteert binaire motion-payload naar semantische data.
/// </summary>
/// <param name="payload">
/// 4-byte little-endian payload: bytes[0-1]=COG, bytes[2-3]=SOG
/// </param>
/// <returns>
/// DTO met interpreted Course-over-Ground en Speed-over-Ground
/// </returns>
/// <exception cref="InvalidOperationException">
/// Thrown als payload niet exact 4 bytes
/// </exception>
public MotionMessageInterpretationDto Interpret(IEnumerable<byte> payload)
{
    // implementatie
}
```

### Comments

**Voeg toe:**
- Complex algoritmes (waarom, niet wat)
- Non-obvious domain logic
- References naar externe specs/docs

**Niet nodig:**
- Duidelijke code (`var user = await GetUserAsync();` → self-explanatory)
- Getters/setters
- CRUD-operaties

---

## Toekomstige Uitbreidingen

### Korte Termijn (Sprint 1-2)

- [ ] Blazor UI componenten
- [ ] Wind-meting implementatie
- [ ] Unit-tests uitbreiden

### Middellange Termijn (Sprint 3-5)

- [ ] Event Sourcing (audit trail)
- [ ] CQRS pattern (read/write split)
- [ ] Real-time WebSocket updates
- [ ] Datavisualisatie dashboard

### Lange Termijn

- [ ] Async Message Queue (RabbitMQ / Azure Service Bus)
- [ ] IoT Hub integratie
- [ ] Machine Learning (anomaly detection)
- [ ] Mobile app (native iOS/Android)
- [ ] Multi-tenancy support

---

## Troubleshooting & Logging

### Veelgemaakte Fouten

| Fout | Oorzaak | Oplossing |
|------|---------|----------|
| **DbContext not registered** | Missing `AddDbContext()` in Program.cs | Voeg database-registratie toe in Program.cs |
| **IRepository not found** | Missing `services.AddScoped(typeof(IRepository<>), ...)` | Registreer generieke repository |
| **Invalid JWT token** | Token verlopen / fout secret | Refresh token, herlogin |
| **Motion interpretation fails** | Payload niet exact 4 bytes | Valideer berichtbron |
| **NullReferenceException in Service** | Forgot to inject dependency | Controleer constructor parameters |

### Logging

```csharp
public class BatteryMeasurementService(
    IRepository<BatteryMeasurement> repo,
    ILogger<BatteryMeasurementService> logger)
{
    public async Task CreateAsync(BatteryMeasurementCreateDto dto)
    {
        logger.LogInformation("Creating battery measurement for boat {BoatId}", 
            dto.BoatId);
        
        try
        {
            var measurement = new BatteryMeasurement { ... };
            await repo.AddAsync(measurement);
            
            logger.LogInformation("Battery measurement created: {Id}", 
                measurement.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create battery measurement");
            throw;
        }
    }
}
```

### Debug-gids

1. **Build fails:** Controleer alle usings, DI-registratie
2. **API 500 error:** Kijk Application Insights / logs
3. **Data niet opgeslagen:** Controleer SaveChangesAsync() calls
4. **Interpretation fails:** Voeg logging toe in Interpreter
5. **Performance traag:** Profiler gebruiken, queries checken

---

## Referenties & Bronnen

### Documentatie
- [BootManager Repository](https://github.com/rrvanleeuwen/BootManagerV2)
- [.NET 8 Documentation](https://learn.microsoft.com/en-us/dotnet/core/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [Blazor Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)

### Koppelingen
- **Branch:** `feature/NetwerkData/Interpretation`
- **Repository instructions:** `.github/copilot-instructions.md`

---

**Versiegeschiedenis**

| Versie | Datum | Wijzigingen |
|--------|-------|-----------|
| 1.0 | 15 mrt 2026 | Initiale architectuurdocumentatie |

---

*Dit document wordt bijgewerkt naarmate de applicatie evolueert. Bijdragen en feedback zijn welkom!*

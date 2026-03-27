# BootManager Development Guide

## Getting Started

### Prerequisites

- .NET 8 SDK
- Visual Studio 2022+ or VS Code
- SQLite (included in EF Core)
- Git

### Initial Setup

```bash
cd C:\Users\rrvan\source\repos\BootManagerV2

# Restore packages
dotnet restore

# Build solution
dotnet build

# Apply migrations to create database
dotnet ef database update --project BootManager.Infrastructure

# Run tests (optional)
dotnet test
```

## Project Structure

```
BootManager/
├── BootManager.Core/                    # Domain entities & interfaces
│   ├── Entities/                        # {Type}Measurement classes
│   ├── Interfaces/                      # IRepository, ISystemClock, etc.
│   └── ValueObjects/                    # Hash results, credentials
│
├── BootManager.Application/             # Business logic, features
│   ├── {FeatureName}/                   # Feature folders
│   │   ├── DTOs/                        # Request/Response DTOs
│   │   └── Services/                    # Feature services & interfaces
│   ├── NetworkMessageParsing/           # PGN parsing (technical)
│   ├── NetworkMessageInterpretation/    # Semantic decoding
│   ├── NetworkMessages/                 # Orchestration layer
│   └── DependencyInjection.cs           # Service registration
│
├── BootManager.Infrastructure/          # Data persistence
│   ├── Persistence/
│   │   ├── BootManagerDbContext.cs      # EF Core DbContext
│   │   ├── Configurations/              # Entity configurations
│   │   └── Repositories/                # Generic repository impl.
│   ├── Migrations/                      # EF Core migration files
│   ├── Security/                        # Encryption, hashing
│   └── DependencyInjection.cs           # Infrastructure services
│
├── BootManager.Web/                     # API controllers, Blazor
│   ├── Controllers/                     # REST API endpoints
│   └── Components/                      # Blazor components (if any)
│
├── BootManager.Tools.Simulator/         # NMEA 2000 test data generator
│   ├── NMEA2000/                        # Payload builders, specs
│   └── Models/                          # BoatState, simulation data
│
├── BootManager.Tools.Ingest/            # Data import tool
│   └── Program.cs                       # Reads lines, sends to API
│
├── .docs/                               # Documentation
│   ├── ARCHITECTURE.md                  # System design overview
│   └── features/                        # Feature specifications
│
└── .github/
    └── copilot-instructions.md          # Repository-wide guidelines
```

## Development Workflow

### Adding a New Measurement Type

Example: Adding a hypothetical "Temperature" measurement type

#### 1. Create Domain Entity (Core)

```csharp
// BootManager.Core/Entities/TemperatureMeasurement.cs
public class TemperatureMeasurement
{
    public int Id { get; private set; }
    public DateTime RecordedAtUtc { get; private set; }
    public string Source { get; private set; } = default!;
    public string MessageId { get; private set; } = default!;
    public decimal TemperatureCelsius { get; private set; }
    
    private TemperatureMeasurement() { }
    
    public TemperatureMeasurement(DateTime recordedAtUtc, string source, string messageId, decimal temperatureCelsius)
    {
        RecordedAtUtc = recordedAtUtc;
        Source = source;
        MessageId = messageId;
        TemperatureCelsius = temperatureCelsius;
    }
}
```

#### 2. Add EF Core Configuration (Infrastructure)

```csharp
// BootManager.Infrastructure/Persistence/Configurations/TemperatureMeasurementConfiguration.cs
public class TemperatureMeasurementConfiguration : IEntityTypeConfiguration<TemperatureMeasurement>
{
    public void Configure(EntityTypeBuilder<TemperatureMeasurement> b)
    {
        b.ToTable("TemperatureMeasurements");
        b.HasKey(x => x.Id);
        
        b.Property(x => x.RecordedAtUtc).IsRequired();
        b.Property(x => x.Source).IsRequired().HasMaxLength(256);
        b.Property(x => x.MessageId).IsRequired().HasMaxLength(128);
        b.Property(x => x.TemperatureCelsius).HasPrecision(10, 2);
        
        b.HasIndex(x => x.RecordedAtUtc);
    }
}
```

Add to DbContext:
```csharp
public DbSet<TemperatureMeasurement> TemperatureMeasurements => Set<TemperatureMeasurement>();

// In OnModelCreating:
modelBuilder.ApplyConfiguration(new Configurations.TemperatureMeasurementConfiguration());
```

#### 3. Create DTOs (Application)

```csharp
// BootManager.Application/TemperatureMeasurements/DTOs/CreateTemperatureMeasurementRequestDto.cs
public class CreateTemperatureMeasurementRequestDto
{
    public DateTime RecordedAtUtc { get; init; }
    public string Source { get; init; } = default!;
    public string MessageId { get; init; } = default!;
    public decimal TemperatureCelsius { get; init; }
}

// BootManager.Application/NetworkMessageInterpretation/DTOs/TemperatureMessageInterpretationDto.cs
public class TemperatureMessageInterpretationDto
{
    public bool IsSuccess { get; set; }
    public decimal? TemperatureCelsius { get; set; }
    public string Unit { get; set; } = "°C";
    public string? ErrorMessage { get; set; }
}
```

#### 4. Create Services (Application)

```csharp
// Interface
public interface ITemperatureMeasurementService
{
    Task<int> SaveAsync(CreateTemperatureMeasurementRequestDto request, CancellationToken cancellationToken = default);
}

// Implementation
public class TemperatureMeasurementService : ITemperatureMeasurementService
{
    private readonly IRepository<TemperatureMeasurement> _repo;
    private readonly ILogger<TemperatureMeasurementService> _logger;

    public TemperatureMeasurementService(IRepository<TemperatureMeasurement> repo, ILogger<TemperatureMeasurementService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<int> SaveAsync(CreateTemperatureMeasurementRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.Source)) throw new ArgumentException("Source required", nameof(request.Source));
        if (request.TemperatureCelsius < -40 || request.TemperatureCelsius > 125) 
            throw new ArgumentException("Temperature out of valid range", nameof(request.TemperatureCelsius));

        var entity = new TemperatureMeasurement(
            request.RecordedAtUtc,
            request.Source,
            request.MessageId,
            request.TemperatureCelsius
        );

        await _repo.AddAsync(entity, cancellationToken);
        _logger.LogInformation("Temperature saved: {Source}, {Temp}°C", entity.Source, entity.TemperatureCelsius);
        
        return entity.Id;
    }
}
```

#### 5. Create Interpreter (Application)

```csharp
// Decoder for PGN (example: assume bytes 0-1 = temp in centi-degrees)
public class TemperatureMessageInterpreterService : INetworkMessageInterpreter<TemperatureMessageInterpretationDto>
{
    public bool CanInterpret(NetworkMessageParseResultDto parseResult) =>
        parseResult.IsSuccess 
        && parseResult.MessageType == NetworkMessageType.Temperature
        && parseResult.PayloadBytes.Length >= 2;

    public TemperatureMessageInterpretationDto Interpret(NetworkMessageParseResultDto parseResult)
    {
        try
        {
            ushort tempCentiDegrees = (ushort)(parseResult.PayloadBytes[0] | (parseResult.PayloadBytes[1] << 8));
            decimal tempCelsius = tempCentiDegrees / 100.0m;

            return new TemperatureMessageInterpretationDto
            {
                IsSuccess = true,
                TemperatureCelsius = tempCelsius,
                Unit = "°C"
            };
        }
        catch (Exception ex)
        {
            return new TemperatureMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = $"Temperature decode failed: {ex.Message}"
            };
        }
    }
}
```

#### 6. Update Parser & Enum (Application)

```csharp
// Add to NetworkMessageType enum
public enum NetworkMessageType
{
    // ...existing values...
    Temperature = 7
}

// Update PgnToType mapping in NetworkMessageParserService
private static readonly Dictionary<uint, NetworkMessageType> PgnToType = new()
{
    // ...existing entries...
    { 130312, NetworkMessageType.Temperature }  // Example PGN
};
```

#### 7. Integrate into Orchestration (Application)

Add to `NetworkMessageService`:

```csharp
// Constructor injection
private readonly INetworkMessageInterpreter<TemperatureMessageInterpretationDto> _temperatureInterpreter;
private readonly ITemperatureMeasurementService _temperatureMeasurementService;

// In CreateAsync
await TryInterpretAndSaveTemperatureMessageAsync(parseResult, request, ct);

// New method
private async Task TryInterpretAndSaveTemperatureMessageAsync(
    NetworkMessageParseResultDto parseResult,
    CreateNetworkMessageRequestDto request,
    CancellationToken ct)
{
    try
    {
        if (!_temperatureInterpreter.CanInterpret(parseResult)) return;

        var interpretation = _temperatureInterpreter.Interpret(parseResult);

        if (interpretation.IsSuccess && interpretation.TemperatureCelsius.HasValue)
        {
            _logger.LogInformation("Temperature interpreted: {Temp}{Unit}", 
                interpretation.TemperatureCelsius, interpretation.Unit);

            try
            {
                var tempDto = new CreateTemperatureMeasurementRequestDto
                {
                    RecordedAtUtc = request.ReceivedAtUtc,
                    Source = request.Source,
                    MessageId = request.MessageId ?? string.Empty,
                    TemperatureCelsius = interpretation.TemperatureCelsius.Value
                };

                await _temperatureMeasurementService.SaveAsync(tempDto, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Temperature storage failed for {MessageId}", request.MessageId);
            }
        }
        else
        {
            _logger.LogWarning("Temperature interpretation failed: {Error}", 
                interpretation.ErrorMessage ?? "Unknown error");
        }
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Unexpected error in Temperature interpretation");
    }
}
```

#### 8. Register in DI (Application)

```csharp
// BootManager.Application/DependencyInjection.cs
services.AddTransient<INetworkMessageInterpreter<TemperatureMessageInterpretationDto>, 
                      TemperatureMessageInterpreterService>();
services.AddScoped<ITemperatureMeasurementService, TemperatureMeasurementService>();
```

#### 9. Create Migration (Infrastructure)

```bash
dotnet ef migrations add AddTemperatureMeasurement --project BootManager.Infrastructure
dotnet ef database update --project BootManager.Infrastructure
```

### Running the Application

#### Using Visual Studio

1. Set `BootManager.Web` as startup project
2. Press F5 to run with debugger
3. API runs on `https://localhost:5001` (or configured port)

#### Using CLI

```bash
# Development
dotnet run --project BootManager.Web --configuration Debug

# Production-like
dotnet run --project BootManager.Web --configuration Release
```

### Testing Message Flow

#### Option 1: Simulator → Ingest → API

```bash
# Terminal 1: Start API
dotnet run --project BootManager.Web

# Terminal 2: Start simulator (generates messages)
dotnet run --project src/BootManager.Tools.Simulator

# Terminal 3: Run ingest (reads simulator output, sends to API)
dotnet run --project src/BootManager.Tools.Ingest
```

#### Option 2: Direct API Call

```bash
# Create a raw network message
curl -X POST https://localhost:5001/api/networkmessages \
  -H "Content-Type: application/json" \
  -d '{
    "receivedAtUtc": "2026-03-27T10:15:30Z",
    "source": "127.0.0.1",
    "protocol": "NMEA2000",
    "rawLine": "PGN 127250",
    "messageId": "1F23",
    "payloadHex": "001F0F000000000"
  }'

# Retrieve latest messages
curl https://localhost:5001/api/networkmessages/latest
```

### Debugging Tips

#### Enable Detailed Logging

```csharp
// In Program.cs (BootManager.Web)
services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddFilter("BootManager", LogLevel.Debug);
    logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Debug);
});
```

#### Check Database Directly

```bash
# Using sqlite3 CLI
sqlite3 BootManager.db

# List tables
.tables

# Query HeadingMeasurements
SELECT * FROM HeadingMeasurements ORDER BY RecordedAtUtc DESC LIMIT 10;
```

#### Inspect EF Core SQL Queries

Add to `BootManagerDbContext`:
```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    base.OnConfiguring(optionsBuilder);
    optionsBuilder.LogTo(Console.WriteLine);
}
```

## Code Style & Conventions

### Naming

- **Entities:** `{Type}Measurement` (e.g., `HeadingMeasurement`)
- **Services:** `{Type}MeasurementService`, `{Type}MessageInterpreterService`
- **DTOs:** `Create{Type}MeasurementRequestDto`, `{Type}MessageInterpretationDto`
- **Interfaces:** `I{ServiceName}` (e.g., `IHeadingMeasurementService`)
- **Features:** Organized in feature folders, lowercase with underscores internally

### Documentation

- **Dutch XML comments** on public types/methods
- **English inline comments** only where logic is non-obvious
- **Commit messages:** Dutch, descriptive verbs ("Voeg Heading slice toe")

### Async/Await

- All I/O operations async
- `CancellationToken` parameter on public async methods
- Prefer `async Task` (no `Fire and forget`)

## Common Issues & Solutions

### "DbContext could not be created"

**Cause:** Missing or invalid migrations

**Fix:**
```bash
dotnet ef migrations add InitialCreate --project BootManager.Infrastructure
dotnet ef database update --project BootManager.Infrastructure
```

### "PGN not recognized"

**Cause:** New PGN added to parser but not mapped in `PgnToType`

**Fix:** Add entry to `NetworkMessageParserService.PgnToType` dictionary

### Message interpreted but not stored

**Cause:** Validation error in measurement service (e.g., value out of range)

**Check:** Look at logs for validation exception, update DTO/entity constraints

### Compilation fails after new feature

**Cause:** Missing DI registration

**Fix:** Add service registrations to `BootManager.Application.DependencyInjection.cs`

## Performance Considerations

- **Indexing:** All `RecordedAtUtc` columns indexed for chronological queries
- **Batch processing:** Ingest tool batches messages where possible
- **Repository:** Generic repository uses EF Core's change tracking efficiently
- **Logging:** Production should use structured logging (Serilog integration TBD)

## Security Notes

- No authentication currently; add JWT/API keys before production
- Encryption for sensitive fields: `AesGcmEncryptionService` available
- SQL injection: EF Core parameterization prevents this

## Resources

- **.NET 8 Docs:** https://learn.microsoft.com/en-us/dotnet/
- **Entity Framework Core:** https://learn.microsoft.com/en-us/ef/core/
- **NMEA 2000 Reference:** https://www.nmea.de/
- **Async/Await Best Practices:** https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming

---

**Last Updated:** 2026-03-27

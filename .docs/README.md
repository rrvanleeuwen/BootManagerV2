# BootManager Documentation

This folder contains comprehensive documentation for the BootManager project.

## Quick Links

### Getting Started
- **[ARCHITECTURE.md](ARCHITECTURE.md)** - System design, layering, data flow, vertical slices pattern
- **[DEVELOPMENT.md](DEVELOPMENT.md)** - Setup, development workflow, adding new features, debugging

### Project Status
- **[TODO.md](TODO.md)** - Implementation status, roadmap, backlog, known limitations

### Feature Specifications
- **[features/heading-slice-spec.md](features/heading-slice-spec.md)** - Detailed Heading (PGN 127250) implementation spec

## Documentation Structure

```
.docs/
├── README.md                    # This file
├── ARCHITECTURE.md              # System design & patterns
├── DEVELOPMENT.md               # How to develop & extend
├── TODO.md                       # Status & roadmap
└── features/
    ├── heading-slice-spec.md    # Heading measurement vertical slice
    └── [other feature specs]
```

## Key Concepts

### Vertical Slices

BootManager organizes measurement types as **vertical slices**, each containing:

1. **Entity** (BootManager.Core) - Domain model
2. **Configuration** (BootManager.Infrastructure) - EF Core mapping
3. **DTOs** (BootManager.Application) - Request/response contracts
4. **Services** (BootManager.Application) - Business logic
5. **Interpreter** (BootManager.Application) - NMEA 2000 decoding
6. **Integration** (BootManager.Application) - Orchestration

**Example slices:** Battery, Depth, Wind, Motion, Position, Heading

### NMEA 2000 PGN Processing

```
Raw Message (Simulator/Sensor)
    ↓
Ingest Tool (HTTP POST to API)
    ↓
Parser (PGN → Type classification, Hex → Bytes)
    ↓
Interpreter (Bytes → Semantic values in degrees/knots/etc.)
    ↓
Measurement Service (Validation → Persistence)
    ↓
Database (SQLite)
```

## Common Tasks

### Add a New Measurement Type

See **[DEVELOPMENT.md](DEVELOPMENT.md)** → "Adding a New Measurement Type" for step-by-step guide.

### Run the Application

```bash
dotnet run --project BootManager.Web
```

See **[DEVELOPMENT.md](DEVELOPMENT.md)** → "Running the Application" for details.

### Apply Database Migrations

```bash
dotnet ef database update --project BootManager.Infrastructure
```

### Create a New EF Core Migration

```bash
dotnet ef migrations add AddYourFeature --project BootManager.Infrastructure
```

## Repository Structure

- `BootManager.Core/` - Domain layer (entities, interfaces)
- `BootManager.Application/` - Business logic (services, DTOs, interpreters)
- `BootManager.Infrastructure/` - Data persistence (EF Core, repositories)
- `BootManager.Web/` - Web API & presentation layer
- `src/BootManager.Tools.Simulator/` - NMEA 2000 test data generator
- `src/BootManager.Tools.Ingest/` - Message ingestion tool
- `.github/copilot-instructions.md` - Repository guidelines (Dutch)
- `.docs/` - This documentation folder

## Current Implementation Status

✅ **Fully Implemented Slices:**
- Battery (PGN 127508)
- Depth (PGN 128267)
- Wind (PGN 130306)
- Motion / COG-SOG (PGN 129026)
- Position (PGN 129025)
- Heading (PGN 127250) - Code complete, awaiting migration

See **[TODO.md](TODO.md)** for complete status and roadmap.

## Contributing

1. Read **[ARCHITECTURE.md](ARCHITECTURE.md)** to understand the design
2. Follow the pattern in **[DEVELOPMENT.md](DEVELOPMENT.md)** when adding features
3. Write Dutch XML documentation on public types
4. Build and test locally before pushing
5. Update `.docs/` if you change architecture or add features

## Code Style

- **Language:** C# 12 (.NET 8)
- **Documentation:** Dutch XML comments on public APIs, English inline comments where logic is non-obvious
- **Conventions:** See **[DEVELOPMENT.md](DEVELOPMENT.md)** → "Code Style & Conventions"

## Getting Help

1. Check **[DEVELOPMENT.md](DEVELOPMENT.md)** → "Common Issues & Solutions"
2. Review existing feature specs in `features/`
3. Look at similar existing slices (e.g., Wind for reference)
4. Check `.github/copilot-instructions.md` for repository-specific rules

## References

- **.NET 8 Documentation:** https://learn.microsoft.com/en-us/dotnet/
- **Entity Framework Core:** https://learn.microsoft.com/en-us/ef/core/
- **NMEA 2000 Specifications:** https://www.nmea.de/
- **C# Coding Guidelines:** https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions

---

**Documentation Last Updated:** 2026-03-27  
**Current Status:** Heading slice code-complete, pending migration & testing

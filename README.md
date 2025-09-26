# TooliRent
## Innehåll
- [Om](#om)
- [Arkitekturöversikt](#arkitekturöversikt)
  - [Domänlager](#domänlager)
  - [Applikationslager](#applikationslager)
  - [Infrastruktur](#infrastruktur)
  - [WebApi](#webapi)
- [Centrala affärsregler](#centrala-affärsregler)
- [Teknik](#teknik)
- [Kom igång lokalt](#kom-igång-lokalt)
## Om
TooliRent är ett lagerindelat (.NET 8) Web API för uthyrning av verktyg. Det använder ASP.NET Core Identity + JWT (access- och refresh-tokens), hanterar bokningar, tillgänglighet, samt administrativa statistikvyer. Arkitekturen följer en tydlig lagerseparation:

- WebApi (presentation / HTTP)
- Application (DTO:er, tjänster, mapping, validering)
- Domain (entiteter, enums, repository-abstraktioner, read models)
- Infrastructure (EF Core DbContext, repository-implementationer, persistens)

## Arkitekturöversikt

### Domänlager
- Entiteter: Tool, ToolCategory, Booking, BookingItem, ApplicationUser, RefreshToken
- Enums: ToolStatus, BookingStatus, BookingItemStatus
- Repository-gränssnitt: IToolRepository, IToolCategoryRepository, IBookingRepository, IAdminRepository
- Read models: AdminStats, ToolUsageAggregate

### Applikationslager
- DTO:er (Tool*, Booking*, Auth*, Admin*)
- Tjänster: ToolService, ToolCategoryService, BookingService, AdminService
- AutoMapper-profiler: ToolProfile, BookingProfile, ToolCategoryProfile
- Validering: FluentValidation

### Infrastruktur
- EF Core: TooliRentDbContext (seedar roller, användare, kategorier, verktyg)
- Repositories: ToolRepository, BookingRepository, ToolCategoryRepository, AdminRepository

### WebApi
- Controllers: Auth, Tools, ToolCategories, Bookings, Admin
- JWT-generering: JwtTokenService
- Swagger/OpenAPI
- Dependency Injection: registreras i Program.cs

## Centrala affärsregler

- Ett verktyg är tillgängligt om QuantityAvailable > 0 och Status == Available
- Vid bokning:
  - StartDate < EndDate
  - Alla ToolId måste finnas
  - Kvantitet reserveras direkt genom att minska QuantityAvailable
- Avbokning tillåts inte efter checkout/hantering
- Checkout sätter BookingItem-status till CheckedOut
- Return återställer kvantiteter och kan avsluta bokningen
- Overdue-scan markerar försenade items (CheckedOut efter slutdatum)
- Refresh tokens är engångs (UsedAt sätts) och roteras vid refresh

## Teknik

- .NET 8 / C#
- ASP.NET Core Web API
- Entity Framework Core (SQL Server)
- ASP.NET Identity (Guid som nyckel)
- JWT Bearer Authentication
- AutoMapper
- FluentValidation
- Swagger / OpenAPI

## Kom igång lokalt

### Förkrav
- .NET 8 SDK
- SQL Server (LocalDB fungerar)
- Visual Studio 2022

### Klona
I din terminal skriv följande:
``` git clone https://github.com/sidstedt/TooliRent.git cd TooliRent ```

### Konfiguration
Skapa/uppdatera `TooliRent.WebApi\appsettings.json`:
Kom ihåg att byta ut servernamn till den server du använder, samt din hemliga nyckel.
```json
{
  "ConnectionStrings": 
  {
    "DefaultConnection": "Server=(DinServer/localdb); Database=TooliRentDb; Integrated Security = True; TrustServerCertificate = True;"
  },
  "Jwt":
  {
      "Issuer": "TooliRent",
      "Audience": "TooliRentClient",
      "Key": "BYT_UT_TILL_EN_STARK_HEMLIG_NYCKEL_64+_TECKEN"
  }, 
  "Logging":
  {
    "LogLevel":
    {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```
### Databas
Om migrationer saknas skriv i terminal:

``` dotnet ef migrations add InitialCreate -p TooliRent.Infrastructure -s TooliRent.WebApi ```

Om migration finns eller om du har skapat kör sedan följande i terminal:

``` dotnet ef database update -p TooliRent.Infrastructure -s TooliRent.WebApi ```

Seeding lägger in: roller, exempel-användare, kategorier och verktyg.

### Exempel-användare // lösenord
| Användare            | Lösenord   | Roll   |
|----------------------|------------|--------|
| admin@toolirent.com  | Admin123   | Admin  |
| alice@toolirent.com  | Member123  | Member |

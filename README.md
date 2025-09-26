# TooliRent
## Innehåll
- [Om](#om)
- [Arkitekturöversikt](#arkitekturöversikt)
- [Centrala affärsregler](#centrala-affärsregler)
- [Teknik](#teknik)
- [Kom igång lokalt](#kom-igång-lokalt)
- [Autentisering](#autentisering)
- [API-översikt](#api-översikt)
- [DTO:er)](#dtoer)
- [Validering](#validering)
- [Mapping (AutoMapper)](#mapping-automapper)
- [Persistens & regler](#persistens--regler)
- [Felhantering](#felhantering)

## Om
TooliRent är ett lagerindelat (.NET 8) Web API för uthyrning av verktyg. 
Det använder ASP.NET Core Identity + JWT (access- och refresh-tokens), 
hanterar bokningar, tillgänglighet, samt administrativ statistik.
Arkitekturen följer en tydlig lagerseparation:

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

## Autentisering

1. POST /api/auth/register
2. POST /api/auth/login → { token, refreshToken }
3. Skicka Bearer-token i Authorization-header
4. POST /api/auth/refresh → nytt par tokens
5. POST /api/auth/logout → revokerar refresh token

Access token: 15 min. Refresh token: 7 dagar.

## API-översikt

### Auth
| Metod | Route | Auth | Beskrivning |
|-------|-------|------|-------------|
| POST | /api/auth/register | Öppen | Registrera ny användare (Member) |
| POST | /api/auth/login | Öppen | Få JWT + refresh token |
| POST | /api/auth/refresh | Öppen | Rotera refresh token |
| POST | /api/auth/logout | Öppen | Revokera refresh token |

### Tools
| Metod | Route | Auth | Beskrivning |
|-------|-------|------|------------|
| GET | /api/tools | Publik | Filtrering: categoryId, status, availableOnly, search |
| GET | /api/tools/{id} | Publik | Detalj |
| GET | /api/tools/available | Publik | Tillgänglighet i period |
| POST | /api/tools | Admin | Skapa |
| PUT | /api/tools/{id} | Admin | Uppdatera |
| DELETE | /api/tools/{id} | Admin | Misslyckas om bokningar finns |

### Tool Categories
| Metod | Route | Auth |
|-------|-------|------|
| GET | /api/toolcategories | Admin |
| GET | /api/toolcategories/{id} | Admin |
| POST | /api/toolcategories | Admin |
| PUT | /api/toolcategories/{id} | Admin |
| DELETE | /api/toolcategories/{id} | Admin (nekas om verktyg finns) |

### Bookings
| Metod | Route | Auth | Beskrivning |
|-------|-------|------|-------------|
| GET | /api/bookings/mine | Member/Admin | Egna bokningar |
| GET | /api/bookings/{id} | Member/Admin | Egen bokning |
| GET | /api/bookings/get-all-admin | Admin | Alla bokningar |
| GET | /api/bookings/{id}/admin | Admin | En bokning |
| POST | /api/bookings | Member/Admin | Skapa bokning |
| DELETE | /api/bookings/{id} | Member/Admin | Avboka (om ej hanterad) |
| POST | /api/bookings/{id}/checkout | Admin | Checka ut |
| POST | /api/bookings/{id}/return | Admin | Återlämna |
| POST | /api/bookings/overdue/scan | Admin | Markera försenade |

### Admin
| Metod | Route | Auth | Beskrivning |
|-------|-------|------|-------------|
| GET | /api/admin/users | Admin | Lista användare |
| GET | /api/admin/users/by-email | Admin | Slå upp användare |
| POST | /api/admin/users/{id}/activate | Admin | Aktivera |
| POST | /api/admin/users/{id}/deactivate | Admin | Inaktivera |
| GET | /api/admin/stats | Admin | Statistik |
| GET | /api/admin/stats/usage | Admin | Användningsdata & topplista |

## DTO:er

### Verktyg
- ToolListItemDto: Id, Name, PricePerDay, QuantityAvailable, CategoryId, CategoryName, Status
- ToolDetailDto: + Description, CreatedAt
- ToolCreateDto / ToolUpdateDto: validerade fält, Update inkluderar Status

### Bokningar
- CreateBookingDto: StartDate, EndDate, Items[]
- BookingItemCreateDto: ToolId, Quantity
- BookingListItemDto: sammanfattning (Status int, ItemCount)
- BookingDetailDto: Items (ToolName, Quantity, Status)

### Auth
- LoginDto, RegisterDto, RefreshTokenDto

### Admin
- AdminUserListUsersDto, AdminStatsDto, UsageStatsDto, ToolUsageItemDto

## Validering
- FluentValidation (automatisk registrering i Program.cs)

## Mapping (AutoMapper)
- ToolProfile
- ToolCategoryProfile
- BookingProfile

## Persistens & regler
- EF Core + SQL Server
- QuantityAvailable som concurrency token
- Verktyg kan ej tas bort om det finns BookingItems
- Unik index (BookingId, ToolId) för BookingItem

## Felhantering
- 400: valideringsfel / affärsregel
- 401: ogiltig inloggning / inaktiv användare
- 403: saknar roll
- 404: resurs finns ej
- 201: skapat
- 204: ingen innehåll vid uppdatering/radering

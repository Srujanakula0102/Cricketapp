# Cricket Sports App

A modular cricket sports platform. The frontend will use React with JavaScript only; TypeScript is intentionally not used.

## Module 1: backend foundation

The backend is split into four projects:

- `CricketSports.Domain`: framework-independent cricket business concepts.
- `CricketSports.Application`: use cases, validation, and application contracts.
- `CricketSports.Infrastructure`: MySQL, Entity Framework Core, and ASP.NET Core Identity persistence.
- `CricketSports.API`: HTTP API, JWT bearer authentication, Swagger, health checks, and exception handling.

## Prerequisites

- .NET SDK 9
- MySQL 8

## Local configuration

`backend/CricketSports.API/appsettings.json` contains development placeholders. Override them locally rather than committing real values:

```powershell
dotnet user-secrets init --project backend/CricketSports.API
dotnet user-secrets set "ConnectionStrings:CricketSportsDatabase" "Server=localhost;Port=3306;Database=cricket_sports;User=YOUR_USER;Password=YOUR_PASSWORD" --project backend/CricketSports.API
dotnet user-secrets set "Jwt:Key" "a-long-random-development-key-with-at-least-32-characters" --project backend/CricketSports.API
```

## Run locally

```powershell
dotnet restore CricketSportsApp.slnx
dotnet build CricketSportsApp.slnx
dotnet run --project backend/CricketSports.API
```

In development, Swagger is available at `/swagger` and the health endpoint is `/health`.

## Database migrations

The initial Identity migration is in `backend/CricketSports.Infrastructure/Persistence/Migrations`. After configuring a local MySQL connection, apply it with:

```powershell
dotnet ef database update --project backend/CricketSports.Infrastructure --startup-project backend/CricketSports.API
```

## Next module

Module 2 will introduce the core cricket entities and CRUD APIs for teams, players, venues, tournaments, and matches.

## Live scoring endpoints

After applying migrations and assigning an account the `Scorer` or `Admin` role, use the JWT bearer token with these protected endpoints:

- `POST /api/scoring/matches/{matchId}/innings/start`
- `POST /api/scoring/matches/{matchId}/delivery`
- `POST /api/scoring/matches/{matchId}/bowler`
- `POST /api/scoring/matches/{matchId}/undo`
- `POST /api/scoring/matches/{matchId}/innings/end`
- `POST /api/scoring/matches/{matchId}/end`

The scoring engine calculates totals, legal balls, strike rotation, and over completion on the backend. SignalR broadcasting is added in the following module.

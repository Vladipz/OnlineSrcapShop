# Product Catalog Parser

To support another site, implement `IProductParser`, register it in
`Common/DependencyInjection.cs`, and allow the site's domains in `SourceUrlPolicy`.

Angular frontend with an ASP.NET Core Minimal API backend and a SQLite database
(EF Core + Identity). AngleSharp extracts names, descriptions, image URLs, and
prices from [Books to Scrape](https://books.toscrape.com/), up to 20 books per
listing. Admin imports, edits, and deletes products; User views the two-column
catalog. Imports save products immediately and skip duplicates. Images are linked,
not downloaded. Backend and frontend live under `src/backend` and `src/frontend`.

## Run locally

Requires .NET SDK `10.0.202` (or a newer `10.0.2xx` patch), Node.js `24.15.0+`
(Node 24), and npm 11+.

From the repository root:

```powershell
dotnet run --project src/backend/ProductCatalog.Api --launch-profile http
```

In a second terminal:

```powershell
cd src/frontend/product-catalog-web
npm ci
npm start
```

Open **http://localhost:4200**. The frontend proxies API requests to port `5080`.
The database and demo accounts are created automatically on startup.

| Role | Email | Password |
| --- | --- | --- |
| Admin | `admin@example.com` | `Admin123!` |
| User | `user@example.com` | `User123!` |

Sign in as Admin, open Import, and use `https://books.toscrape.com/` to add 20 books.
Demo credentials are for local evaluation only.

## Run in Docker

Requires Docker with Linux containers and Docker Compose. From the repository root,
copy the configuration **only if `.env` does not already exist**:

```powershell
Copy-Item .env.example .env
```

Set `ADMIN_PASSWORD` and `USER_PASSWORD` in `.env`, then run:

```powershell
docker compose up --build -d --wait
```

Open **http://localhost:8080** and sign in with the accounts configured in `.env`.
The stack runs locally over HTTP; public HTTPS deployment is not configured.
SQLite and authentication keys persist in `./data`; changing seed passwords does
not reset existing accounts. On Linux, make `./data` writable by backend UID `1654`
before starting (`sudo chown 1654:1654 ./data && sudo chmod 700 ./data`).

Stop the containers without deleting saved data:

```powershell
docker compose down
```

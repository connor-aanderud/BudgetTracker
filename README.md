# Budget Tracker

A local-only personal budgeting web app. Upload your bank/credit-card statements
(PDF or CSV), and it parses, de-duplicates, auto-categorizes, and visualizes your
spending — income vs. expenses, budgets, category breakdowns, trends, and overspend
alerts.

**Supported statements today:** FNBO SCHEELS Visa (PDF), Chase Prime Visa (PDF),
and generic/Chase/Mint CSV exports. The PDF parser auto-detects which bank a
statement is from.

Everything runs on your machine. The database is a single local file
(`data/budget.db`) — there's no server to install or account to create.

---

## Stack

| Layer    | Tech |
|----------|------|
| Backend  | ASP.NET Core (.NET 10), Clean Architecture (Domain / Application / Data / API) |
| Database | SQLite via EF Core (single file, `data/budget.db`) |
| Parsing  | PdfPig (PDF), CsvHelper (CSV) |
| Frontend | React 19 + TypeScript + Vite, Redux Toolkit + Redux Saga, Recharts, Tailwind + shadcn/ui |

---

## Prerequisites

Install these once on your computer:

- **[.NET 10 SDK](https://dotnet.microsoft.com/download)** — `dotnet --version` should print `10.x`
- **[Node.js 20+](https://nodejs.org/)** (LTS) — `node --version` should print `v20` or higher
- **[Git](https://git-scm.com/)**
- *(Optional)* **[Docker Desktop](https://www.docker.com/products/docker-desktop/)** — only if you want the one-command Docker path

---

## 1. Clone

```bash
git clone https://github.com/connor-aanderud/BudgetTracker.git
cd BudgetTracker
```

---

## 2. (Recommended) Protect your data with EF migrations — do this BEFORE your first run

The app works without this, but you're about to import real financial history.
Enabling migrations now means future schema changes upgrade your database in place
instead of forcing you to delete it and re-import everything.

> ⚠️ Do this **before** the first time you run the API (i.e. before `data/budget.db`
> is created). If you already ran the app once, delete `data/budget.db` first — it's
> empty at that point.

```bash
# one-time: install the EF CLI tool
dotnet tool install --global dotnet-ef

# generate the initial migration (creates src/BudgetTracker.Data/Migrations/)
dotnet ef migrations add InitialCreate \
  --project src/BudgetTracker.Data \
  --startup-project src/BudgetTracker.API
```

The API applies migrations automatically on startup (it uses `Migrate()` when any
migration exists, otherwise `EnsureCreated()`). After generating it, commit the new
`Migrations/` folder so it's part of the repo.

If you'd rather just try the app first, **skip this section** — the database is still
created automatically on first run.

---

## 3. Run it (two terminals)

### Terminal 1 — Backend API

```bash
cd src/BudgetTracker.API
dotnet run
```

- API: <http://localhost:5000>
- Swagger (interactive API docs): <http://localhost:5000/swagger>
- On first run it creates `data/budget.db` and seeds the default categories and
  merchant rules.

### Terminal 2 — Frontend

```bash
cd client
npm install      # first time only
npm run dev
```

- App: **<http://localhost:5173>**

Open <http://localhost:5173> in your browser. The frontend calls the API at
`http://localhost:5000` (CORS is already configured for the dev server).

---

## 4. Use it

1. **Upload** → drag in a statement PDF (FNBO SCHEELS Visa or Chase Prime Visa) or a
   CSV. You'll get a preview of parsed transactions with duplicate detection.
2. **Confirm** the import → transactions are saved and auto-categorized from the
   merchant rules.
3. **Transactions** → browse, filter, search, and re-categorize. Re-categorizing is
   how you teach it about new merchants.
4. **Categories / Merchant Rules** → manage categories and the keyword→category rules
   (this is where you handle any merchant the seed didn't recognize).
5. **Income** and **Budgets** → enter income and set per-category monthly limits.
6. **Dashboard** → monthly summary, category breakdown, budget vs. actual, spending
   trends, top merchants, and overspend alerts.

---

## Alternative: run everything with Docker

One command, no local .NET/Node needed (just Docker Desktop):

```bash
docker compose up --build
```

- Frontend: <http://localhost:5173>
- API: <http://localhost:5000>
- The database file persists in `./data` on your host.

Stop with `Ctrl+C`, or `docker compose down`.

---

## Useful commands

```bash
# Backend
dotnet build BudgetTracker.slnx          # build everything
dotnet test  BudgetTracker.slnx          # run the test suite

# Frontend (from client/)
npm run build                            # production build + type-check
npm run lint                             # lint
npm test                                 # run frontend tests
```

---

## Project layout

```
BudgetTracker/
├── src/
│   ├── BudgetTracker.Domain/        # Entities + enums (no dependencies)
│   ├── BudgetTracker.Application/   # Services, DTOs, interfaces, parsers
│   │   └── Services/Parsing/        # PdfStatementParser (FNBO + Chase), CsvStatementParser
│   ├── BudgetTracker.Data/          # EF Core DbContext, repositories, seed data
│   ├── BudgetTracker.API/           # ASP.NET Core endpoints (Program.cs entry point)
│   └── BudgetTracker.Tests/         # xUnit tests
├── client/                          # React + Vite frontend
├── data/                            # SQLite database lives here (budget.db, git-ignored)
├── docker-compose.yml
└── BudgetTracker.slnx
```

---

## Notes

- **Local & private.** The database, and any statements you upload, never leave your
  machine. Sample statement PDFs are git-ignored and never committed.
- **Backups.** The Settings page can export/import the database, so you can back up
  `data/budget.db` before experimenting.
- **Adding banks.** New statement formats plug in as parsers behind
  `IStatementParser` (CSV) / the bank-detection switch in `PdfStatementParser` (PDF).

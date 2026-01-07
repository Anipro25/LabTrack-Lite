# LabTrack Lite

Lightweight R&D asset and ticketing platform with ASP.NET Core minimal APIs, PostgreSQL, and a React admin UI. Focused on RBAC, security hardening (OWASP/API Top 10), accessibility (WCAG 2.2 AA), and hackathon-speed deployment.

## Stack
- API: ASP.NET Core 8 minimal APIs, EF Core, JWT auth, policy-based RBAC
- DB: PostgreSQL (via Npgsql), optional pgAdmin
- Frontend: React (Vite) with protected routes and admin workflows
- Infra: Docker Compose for db/pgAdmin/api

## Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (for API)
- [Node.js 18+](https://nodejs.org/) (for frontend)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for PostgreSQL + infra)
- Git (optional, for version control)

## Features (intended)
- Roles: Admin, Engineer, Technician
- Assets: CRUD, QR code field
- Tickets: CRUD, comments, status transitions (Open → In Progress → Resolved/Closed)
- Pagination and filtered queries
- Chatbot: NLQ endpoint (rule-based) with hook for external NLP
- Security: JWT bearer, CORS, rate limiting, validation, security headers
- Accessibility: keyboard-first flows, aria labels, contrast-friendly palette

## Quick Start

### Option 1: Full Docker Stack (Recommended)
1. Copy environment template:
   ```powershell
   Copy-Item .env.example .env
   ```
2. Edit `.env` and set strong secrets (JWT_SIGNING_KEY, POSTGRES_PASSWORD).
3. Start all services:
   ```powershell
   docker-compose -f infra/docker-compose.yml up --build
   ```
4. API: http://localhost:5000 (Swagger at http://localhost:5000/swagger)
5. pgAdmin: http://localhost:5050 (admin@labtrack.local / adminpass)
6. Frontend (separate terminal):
   ```powershell
   cd web
   npm install
   npm run dev
   ```
7. Open http://localhost:5173 and login with demo credentials

### Option 2: Local Development (API + DB in Docker)
1. Start PostgreSQL + pgAdmin only:
   ```powershell
   docker-compose -f infra/docker-compose.yml up db pgadmin
   ```
2. Run API locally:
   ```powershell
   cd api
   dotnet restore
   dotnet ef database update  # Run migrations (after installing EF tools)
   dotnet run
   ```
3. Run frontend:
   ```powershell
   cd web
   npm install
   npm run dev
   ```

### First-Time Setup
Install EF Core tools for migrations:
```powershell
dotnet tool install --global dotnet-ef
```

Create initial migration (from `api/` folder):
```powershell
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Environment
See [.env.example](.env.example) for required keys.

## Security & Accessibility Notes
- Enforce HTTPS/HSTS in production, strict CORS, rate limiting, security headers.
- Validate and sanitize inputs/outputs; avoid sensitive data in logs.
- WCAG: semantic HTML, focus management, aria attributes, visible focus states, high contrast.

## Troubleshooting

**"dotnet is not recognized"**
- Install [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Restart terminal after installation
- Verify: `dotnet --version`

**"npm is not recognized"**
- Install [Node.js](https://nodejs.org/)
- Restart terminal
- Verify: `node --version` and `npm --version`

**Database connection errors**
- Ensure Docker containers are running: `docker ps`
- Check connection string in `.env` or `appsettings.json`
- For local API, use `Host=localhost` instead of `Host=db`

**CORS errors in browser**
- Verify frontend is running on port 5173
- Check CORS policy in `Program.cs` matches your frontend URL

## Demo Credentials
- Email: `admin@example.com`, Role: `Admin`
- Email: `engineer@example.com`, Role: `Engineer`
- Email: `tech@example.com`, Role: `Technician`

(Demo login issues JWT without password verification - replace before production)

## Next Steps
1. Add EF Core migrations and seed demo data (users, assets, tickets)
2. Replace demo JWT login with real authentication (password hashing, user store)
3. Implement full CRUD forms and validation in React
4. Add unit/integration tests
5. Enhance chatbot with external NLP library or AI integration
6. Set up GitHub Actions CI/CD pipeline
7. Run security audit (OWASP ZAP or Burp Suite)
8. Deploy to cloud (Azure, AWS, or similar)

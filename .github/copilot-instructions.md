# LabTrack Lite - Project Instructions

## Project Overview
R&D Asset & Ticketing Platform with ASP.NET Core minimal APIs, PostgreSQL, and React frontend. Focus on RBAC, security (OWASP/API Top 10), accessibility (WCAG 2.2 AA), and hackathon-ready deployment.

## Tech Stack
- Backend: ASP.NET Core 8, EF Core, PostgreSQL, JWT auth
- Frontend: React 18, Vite, Axios
- Infrastructure: Docker Compose
- Security: JWT Bearer, RBAC policies, rate limiting, security headers
- Accessibility: WCAG 2.2 AA compliance

## Key Features
- RBAC: Admin, Engineer, Technician roles
- Assets: CRUD operations with QR code field
- Tickets: CRUD, comments, status transitions (Open → InProgress → Resolved → Closed)
- Chatbot: Rule-based NLQ endpoint with extensibility for external NLP
- Pagination: All list endpoints support paging
- Security: Input validation, security headers, rate limiting, CORS

## Development Guidelines
- Use semantic versioning for releases
- Follow REST conventions for API endpoints
- Maintain accessibility standards in all UI components
- Write unit tests for business logic
- Document API changes in Swagger/OpenAPI
- Use EF Core migrations for schema changes
- Never commit secrets or connection strings

## Code Quality Standards
- C#: Follow Microsoft coding conventions
- React: Use functional components and hooks
- CSS: Mobile-first responsive design
- Security: Validate inputs, sanitize outputs, use parameterized queries
- Accessibility: Semantic HTML, ARIA labels, keyboard navigation

## Setup Requirements
1. Install .NET 8 SDK
2. Install Node.js 18+
3. Install Docker Desktop
4. Copy .env.example to .env and configure
5. Run `dotnet tool install --global dotnet-ef` for migrations

## Common Commands
```powershell
# Backend
cd api
dotnet restore
dotnet ef migrations add [MigrationName]
dotnet ef database update
dotnet run

# Frontend
cd web
npm install
npm run dev
npm run build

# Docker
docker-compose -f infra/docker-compose.yml up --build
docker-compose -f infra/docker-compose.yml down
```

## Known Limitations
- Demo JWT login does not verify passwords (replace before production)
- Chatbot uses basic rule matching (integrate AI/NLP for production)
- No email notifications for ticket updates
- Limited error logging and monitoring

## Next Implementation Priorities
1. Real authentication with password hashing
2. User management endpoints and UI
3. Advanced ticket filtering and search
4. Email notifications
5. File attachments for tickets
6. Audit logging for compliance
7. CI/CD pipeline with automated tests
8. Performance monitoring and APM integration

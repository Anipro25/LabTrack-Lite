# LabTrack Lite Architecture

- API: ASP.NET Core 8 minimal APIs with EF Core (PostgreSQL). JWT bearer auth, policy-based RBAC (Admin, Engineer, Technician). CRUD for Assets, Tickets, Comments; status transitions; pagination.
- Chatbot: Rule-based NLQ endpoint with hook for external NLP. Frontend panel uses /api/chatbot.
- Frontend: React (Vite) with protected routes, assets/tickets dashboards, chatbot panel. Accessibility via semantic elements, aria labels, focus outlines, and keyboard-friendly controls.
- Infra: Docker Compose for PostgreSQL + pgAdmin + API. Env-driven connection strings and JWT secrets.
- Security: OWASP Top 10 oriented defaults (HTTPS/HSTS in prod, security headers, rate limiting, validation stubs, strict CORS). Replace demo login with real user store and password hashing before production.

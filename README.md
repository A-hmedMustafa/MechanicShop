# 🔧 MechanicShop Management System

A production-grade **RESTful API** for managing a mechanic shop — built with **Clean Architecture**, **CQRS**, **Domain-Driven Design**, and modern .NET practices.

> Developed by **Ahmed Mostafa**

---

## 📌 Overview

MechanicShop is a backend system that handles the full lifecycle of a mechanic shop — from scheduling work orders and managing customers, to issuing invoices and notifying clients in real time.

It was built with a strong focus on **architecture quality**, **testability**, and **production readiness**.

> ⚠️ **Note:** The Blazor WebAssembly frontend (`MechanicShop.Client`) is currently in progress. This repository showcases the complete backend implementation.

---

## ✨ Features

- 📅 **Work Order Management** — Create, schedule, relocate, assign labor, and track work orders across garage spots
- 👥 **Customer & Vehicle Management** — Full CRUD with vehicle assignment
- 🔧 **Repair Task Catalog** — Manage tasks with parts, labor costs, and estimated durations
- 🧾 **Invoice Generation** — Auto-generate PDF invoices when work orders are completed
- 💳 **Payment Settlement** — Mark invoices as paid
- 📡 **Real-Time Notifications** — SignalR hub pushes live updates to connected clients
- 🔐 **JWT Authentication** — Secure login with refresh token support
- 📊 **Dashboard Stats** — Today's revenue, completion rates, profit margins, and more
- 🗓️ **Daily Schedule View** — 15-minute slot availability grid per garage spot
- 🤖 **Automated Cleanup** — Background job auto-cancels overdue bookings

---

## 🏗️ Architecture

The project follows **Clean Architecture** with strict layer separation:

```
MechanicShop/
├── src/
│   ├── MechanicShop.Domain          # Entities, Value Objects, Domain Events, Result pattern
│   ├── MechanicShop.Application     # CQRS Handlers, Validators, Pipeline Behaviors
│   ├── MechanicShop.Infrastructure  # EF Core, Identity, Caching, SignalR, PDF, Background Jobs
│   ├── MechanicShop.Contracts       # Shared request/response models
│   ├── MechanicShop.Api             # Controllers, Middleware, OpenAPI, Program.cs
│   └── MechanicShop.Client          # Blazor WebAssembly frontend (🚧 In Progress)
├── tests/
│   ├── MechanicShop.Tests.Common              # Shared factories and test utilities
│   ├── MechanicShop.Domain.UnitTests          # Domain logic tests
│   ├── MechanicShop.Application.UnitTests     # Handler and behavior tests
│   ├── MechanicShop.Application.SubcutaneousTests  # Full pipeline tests with real DB
│   └── MechanicShop.Api.IntegrationTests      # End-to-end HTTP tests
```

### Dependency Flow
```
Api → Application → Domain
Infrastructure → Application → Domain
Contracts → Api / Application
```

The **Domain** layer has zero external dependencies. The **Application** layer depends only on interfaces. The **Infrastructure** layer provides the implementations.

---

## 🛠️ Tech Stack

| Category | Technology |
|---|---|
| **Framework** | ASP.NET Core (.NET 10) |
| **Architecture** | Clean Architecture + CQRS |
| **Mediator** | MediatR |
| **Database** | SQL Server + Entity Framework Core |
| **Caching** | HybridCache (Memory + Redis) |
| **Authentication** | ASP.NET Identity + JWT Bearer |
| **Real-Time** | SignalR |
| **Validation** | FluentValidation |
| **PDF Generation** | QuestPDF |
| **Background Jobs** | .NET Hosted Services |
| **Logging** | Serilog |
| **Observability** | OpenTelemetry + Prometheus |
| **API Docs** | Scalar + Swagger UI |
| **Rate Limiting** | ASP.NET Core Rate Limiter |
| **Containerization** | Docker + Docker Compose |
| **Testing** | xUnit + NSubstitute + Testcontainers |

---

## 🧠 Key Design Decisions

### Result Pattern
Instead of throwing exceptions for expected failures, every operation returns a `Result<T>` — either a success value or a list of typed errors. This makes failure handling explicit and consistent across all layers.

### Pipeline Behaviors
Cross-cutting concerns are handled as five MediatR pipeline behaviors:
- **ValidationBehavior** — runs FluentValidation before every handler
- **CachingBehavior** — caches query results with tag-based invalidation
- **LoggingBehavior** — logs every request with user context
- **PerformanceBehavior** — detects and logs slow requests exceeding 500ms
- **UnhandledExceptionBehavior** — catches and logs unexpected exceptions at the pipeline level before rethrowing

### Domain Events
Entities raise domain events (e.g. `WorkOrderCompleted`) that are dispatched inside `SaveChangesAsync`. This decouples side effects — sending emails, pushing SignalR notifications — from the core business logic.

### Auditing
An EF Core `SaveChangesInterceptor` automatically stamps `CreatedAt`, `CreatedBy`, `LastModifiedAt`, and `LastModifiedBy` on every entity — no handler needs to think about it.

---

## 🧪 Testing Strategy

The project uses a **four-tier testing pyramid** with **332 tests**:

| Layer | Type | Description |
|---|---|---|
| Domain | Unit Tests | Pure business rule validation — no infrastructure |
| Application | Unit Tests | Handler logic with mocked dependencies |
| Application | Subcutaneous Tests | Full pipeline with real Docker SQL Server |
| API | Integration Tests | End-to-end HTTP requests through the full stack |

Subcutaneous and integration tests use **Testcontainers** to spin up a real SQL Server in Docker for each test run — no mocking of the database.

---

## 🚀 Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Run with Docker Compose

```bash
git clone https://github.com/A-hmedMustafa/MechanicShop.git
cd MechanicShop
docker-compose up --build
```

The API will be available at `https://localhost:7094`

### Run locally

```bash
cd src/MechanicShop.Api
dotnet run
```

### Run tests

```bash
dotnet test
```

> ⚠️ Subcutaneous and integration tests require Docker to be running — they spin up a SQL Server container automatically via Testcontainers.

---

## 📖 API Documentation

When running in development, two interactive API explorers are available:

- **Scalar UI** → `https://localhost:7094/scalar`
- **Swagger UI** → `https://localhost:7094/swagger`

All endpoints are versioned under `/api/v1/`.

---

## 🔐 Default Credentials (Development Seed)

| Role | Email | Password |
|---|---|---|
| Manager | `pm@localhost` | `pm@localhost` |
| Labor | `john.labor@localhost` | `john.labor@localhost` |
| Labor | `peter.labor@localhost` | `peter.labor@localhost` |

---

## 📂 Domain Overview

```
Customers
  └── Vehicles

Employees (Labor / Manager)

RepairTasks
  └── Parts

WorkOrders
  ├── RepairTasks (many-to-many)
  ├── Vehicle
  ├── Labor (Employee)
  └── Invoice
       └── InvoiceLineItems
```

---

## 👤 Author

**Ahmed Mostafa**
- GitHub: [@A-hmedMustafa](https://github.com/A-hmedMustafa)
- LinkedIn: [Ahmed Mostafa](https://www.linkedin.com/in/ahmed-mostafa-63037b3b9/)

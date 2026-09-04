# MediBook — Clinic Appointment & Practice Management API

MediBook is a production-style .NET 10 Web API for managing clinic operations — 
patients, doctors, multi-location clinics, appointment scheduling, and role-based 
authentication. Built as a hands-on backend engineering exercise following 
Clean Architecture principles, with every design decision (domain modeling, 
EF Core relationships, business rules, and security) made deliberately and 
documented through iterative development.

## Features

- **Domain-driven entity design**: Patients, Doctors, Clinics, Clinic Locations, 
  Specialties, Appointment Types, and Appointments with carefully considered 
  relationships and cardinalities.
- **Clean Architecture**: Four-project structure (Domain → Application → 
  Infrastructure → Api) with strict dependency direction — Domain has zero 
  external references.
- **Scheduling engine**: Timezone-aware appointment slot generation derived from 
  doctor working hours, appointment type durations, and existing bookings — with 
  correct handling of IANA timezone conversion, cross-midnight edge cases, and 
  interval-overlap conflict detection.
- **Business rule enforcement**: Doctor-clinic assignment validation, working-hour 
  overlap prevention, appointment conflict detection, and entity-level invariant 
  protection (e.g., appointment cancellation state machine).
- **Result pattern**: Custom `Result`/`Result<T>` types with typed errors 
  (`Validation`, `NotFound`, `Conflict`) mapped cleanly to HTTP status codes via a 
  shared `ApiBaseController`.
- **Authentication & Authorization**: ASP.NET Core Identity with JWT access tokens 
  and refresh token rotation, decoupled from domain entities (a `Patient`/`Doctor` 
  can exist independently of a login account), with role-based endpoint protection.
- **EF Core**: Fluent API configuration throughout, deliberate `DeleteBehavior` 
  choices distinguishing historical records (Restrict) from pure relationship 
  tables (Cascade), composite keys for many-to-many joins.

## Tech Stack

- .NET 8 / ASP.NET Core Web API
- Entity Framework Core (SQL Server)
- ASP.NET Core Identity + JWT Bearer Authentication
- xUnit + Moq (unit testing)

## Architecture
MediBook.Domain → Entities, enums. Zero external references.
MediBook.Application → Interfaces, services, DTOs, Result pattern, business logic.
MediBook.Infrastructure → EF Core, repositories, Identity, DI wiring.
MediBook.Api → Controllers, Program.cs, JWT configuration.


## Status

Actively in development. Core CRUD, scheduling, and authentication are complete. 
Upcoming: broader authorization refinement, transactions, concurrency handling, 
caching, and integration testing.

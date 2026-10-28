# 🗄️ TokoBuku.Infrastructure

Infrastructure and external persistence layer powered by **Entity Framework Core** and **ASP.NET Core Identity**.

### Layer Scope:
- **Data**:
  - `ApplicationDbContext`: EF Core entity mappings and Fluent API configurations.
  - `Configurations`: Entity-to-table schemas for Microsoft SQL Server (`TokoBukuV2Db`).
  - `Migrations`: Relational database migration history.
- **Identity**: Authentication and role-based access management (*Administrator* & *Cashier*).
- **Services**: External infrastructure adapters (Date/Time provider, audit logging, file storage).

> 🔒 *Portfolio Notice: Implementation files (`*.cs`) in this layer are maintained in a private repository to protect proprietary business logic.*

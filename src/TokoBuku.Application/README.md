# ⚙️ TokoBuku.Application

Application orchestration and use-case layer structured around **CQRS (Command Query Responsibility Segregation)** using **MediatR**.

### Layer Scope:
- **Features**:
  - `Books`: Catalog curation, promotional pricing, restocking, and margin calculations.
  - `Sales`: Cashier POS checkout and digital customer ordering.
  - `Purchases`: Inbound procurement workflows from suppliers.
  - `Customers` & `Suppliers`: Partner directories and customer relationship management.
- **Common Behaviors**: Validation pipeline behavior (FluentValidation), Performance & Logging behavior.
- **Interfaces**: Persistence and external service abstraction contracts (`IApplicationDbContext`, `IUnitOfWork`, `ICurrentUserService`).

> 🔒 *Portfolio Notice: Implementation files (`*.cs`) in this layer are maintained in a private repository to protect proprietary business logic.*

# 🧪 Test Suites

Comprehensive automated testing suites ensuring system reliability and architectural integrity.

### Test Coverage:
- **TokoBuku.Domain.UnitTests**: Pure unit tests verifying domain entity methods and business invariant protection.
- **TokoBuku.Application.UnitTests**: Unit tests for CQRS command/query handlers and FluentValidation pipelines.
- **TokoBuku.RegressionTests**: Architectural boundary guards powered by **NetArchTest** ensuring Clean Architecture rules (e.g., Domain layer has zero outgoing project references).

> 🔒 *Portfolio Notice: Test suite implementation files (`*.cs`) are maintained in a private repository.*

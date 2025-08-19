# 📖 Library System – README

## Overview
This project is an early implementation of a **Library System** built with **ASP.NET Core**.  
Development is done using **Test-Driven Development (TDD)**: writing tests first, then adding the minimal implementation.  

**Currently implemented:**
- **Domain Layer**
  - Entities: `Book`, `Loan`
  - Business logic in `LoanDomain.Borrow` and `LoanDomain.Return`
  - Guard clauses for invalid cases (empty `userId`, `now` in the past/future, unavailable book, etc.)
  - **11 unit tests** (`LoanDomainTests.cs`) covering `Borrow` rules
- **Application Layer**
  - `LoanService` with methods: `BorrowAsync`, `ReturnAsync`, `ListActiveLoansAsync`
  - Repository interfaces: `IBookRepository`, `ILoanRepository`
  - In-memory fake repositories for testing
  - **5 unit tests** (`LoanServiceTests.cs`) covering main service flows:
    - Happy path (Borrow, Return)
    - Error cases (Book not found, Loan not found)
    - Listing active loans

---

## Project Structure
```
Library.sln
 ├─ Library.Domain
 │   ├─ Book.cs
 │   ├─ Loan.cs
 │   └─ LoanDomain.cs
 │
 ├─ Library.Application
 │   ├─ Abstractions
 │   │   ├─ IBookRepository.cs
 │   │   └─ ILoanRepository.cs
 │   └─ Services
 │       └─ LoanService.cs
 │
 ├─ Library.Tests
 │   ├─ LoanDomainTests.cs
 │   └─ LoanServiceTests.cs
 │
 └─ (Infrastructure, Api) – not yet implemented
```

---

## Tech Stack
- **.NET 8 / C# 12**
- **xUnit** – unit testing framework
- **ConcurrentDictionary-based fake repositories** – simple in-memory persistence
- **TDD workflow** – red-green-refactor cycle

---

## How to Run Tests
1. Clone the repository:
   ```bash
   git clone <repo-url>
   cd Library
   ```
2. Run all tests:
   ```bash
   dotnet test
   ```

Expected result: all tests should pass ✅

---

## Next Steps
- **Infrastructure**: EF Core InMemory DbContext + Repository implementations
- **API**: ASP.NET Core Web API (Controllers: Books, Users, Loans)
- **Dependency Injection** setup (Service + Repositories + DbContext)
- **Swagger** for API documentation
- **Exception Middleware** (map KeyNotFound → 404, InvalidOperation → 409, ArgumentException → 400)

---

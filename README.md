# 📖 Library System

## Overview
This project is an implementation of a **Library System** built with **ASP.NET Core**.  
Development was intentionally limited to MVP scope and executed with **Test-Driven Development (TDD)**: failing test → minimal implementation → refactor.  
Avoided nonessential features (YAGNI) to move fast.

---

## Tech Stack
- **.NET 9 / C# 13**
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

**Currently implemented:**
- **API Layer**
  - **BooksController**:
    - `POST /api/books` – Create
    - `PUT /api/books/{id}` – Update
    - `DELETE /api/books/{id}` – Delete (only if no active loans)
    - `GET /api/books` – List all (filters: `isAvailable`, `author`)
    - `GET /api/books/{id}` – Get by Id
  - **LoansController**:
    - `POST /api/loans` – Borrow a book
    - `POST /api/loans/{id}/return` – Return a book
    - `GET /api/loans` – List active loans
    - `GET /api/loans/{id}` – Get by Id
  - **UsersController**:
    - `POST /api/users` – Register
    - `GET /api/users` – List all
    - `GET /api/users/{id}` – Get by Id
- **Application Layer**
  - `BookService`: Create, Update, Delete (restricted if on loan), List with filters
  - `LoanService`: Borrow, Return, List active loans
  - `UserService`: Register (validation, unique email), List all
  - Repository interfaces: `IBookRepository`, `ILoanRepository`, `IUserRepository`
  - In-memory fake repositories for testing
  - **5 unit tests** (`LoanServiceTests.cs`) covering main service flows:
    - Happy path (Borrow, Return)
    - Error cases (Book not found, Loan not found)
    - Listing active loans
- **Domain Layer**
  - Entities: `Book`, `Loan`, `User`
  - Business logic in `LoanDomain.Borrow` and `LoanDomain.Return`
  - Guard clauses for invalid cases (empty `userId`, `now` in the past/future, unavailable book, etc.)
  - **11 unit tests** (`LoanDomainTests.cs`) covering `Borrow` rules
- **Infrastructure Layer**
  - Repositories:
    - `BookRepository`: full CRUD, filtering by availability & author
    - `LoanRepository`: create, update, get by id, list active, check for active loans
    - `UserRepository`: add, list, get by id/email (email uniqueness enforced)
  - **EF Core InMemory database** with `LibraryDbContext`
- **Error Handling**
  - Global `ProblemDetailsMiddleware`
    - Maps exceptions to proper HTTP codes with RFC7807 JSON responses:
      - 400 (bad request / invalid arguments)
      - 404 (not found)
      - 409 (conflict – e.g., return twice, delete book on loan)
      - 500 (internal server error)
- **Testing**
  - **Unit tests** with `xUnit`:
    - `LoanDomainTests`: borrow/return business rules
    - `BookServiceTests`: CRUD, filters, active loan restriction
    - `UserServiceTests`: registration, email uniqueness
    - `LoanServiceTests`: service-level operations
  - Integration tests (next step): full API via WebApplicationFactory

---

## Project Structure

```
Library.sln
 ├─ Library.Api
 │   ├─ Contracts
 │   │   ├─ BooksDtos.cs
 │   │   ├─ LoansDtos.cs
 │   │   └─ UsersDtos.cs
 │   ├─ Contollers
 │   │   ├─ BooksController.cs
 │   │   ├─ LoansController.cs
 │   │   └─ UsersController.cs
 │   ├─ Middleware
 │   │   └─ ProblemDetailsMiddleware.cs
 │   └─ Program.cs
 │
 ├─ Library.Application
 │   ├─ Abstractions
 │   │   ├─ IBookRepository.cs
 │   │   ├─ ILoanRepository.cs
 │   │   └─ IUserRepository.cs
 │   ├─ Books
 │   │   ├─ BookDtos.cs
 │   │   └─ BookService.cs
 │   ├─ Loans
 │   │   └─ LoanService.cs
 │   ├─ Users
 │   │   ├─ UserDtos.cs
 │   │   └─ UserService.cs
 │
 ├─ Library.Domain
 │   ├─ Book.cs
 │   ├─ Loan.cs
 │   ├─ LoanDomain.cs
 │   └─ User.cs
 │
 ├─ Library.Infrastructure
 │   ├─ Data
 │   │   └─ LibraryDbContext.cs
 │   └─ Repositories
 │       ├─ BookRepository.cs
 │       ├─ LoanRepository.cs
 │       └─ UserRepository.cs
 │
 ├─ Library.Tests
 │   ├─ BookServiceTests.cs
 │   ├─ LoanDomainTests.cs
 │   ├─ LoanServiceTests.cs
 │   ├─ ReturnDomainTests.cs
 │   └─ UserServiceTests.cs
 │
 └─ WapProj
```

## Roadmap
- ✅ Domain + business logic
- ✅ Book CRUD + filters
- ✅ User registration + listing
- ✅ Loan borrow/return/list
- ✅ ProblemDetails global error handling
- ✅ Unit testing
- 🔜 Integration tests
- 🔜 Pagination/sorting
- 🔜 Switch to relational DB with migrations

RaceDay

RaceDay is a web-based event management system for South African road running, walking and cycling events (Comrades, Cape Town Cycle Tour, park runs, etc). A lot of these events still run on paper and spreadsheets, so this project builds a proper system for it.

There are two types of users:

Organisers can create events, add categories, view enrolments and capture results
Participants can browse events, enter them and view their own results

This is an individual project done in 3 parts:

Part 1: Planning (ERD, API endpoint plan, SQL script)
Part 2: The API (this part)
Part 3: Not started yet

Part 2: What’s built

An ASP.NET Core Web API that follows my Part 1 plan. It connects to SQL Server using Entity Framework Core and has role-based session authentication, Swagger documentation, unit tests, and a GitHub Actions workflow that runs the tests.

Tech used
ASP.NET Core Web API (.NET 8, C#)
Entity Framework Core (SQL Server)
SQL Server
Cookie-based session authentication
BCrypt for password hashing
Swagger (Swashbuckle)
xUnit, Moq and the EF Core in-memory provider for tests
GitHub Actions for CI


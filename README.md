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

Folder structure

/RaceDay          - the API project
  /Controllers    - Auth, Events, Categories, Entries, Results, Users
  /Data           - RaceDayContext (EF Core)
  /Models         - entity classes and DTOs
/RaceDay.Tests    - unit tests
/docs             - API endpoint plan
/scripts          - SQL script (create tables)
/.github/workflows/ci.yml  (or dotnet.yml) - CI workflow
README.md

Database

Six tables: USERS, EVENT, CATEGORY, ENTRIES, RESULTS, ROUTES.

Organiser → many Events
Event → many Categories
Event → many Routes
Participants ↔ Events is many-to-many, resolved through ENTRIES (via CATEGORY)
Entry → one Result

I used EF Core, but the tables come from my SQL script rather than migrations. The models are mapped to the table names in RaceDayContext, so the schema still matches my Part 1 ERD.

Changes from Part 1
Added DESCRIPTION and DISTANCE to EVENT, because the brief says every event must capture them
Changed the date, distance and fee columns from NVARCHAR to proper types (DATETIME2, FLOAT, DECIMAL) so they match the C# models
Renamed the Route model to RaceRoute because it clashed with ASP.NET’s own Route class
The password hash column in USERS is called PASSWORD, and it’s mapped to PasswordHash in the model

How to run it
Run the SQL script in /scripts on your SQL Server. It creates the RACEDAY database and all 6 tables.
Open appsettings.json and set the connection string to your server:
   "DefaultConnection": "Server=YOUR_SERVER;Database=RACEDAY;Trusted_Connection=True;TrustServerCertificate=True;"
   


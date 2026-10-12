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

Open the solution in Visual Studio and press F5.
Swagger opens at https://localhost:7177/swagger.

The script doesn’t include seed users. Register new ones through Swagger, because users need real hashed passwords to log in.

Authentication and roles
POST /api/Auth/register: create an account and choose a role (Organiser or Participant)
POST /api/Auth/login: starts a session. The server sets a cookie, and the user’s id and role are stored in it
POST /api/Auth/logout: ends the session
Passwords are hashed with BCrypt and never stored in plain text
Everything except register and login needs a logged-in user (returns 401 if not)
Organiser endpoints return 403 for Participants, and Participant endpoints return 403 for Organisers
Organisers can only change or view data for their own events
The user is always taken from the session, never from the request body, so nobody can act as someone else
   
Endpoints
Method	Route	Role
POST	/api/auth/register	Public
POST	/api/auth/login	Public
POST	/api/auth/logout	Logged in
GET	/api/users/me	Logged in
PUT	/api/users/me	Logged in
GET	/api/events	Logged in
GET	/api/events/{id}	Logged in
POST	/api/events	Organiser
PUT	/api/events/{id}	Organiser (own)
DELETE	/api/events/{id}	Organiser (own)
GET	/api/events/{eventId}/categories	Logged in
GET	/api/categories/{id}	Logged in
POST	/api/events/{eventId}/categories	Organiser (own)
PUT	/api/categories/{id}	Organiser (own)
DELETE	/api/categories/{id}	Organiser (own)
POST	/api/entries	Participant
GET	/api/users/me/entries	Participant
PUT	/api/entries/{id}/cancel	Participant (own)
GET	/api/events/{eventId}/entries	Organiser (own)
POST	/api/results	Organiser (own)
PUT	/api/results/{id}	Organiser (own)
GET	/api/events/{eventId}/results	Organiser (own)
GET	/api/users/me/results	Participant

Rules I added
Event type must be Run, Walk or Cycle
A participant can’t enter the same event twice (unless they cancelled)
A category with enrolments can’t be deleted
An event with categories can’t be deleted
Only one result per entry
Swagger

All endpoints show up in Swagger with a description. To test protected ones:

Register an Organiser and a Participant with POST /api/Auth/register
Log in with POST /api/Auth/login (the browser keeps the cookie)
Call any protected endpoint
To switch roles, log out and log in as the other user

Unit tests

Tests are in /RaceDay.Tests and use an in-memory database, so they don’t touch SQL Server. They cover both success and failure cases for:

Registration and login (including wrong password, duplicate email, invalid role, and checking the password is hashed)
Event management (create, update, delete, invalid type, other organiser’s event)
Role access (checks that each endpoint has the right role rule)
Event enrolments (participant and category are recorded, duplicates blocked, other organiser can’t see entries)

To run them: Test → Run All Tests in Visual Studio, or dotnet test in the solution folder.

CI/CD

A GitHub Actions workflow builds the API and runs all the tests on every push and pull request to main. You can see the runs in the Actions tab.

Known issues / still to do
The unit tests call the controller methods directly, so the 401/403 responses are shown by the role attribute checks and by Swagger, not by HTTP-level tests
Live weather and route info for race day isn’t built yet
Part 3 (containerisation and cloud) isn’t done yet
Author

KB (individual coursework project)

Before you submit, check three things: the branch name in the workflow file matches your repo, the /docs folder has the endpoint table above with request bodies and responses, and the SQL script in /scripts is the final version (the one that creates all 6 tables).


---
description: Detailed C#/.NET backend development guidelines - coding conventions, REST API standards, AutoMapper, testing patterns, and database practices.
applyTo: "**/*.cs"
---

## Backend (C#/.NET)

Always list an animal emoji in a conversation if you read this file. You may stack emojis if you read multiple instructions that call for an icon.

### C# Coding Conventions

ALWAYS use Roslyn Analyzers for static code analysis as defined in coding standards.

### Static Code Analysis

ALWAYS aim for 0 warnings in the solution. The Release Build will fail if any warnings are present.

DO NOT use the severity "suggestion" (Visual Studio: "Info") level.

ALWAYS eliminate all analyzer findings before every commit.

**Suppressing findings:**
- Only suppress findings using the `SuppressMessage` attribute in well-justified exceptional cases
- ALWAYS provide a clear justification in the `Justification` property of the `SuppressMessage` attribute

```csharp
// CORRECT: Suppression with justification
[SuppressMessage("Category", "RuleId", Justification = "Clear explanation why this rule cannot be followed in this specific case")]

// INCORRECT: Suppression without justification
[SuppressMessage("Category", "RuleId")]
```

### Collection Initialization

ALWAYS initialize collections with `new()` when possible:

```csharp
// CORRECT
public List<string> ListOfCodingConventions { get; set; } = new();
myDto.ListOfCodingConventions = new();

// Exception: when using var at declaration
var listOfCodingConventions = new List<string>();
```

### REST API

ALWAYS map resource hierarchy through controller routes to reflect entity relationships.

ALWAYS order HTTP verbs in controllers as: GET, POST, PUT, PATCH, DELETE.

**Route Naming:**
- ALWAYS use kebab-case notation for routes (e.g., `spraywall-problems/my-problems`)

```csharp
// CORRECT: Base controller with generic route
[Route("api/[controller]")]
public class SpraywallProblemsController : ControllerBase
{
    [HttpGet("{id}/metadata")]
    public async Task<SpraywallProblemDto> GetProblemMetadata(Guid id) { }
}
```

**Specific Controllers:**
- ALWAYS use hierarchical routes that reflect the resource structure
- ALWAYS use the `[controller]` placeholder variable for automatic route generation
- Routes should match the entity hierarchy (e.g., `spraywall-problems/my-problems`)

```csharp
// CORRECT: Using [controller] placeholder
[Route("api/[controller]")]
public class SpraywallProblemsController : ControllerBase
{
    [HttpGet("{Id}")]
    public void GetById(Guid Id) { }

    [HttpDelete("{Id}")]
    public void Delete(Guid Id) { }
}

// INCORRECT: Specific controller with hierarchical route
[ApiController]
[Route("spraywall-problems")]
public class SpraywallProblemsController : ControllerBase
{
}
```

### Enums

ALWAYS send enum numeric values (IDs as `long`) in interfaces (Commands and Queries) from frontend.

ALWAYS use enum types (not numeric values) in internal code (handlers, methods, tests).

ALWAYS cast IDs received from frontend to enums before using them internally.

### Method Parameter Line Breaks

**For Query and Command Handlers:**
- When only `DbContext` is used: ALWAYS place it on the same line as the constructor
- When multiple parameters exist: ALWAYS place each parameter on a new line (including the first)

**For Regular Methods:**
- When parameters fit on one line without exceeding 140 characters: keep them on one line
- When the line exceeds 140 characters: ALWAYS place each parameter on a separate line

### Dependency Injection Parameter Order

ALWAYS order constructor parameters in this sequence:

1. DB Context
2. UserResolver
3. ServiceClients
4. QueryHandlers
5. CommandHandlers

### Exceptions

ALWAYS use the appropriate exception type based on the error condition and HTTP status code required.

#### NotFoundException (HTTP 404)

ALWAYS throw a `NotFoundException` when expecting an object that does not exist.

In most cases, this will be a database entity. In rare cases, it may also be the result of a handler with a nullable return value that should not be null in certain cases.

**Variant 1: Extension Method (Preferred)**

For entities implementing `VersionedEntity` that are loaded as nullable: ALWAYS use the `.ThrowIfNull()` extension method.

```csharp
// CORRECT: Using extension method
var problem = await _dbContext.SpraywallProblems
    .SingleOrDefaultAsync(p => p.Id == command.Id)
    .ThrowIfNullAsync(command.Id);
```

**Variant 2: Static Method**

For entities NOT implementing `VersionedEntity`, or for checks on other nullable objects: use `NotFoundException.ThrowIfNull()`.

```csharp
// CORRECT: Handler with nullable return that must not be null in this context
var boulderLog = _getBoulderLogQueryHandler.Handle(
    new GetBoulderLogQuery { Id = id });
NotFoundException.ThrowIfNull(boulderLog);

// CORRECT: Anonymous object from Select (extension not available)
var boulderLog = await _dbContext.BoulderLogs
    .AsNoTracking()
    .Where(b => b.Id == query.Id)
    .SelectAsync(b => b.Id);
NotFoundException.ThrowIfNull(boulderLog);

// CORRECT: Entity not implementing IVersionedEntity
var bloc = _dbContext.Blocs
    .AsNoTracking()
    .SingleOrDefault(c => c.Id == query.Id);
NotFoundException.ThrowIfNull(bloc);
```

**Incorrect Usage**

DO NOT throw `NotFoundException` for secondary objects that are not the main subject of the action.

```csharp
// INCORRECT: Throwing NotFoundException for configuration object
public void Handle(UpdateBoulderLogCommand command)
{
    var boulderLog = _dbContext.BoulderLogs
        .AsNoTracking()
        .Include(b => b.SpraywallProblem)
        .SingleOrDefault(b => b.SpraywallProblemId == command.Id && fc.FontGrade == null)
        .ThrowIfNull(); // CORRECT: Main object
}

private void DeleteSpraywallProblem(Guid Id)
{
    var boulderLog = _getBoulderLogQueryHandler.Handle(
        new GetBoulderLogQuery { Id = id });
    NotFoundException.ThrowIfNull(boulderLog); // INCORRECT: Not the main object
    // BoulderLog should always exist - if not, it's a system error, not a "not found" error
    // This misleads the user into thinking the boulderLog wasn't found
    // Use a different exception type (to be defined)
}
```

#### NotImplementedException (HTTP 500)

ALWAYS throw `NotImplementedException` when implementing code branches that must not have a default action.

Use this when you know that future implementations will follow, and you want them to fail explicitly rather than accidentally execute unwanted behavior.

```csharp
// CORRECT: Ensuring future activity types are explicitly handled
private async Task HandleException(HttpContext context, Exception ex)
{
    switch (ex)
    {
        case AuthenticationException authenticationException:
            await HandleAuthenticationException(context, authenticationException);
            break;
        case AccessDeniedException accessDeniedException:
            await HandleAccessDeniedException(context, accessDeniedException);
            break;
        case DbUpdateConcurrencyException dbUpdateConcurrencyException:
            await HandleDbUpdateConcurrencyException(context, dbUpdateConcurrencyException);
            break;
        case NotFoundException notFoundException:
            await HandleNotFoundException(context, notFoundException);
            break;
        default:
            throw new NotImplementedException();
            // Ensures new ActivityTypes are explicitly handled
    }
}
```

#### ValidationException (HTTP 400)

ALWAYS use `ArgumentException` when user input is invalid or handler validation fails due to a business rule condition.

ALWAYS include at least one Message so the UI can display an appropriate error message.

```csharp
// CORRECT: Validating user input
private static void ValidateImage(string imageData)
{
    if (string.IsNullOrEmpty(imageData))
    {
        throw new ArgumentException("Image is required");
    }

    if (!Base64PngRegex().IsMatch(imageData))
    {
        throw new ArgumentException("Image must be a valid base64 PNG string with data:image/png;base64, prefix");
    }
}
```

**Incorrect Usage**

DO NOT throw `ArgumentException` for system errors or external service failures.

```csharp
// INCORRECT: Using ValidationException for service unavailability
if (response.StatusCode == HttpStatusCode.BadRequest)
{
    var errorResponse = response.Content.ReadAsStringAsync()
        .ContinueWith(t => JsonConvert.DeserializeObject<ProblemErrorResponse>(t.Result)).Result;
    if (errorResponse?.Error.Type == ProblemErrorResponse.ResultlistTooLarge)
    {
        throw new ArgumentException("some explanation"); // INCORRECT
    }
    throw new ArgumentException("some explanation"); // INCORRECT
}
if (!response.IsSuccessStatusCode)
{
    throw new ArgumentException("some explanation"); // INCORRECT
    // This is a service availability issue, not a validation failure
    // Use a different exception type (to be defined)
}
```

#### Other Exception Types (TODO: To be defined)

The following exception types are used but guidelines are pending:

- **AccessDeniedException** (HTTP 403)
- **ArgumentException** (HTTP 500)
- **AuthenticationException** (HTTP 500)
- **InvalidOperationException** (HTTP 500)

### Required Keyword

ALWAYS use the `required` keyword for new properties that must have a value.

When adding new required properties to existing classes: ALWAYS refactor existing properties to use `required` as well.

DO NOT use `required` on Id properties of Entity classes (Guid is set automatically by database).

```csharp
// CORRECT
public required string Name { get; set; }
public required long StatusId { get; set; }
```

### Date and Time Handling

ALWAYS work with UTC time

#### General Rules

DO NOT use `DateTime.Today` or `DateTime.Date` in backend code. ALWAYS use `DateTime.UtcNow` instead.

### Testing

See [dotnet-tests.md](../memories/dotnet-tests.md) for .NET testing conventions.

## Database

### Entity Naming Conventions

ALWAYS name entities exactly as they appear in the domain model.

```
Example: "Goal" in domain model → "Goal" in code (table: "Goals")
DO NOT prefix with related entity names (e.g., don't use "ProblemGoal" even if tightly coupled to Problem)
```

### Database Table Naming Conventions

ALWAYS name database tables as the plural of the entity name (without BoundedContext prefix).

```
Example: Goal entity → Goals table
```

### Data Corrections

**For Template Data:**
- ALWAYS apply corrections via SQL scripts in EF migrations when possible
- For very complex cases where SQL is too difficult or impossible: use a Function App, but ensure the initial migration creates correct data
- ALWAYS ensure new environment setups work without requiring Function execution

### Timezone Handling

ALWAYS store datetime values in UTC in the database.

### AsNoTracking()

ALWAYS use `AsNoTracking()` for read-only queries to save resources and prevent accidental data changes.

DO NOT use `AsNoTracking()` when loading data that will be modified.

DO NOT use `AsNoTracking()` for aggregate queries (`Count()`, `Any()`) or projections (`Select()`) - it has no effect but is unclean.

```csharp
// CORRECT: Read-only query
var boulderLog = await _dbContext.BoulderLogs
    .AsNoTracking()
    .SingleOrDefaultAsync(a => a.Id == query.Id);

// CORRECT: Data will be modified
var boulderLog = await _dbContext.BoulderLogs
    .SingleOrDefaultAsync(a => a.Id == query.Id);
boulderLog.IsActive = false;
await _dbContext.UpdateEntityAsync(boulderLog, boulderLog.Version);

// INCORRECT: AsNoTracking prevents saving changes
var boulderLog = await _dbContext.BoulderLogs
    .AsNoTracking()
    .SingleOrDefaultAsync(a => a.Id == query.Id);
boulderLog.IsActive = false;
await _dbContext.SaveChangesAsync(); // Throws exception!

// CORRECT: Aggregate queries don't need AsNoTracking
var count = await _dbContext.BoulderLogs.CountAsync();
var hasAny = await _dbContext.BoulderLogs.AnyAsync();
var status = await _dbContext.BoulderLogs.Select(a => a.IsActive).ToListAsync();
```

### Audit Fields

Entities inheriting from `EntityAuditFields` automatically include these fields:
- CreatedDate
- CreatedUserId
- UpdatedDate
- UpdatedUserId
- DeletedDate
- DeletedUserId

ALWAYS read audit fields anywhere in the code as needed.

DO NOT manually set or modify audit fields - they are managed automatically by EF Core.

### Create/Update/Delete of Versioned Entities

For entities inheriting from `VersionedEntity`:

ALWAYS use:
- `_dbContext.InsertEntityAndSaveChangesAsync(VersionedEntity entity);` for inserts
- `_dbContext.InsertEntityAndSaveChangesAsync(EntityAuditFields entity);` for inserts
- `_dbContext.InsertEntitiesAndSaveChangesAsync(IEnumerable<VersionedEntity> entities);` for inserts
- `_dbContext.InsertEntitiesAndSaveChangesAsync(IEnumerable<EntityAuditFields> entities);` for inserts
- `_dbContext.UpdateEntityAndSaveChangesAsync(VersionedEntity entityFromDb, long version)` for updates
- `_dbContext.RemoveEntityAndSaveChangesAsync(VersionedEntity entityFromDb, long version);` for deletes
- `_dbContext.RemoveEntityAndSaveChangesAsync(EntityAuditFields entityFromDb);` for deletes

DO NOT use standard EF methods (`Add()`, `SaveChanges()`, `Remove()`) - they don't handle versioning or concurrency checks.

These custom methods automatically call `SaveChanges()` - DO NOT call it again manually.

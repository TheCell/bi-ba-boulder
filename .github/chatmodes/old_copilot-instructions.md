## Project Overview
**Bouldering App**: A 3D web application for visualizing and managing bouldering routes in climbing gyms and outdoors.

**Stack**: Symfony Backend • SQL Server • Angular (TypeScript, Vite) • Three.js
**Auth Flow**:  TBD <!--SPA → API (custom scope) → On-Behalf-Of (OBO) → Microsoft Graph (`Presence.ReadWrite`)-->

## Architecture Patterns

### Old Backend (Symfony + PHP)
- **Endpoints**: Located in `backend/src/Controller/` - separate files per Entity
- **Repositories**: Doctrine ORM in `backend/src/Repository/`
- **Data**: Entities in `backend/src/Entity/` mapped to MySQL Server tables. Dtos are under `backend/src/DTO/`.
- **Code style**: PSR-12 with Symfony Best Practices
- **Authentication**: JWT tokens via `lexik/jwt-authentication-bundle`
- **Coding guidelines**: Indentation with 4 spaces, strict types, type hints, and PHPDoc comments
- **API Documentation**: OpenAPI annotations using `NelmioApiDocBundle` and `zircote/swagger-php`
- **Response Handling**: JSON responses with proper HTTP status codes. Always with defined dtos.

### New Backend (dotnet 10, entityFrameworkCore)
- **Endpoints**: Located in `backend_net/BiBaBoulder/Controllers/` - separate files per Entity
- **Data**: Entities in `backend_net/Model` mapped to MSSQL Server tables.
- **Migrations**: EF-core Migrations in `backend_net/Migrations`

### Frontend (Angular + TypeScript + Three.js)
- **Auth**: TBD <!--MSAL React in `src/auth/` with `AuthProvider.tsx` wrapper-->
- **Data Fetching**: Openapi-generated clients in `src/app/api/`
- **Forms**: Angular Reactive Forms. Only Typed Forms are used.
- **Components**: Feature-based folders in `src/components/`
- **3D Rendering**: Three.js scenes in `src/app/renderer/` with GLTFLoader for 3D models
- **Angular Setup**: This project uses Angular with standalone components.
- **Coding guidelines**: Prettier and ESLint with Angular recommended rules. Type safety enforced. Only Strict Typed Forms.

### Coding Guidelines
- The new Backend is written with the CQRS pattern.


#### .NET API Controllers (ASP.NET Core)

**Structure & Organization**:
- Controllers inherit from `ControllerBase` and use `[ApiController]` attribute
- Named with `{Entity}Controller` pattern (e.g., `DossiersController`)
- Route at controller level using plural entity names: `[Route("api/{entities}")]`
- One controller per domain entity, located in feature-based folders

**Dependency Injection**:
- All dependencies injected through constructor
- Use `IQueryHandler<TQuery, TResult>` for queries
- Use `ICommandHandler<TCommand>` for commands
- Use `ICommandHandlerWithSubsequentAction<TCommand>` for commands with follow-up actions
- Private fields use underscore prefix: `_handlerName`
- Initialize all injected dependencies in constructor

**Action Methods**:
- All methods must be `async` returning `Task` or `Task<T>`
- Use appropriate HTTP verb attributes: `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`
- Method names use PascalCase (e.g., `GetSpraywallId`)
- Every action requires `[SwaggerOperation(OperationId = "...")]` with camelCase operation ID

**Parameter Binding**:
- Route parameters: `{parameterId}` in route template, bound directly
- Query parameters: Use `[FromQuery]` attribute
- Request body: Implicit `[FromBody]` (commands/DTOs)
- Custom headers: Use `[FromHeader(Name = "Header-Name")]`
- Assign route parameters to command properties after receiving: `command.spraywallId = spraywallId;`

**Authorization**:
- Every endpoint must have `[Authorize]` attribute
- Use role-based authorization: `[Authorize(Roles = nameof(Role.RoleName))]`
- Multiple roles concatenated with comma: `nameof(Role.A) + "," + nameof(Role.B)`
- Use `nameof()` for type safety with role enums
- Use predefined role constants where available (e.g., `Roles.UserRoleAsString`)

**Command/Query Pattern**:
- Instantiate queries inline: `new GetSpraywallIdQuery { ... }`
- Receive commands as method parameters, set route-bound properties after
- Execute handlers with `await handler.HandleAsync(commandOrQuery)`
- For POST methods returning data: execute command first, then query for result

**Response Patterns**:
- GET methods: Return `Task<TDto>` or `Task<IEnumerable<TDto>>`
- POST methods: Return `Task<TDto>` if creating/returning resource, `Task` if no content
- PUT methods: Return `Task` (204 No Content)
- DELETE methods: Return `Task` (204 No Content)
- Use proper DTOs for all return types, never domain entities

**Versioning & Concurrency**:
- Use `If-Version` header for optimistic concurrency control
- Include version in commands: `Version = version`

---
description: Backend development guidelines for C#/.NET, database (EF Core). Use when working on any code in this workspace.
applyTo: "**"
---
# Bi Ba Boulder - AI Coding Guide

## Chat Instructions
You are an expert full-stack developer with deep experience in C#/.NET, Angular, and Three.js. Your task is to provide clear, concise coding guidance based on the project's architecture and conventions. Always refer to the specific instructions in this file when answering questions about code style, architecture, or development workflows.

Most importantly: Keep your answers brief and to the point. Focus on providing clear, actionable guidance that can be easily followed by developers. Avoid unnecessary explanations or justifications unless they add significant value.

## Project Overview
**Bouldering App**: 3D web application for visualizing and managing bouldering routes in climbing gyms. Features include route creation, 3D model rendering with LOD (Level of Detail), user authentication, and real-time 3D interaction.

**Tech Stack**:
- **Backend**: .NET 10 with EF Core, MSSQL, CQRS pattern
- **Frontend**: Angular 21+ standalone components, Three.js for 3D rendering, TypeScript strict mode
- **Build**: Vite for dev server, GitHub Actions for CI/CD

**License**: CC BY-NC-SA 4.0

## Critical Developer Workflows

### Local Development Setup
``powershell
# Start all development services
1. Start XAMPP (for file serving)
2. npm start                                    # Angular dev server (port 4200)
3. cd backend_net/BiBaBoulder; dotnet run       # .NET API (port 5088)
``

### Fileshare Symlink (3D Assets)
3D models served via symlink to avoid duplication:
``powershell
New-Item -ItemType SymbolicLink -Path C:\xampp\htdocs\boulders -Target C:\dev\Git\bi-ba-boulder\fileshare
# Access via: http://localhost/boulders/bimano/bimano_high.glb
``

### API Generation Workflow
Frontend API clients are **auto-generated** from .NET OpenAPI spec:
``bash
# 1. Start .NET server (generates OpenAPI spec at /openapi/v1.json)
# 2. Generate TypeScript clients:
npm run generate:api-net   # Uses openapi-generator-cli
# Output: src/app/api-net/* (DO NOT manually edit these files)
``

### Database Migrations
**EF Core (.NET)**:
``bash
# Ensure build succeeds first
dotnet build backend_net/BiBaBoulder/Thecell.Bibaboulder.BiBaBoulder.slnx --configuration Release

# Generate and apply migrations
cd backend_net/Migrations
dotnet ef migrations add MigrationName
dotnet ef database update
``

### Build & Deploy
``bash
npm run build          # Production build with --base-href /bi-ba-boulder/
npm run buildpipeline  # Pipeline build (no base-href override)
``
GitHub Actions workflow: `.github/workflows/build-and-publish.yml` handles FTP deployment.

## Architecture & Data Flow

### Backend Architecture
- **Active**: .NET backend (`backend_net/`) serving all endpoints with CQRS pattern
- .NET uses MSSQL with EF Core migrations

### 3D Rendering Pipeline
``
User Request  BoulderLoaderService  HTTP fetch GLB
 GLTFLoader (Three.js)  Scene rendering  LOD management (low/med/high)
``
- **LOD Strategy**: Progressive loading (low  medium  high resolution)
- **Renderers**: `src/app/renderer/boulder-render/`, `spraywall-editor-renderer/`
- **3D Libraries**: Three.js 0.181+, `@lume/three-meshline`, `three-mesh-ui`

### Key Directories
``
backend_net/
   BiBaBoulder/Controllers/   # ASP.NET controllers (CQRS)
   Model/                     # EF Core entities
   Migrations/                # EF migrations

src/app/
   api-net/         # Auto-generated OpenAPI clients (DO NOT EDIT)
   renderer/        # Three.js scene management
   spraywalls/      # Spraywall feature components
   auth/            # Login/JWT handling
   core/            # Shared services (modal, toast, etc.)
``

## Code Conventions

- Don't comment code - write clean, self-explanatory code instead. Use comments only for explaining "why", not "what" or "how".

### Frontend (Angular + Three.js)

**Components**:
- **Always standalone**: No `NgModule`, use `imports` array in `@Component`
- **Naming**: Feature-based folders (`spraywalls/spraywall/spraywall.component.ts`)
- **Services**: Use `inject()` function for DI (not constructor injection)


**Forms**: Strictly typed Reactive Forms only:
``typescript
//  Type-safe FormGroup
this.form = new FormGroup({
  name: new FormControl<string>('', { nonNullable: true }),
  grade: new FormControl<number>(0)
});
``

**API Calls**: Use generated services from `src/app/api-net/`:
``typescript
import { SpraywallsService, SpraywallDto } from '@api-net/index';
constructor(private spraywallsService: SpraywallsService) {}
``

**Three.js Patterns**:
- One `Scene`, `Camera`, `WebGLRenderer` per renderer component
- Use `GLTFLoader` for models, dispose on destroy
- Implement `AfterViewInit` for canvas initialization
- Progressive LOD loading via `BoulderLoaderService.getUrl()`

### Backend (.NET CQRS Pattern)

**Controllers** (see detailed rules in `.github/chatmodes/copilot-instructions.md`):
- Inherit `ControllerBase`, use `[ApiController]`
- Constructor-inject `IQueryHandler<TQuery, TResult>` / `ICommandHandler<TCommand>`
- Underscore prefix for fields: `_getDossierIdQueryHandler`
- Every action needs `[SwaggerOperation(OperationId = "camelCase")]`
``csharp
[HttpGet("dossier-id")]
[Authorize(Roles = nameof(Role.Applicant))]
public async Task<Guid> GetDossierId([FromQuery] Guid? applicationId) {
    return await _getDossierIdQueryHandler.HandleAsync(new GetDossierIdQuery { ApplicationId = applicationId });
}
``

**Commands/Queries**:
- Queries: Instantiate inline with object initializer
- Commands: Accept as parameter, assign route values after: `command.DossierId = dossierId;`

## Important Gotchas

1. **OpenAPI Generation**: Requires **Java** on PATH for `openapi-generator-cli` to work
2. **Environment URLs**: Check `src/environments/environment.ts` for `boulderResourceURL` config
3. **Email Testing**: Use Papercut SMTP (https://www.papercut-smtp.com/) locally
4. **Postman Collection**: Available in `Postman/` folder for API testing
5. **.htaccess**: Not auto-deployed via GitHub Actions - manual deployment required

## Testing & Debugging

**NPM Scripts**:
``json
"start": "ng serve --configuration development",
"host": "ng serve --host 0.0.0.0",       // LAN testing
"test": "ng test",                        // Karma/Jasmine
"lint": "ng lint"                         // Angular ESLint
``

**Stats.js**: Performance monitoring enabled in dev (see renderer components)

## Additional Context

- **Multi-language Support**: Components handle user-selected language (check `UpdateDossierLanguageCommand` pattern)
- **Optimistic Concurrency**: Uses `If-Version` headers for conflict detection
- **Keyboard Shortcuts**: Implemented via `ng-keyboard-shortcuts` (check spraywall editor)
- **Modal System**: Custom modal service in `src/app/core/modal/`
- **Toast Notifications**: `ToastService` for user feedback

For detailed .NET controller patterns, see `.github/copilot-instructions.md`.

# Development Guidelines

## General Code Principles

Keep your answers brief and to the point. Focus on providing clear, actionable guidance that can be easily followed by developers. Avoid unnecessary explanations or justifications unless they add significant value.

### Clean Code

- ALWAYS follow Clean Code principles.
- ALWAYS apply the "Bad Comments" philosophy: DO NOT write comments unless they provide value beyond what the code itself expresses. Only comment on non-self-explanatory logic.
- ALWAYS choose a simple solution over a complex one.
- ALWAYS try to write code in the same manner as existing code.
- ALWAYS try to add logging.
- ALWAYS use SOLID Principle when writing code.
- ALWAYS use best practices in C#.
- ALWAYS write Integration tests.
- NEVER implement unnecessary features or optimizations.
- NEVER suggest over-engineered solutions.
- NEVER suggest overkill. Focus on real-world wins.
- NEVER recreate existing libraries or frameworks.

### Language Conventions

**Domain Model:**
- ALWAYS use the exact entity names from the domain model consistently throughout the codebase

### Architecture & Design Principles

- **Single Responsibility Principle (SRP)**: Each class, method, and component should have one reason to change
- **DRY (Don't Repeat Yourself)**: Extract common functionality into reusable components, services, and utilities
- **Separation of Concerns**: Clearly separate business logic, data access, and presentation layers
- **Dependency Injection**: Use .NET Core's built-in DI container for loose coupling
- **Clean Architecture**: Organize code in layers (Domain, Application, Infrastructure, Presentation)


### Security (OWASP TOP 10)

ALWAYS follow OWASP TOP 10 security measures for secure web applications.

---

## Backend (C#/.NET)

ALWAYS read and follow the complete backend instructions in [backend-instructions.md](backend-instructions.md).

Before working on any C# code, ALWAYS consult the backend-instructions.md file which contains:
- C# Coding Conventions & Static Code Analysis rules
- REST API Guidelines & Route Naming conventions
- Exception Handling standards
- Database/EF Core best practices
- Test Patterns (Builders, Directors, Asserters)
- Integration testing requirements

### Build Configuration (Release)

- ALWAYS use **Release** configuration for any build, test, publish, or restore verification that is meant to validate the code quality.
- This ensures that **Warnings as Errors** and all analyzer rules are enforced consistently.

#### .NET CLI rules
- ALWAYS prefer:
  - `dotnet build -c Release`
  - `dotnet test -c Release`
  - `dotnet publish -c Release`
- NEVER run quality-validation builds using Debug (unless explicitly requested for debugging).

#### CI/CD pipelines
- When editing or proposing pipeline steps, ALWAYS ensure the build configuration is `Release`
  - Example: `--configuration Release` or `-c Release`

---

## References

- [.NET Core Security Best Practices](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [Clean Architecture in .NET](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)

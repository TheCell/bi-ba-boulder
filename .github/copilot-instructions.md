# Bi Ba Boulder - AI Coding Guide

## Project Overview
**Bouldering App**: 3D web application for visualizing and managing bouldering routes in climbing gyms. Features include route creation, 3D model rendering with LOD (Level of Detail), user authentication, and real-time 3D interaction.

**Tech Stack**:
- **Legacy Backend**: PHP 8+ with Symfony 6, MySQL/MSSQL, Doctrine ORM, JWT auth via `lexik/jwt-authentication-bundle`
- **New Backend**: .NET 10 with EF Core, MSSQL, CQRS pattern (in migration)
- **Frontend**: Angular 21+ standalone components, Three.js for 3D rendering, TypeScript strict mode
- **Build**: Vite for dev server, GitHub Actions for CI/CD

**License**: CC BY-NC-SA 4.0

## Critical Developer Workflows

### Local Development Setup
``powershell
# Start all development services
1. Start XAMPP (for MySQL/file serving)
2. npm start                                    # Angular dev server (port 4200)
3. cd backend; symfony server:start             # Symfony API (port 8000)
4. (Optional) cd backend_net; dotnet run        # .NET API
``

### Fileshare Symlink (3D Assets)
3D models served via symlink to avoid duplication:
``powershell
New-Item -ItemType SymbolicLink -Path C:\xampp\htdocs\boulders -Target C:\dev\Git\bi-ba-boulder\fileshare
# Access via: http://localhost/boulders/bimano/bimano_high.glb
``

### API Generation Workflow
Frontend API clients are **auto-generated** from Symfony OpenAPI spec:
``bash
# 1. Start Symfony server (generates OpenAPI spec at /api/doc.json)
# 2. Generate TypeScript clients:
npm run generate:api   # Uses openapi-generator-cli 7.9.0
# Output: src/app/api/* (DO NOT manually edit these files)
``

### Database Migrations
**Symfony**:
``bash
cd backend
symfony console doctrine:database:create           # First time only
symfony console make:migration                     # Generate migration
symfony console doctrine:migrations:migrate        # Apply migration
symfony console doctrine:migrations:status         # Check status
``

**.NET**
``bash
dotnet build backend_net/Bibaboulder/Thecell.Bibaboulder.BiBaBoulder.slnx --configuration Release    # Ensure build succeeds
``

**EF Core (.NET)**:
``bash
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

### Backend Migration Status
- **Active**: Symfony backend (`backend/`) serving all endpoints
- **In Progress**: .NET backend (`backend_net/`) - porting to CQRS with Command/Query handlers
- Both share same MSSQL database during transition

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
backend/src/
   Controller/      # Symfony REST endpoints
   Entity/          # Doctrine ORM entities (DB schema)
   DTO/             # Data Transfer Objects for API responses
   Repository/      # DB query methods
   Security/        # JWT auth, email verification

backend_net/
   BiBaBoulder/Controllers/   # ASP.NET controllers (CQRS)
   Model/                     # EF Core entities
   Migrations/                # EF migrations

src/app/
   api/             #  Auto-generated OpenAPI clients (DO NOT EDIT)
   renderer/        # Three.js scene management
   spraywalls/      # Spraywall feature components
   auth/            # Login/JWT handling
   core/            # Shared services (modal, toast, etc.)
``

## Code Conventions

### Frontend (Angular + Three.js)

**Components**:
- **Always standalone**: No `NgModule`, use `imports` array in `@Component`
- **Naming**: Feature-based folders (`spraywalls/spraywall/spraywall.component.ts`)
- **Services**: Use `inject()` function for DI (not constructor injection)
``typescript
//  Preferred pattern
public loginService = inject(LoginTrackerService);
private http = inject(HttpClient);
``

**Forms**: Strictly typed Reactive Forms only:
``typescript
//  Type-safe FormGroup
this.form = new FormGroup({
  name: new FormControl<string>('', { nonNullable: true }),
  grade: new FormControl<number>(0)
});
``

**API Calls**: Use generated services from `src/app/api/`:
``typescript
import { SpraywallsService, SpraywallDto } from '@api/index';
constructor(private spraywallsService: SpraywallsService) {}
``

**Three.js Patterns**:
- One `Scene`, `Camera`, `WebGLRenderer` per renderer component
- Use `GLTFLoader` for models, dispose on destroy
- Implement `AfterViewInit` for canvas initialization
- Progressive LOD loading via `BoulderLoaderService.getUrl()`

### Backend (Symfony PHP)

**Controllers**:
- Extend `AbstractController`, use `#[Route('/api/...')]`
- **Always** return DTOs, never raw entities
- OpenAPI annotations: `#[OA\Response]`, `#[OA\RequestBody]`
``php
#[Route('/api/spraywalls', methods: ['GET'])]
#[OA\Response(response: 200, content: new OA\JsonContent(type: 'array', items: new Model(type: SpraywallDto::class)))]
public function index(): JsonResponse {
    return $this->json($this->spraywallRepository->findAllAsDto());
}
``

**Entities**:
- Doctrine annotations/attributes for ORM mapping
- Generate via: `symfony console make:entity`
- Use UUIDs for primary keys (see existing patterns)

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
6. **XAMPP PHP Extensions**: Enable `gd2`, `openssl`, `sodium` in `xampp/php/php.ini`
7. **JWT Setup**: Run `symfony console lexik:jwt:generate-keypair` and set passphrase in `.env.local`

## Testing & Debugging

**Symfony Debug Commands**:
``bash
symfony console debug:router              # List all routes
symfony console debug:config security     # Security config
symfony console cache:clear               # Clear cache
``

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

For detailed .NET controller patterns, see `.github/chatmodes/copilot-instructions.md`.

## Project Overview
**Bouldering App**: A 3D web application for visualizing and managing bouldering routes in climbing gyms and outdoors.

**Stack**: Symfony Backend • SQL Server • Angular (TypeScript, Vite) • Three.js
**Auth Flow**:  TBD <!--SPA → API (custom scope) → On-Behalf-Of (OBO) → Microsoft Graph (`Presence.ReadWrite`)-->

## Architecture Patterns

### Backend (Symfony + PHP)
- **Endpoints**: Located in `backend/src/Controller/` - separate files per Entity
- **Repositories**: Doctrine ORM in `backend/src/Repository/`
- **Data**: Entities in `backend/src/Entity/` mapped to MySQL Server tables
- **Code style**: PSR-12 with Symfony Best Practices
- **Authentication**: JWT tokens via `lexik/jwt-authentication-bundle`
- **Coding guidelines**: Indentation with 4 spaces, strict types, type hints, and PHPDoc comments

### Frontend (Angular + TypeScript + Three.js)
- **Auth**: TBD <!--MSAL React in `src/auth/` with `AuthProvider.tsx` wrapper-->
- **Data Fetching**: Openapi-generated clients in `src/app/api/`
- **Forms**: Angular Reactive Forms. Only Typed Forms are used.
- **Components**: Feature-based folders in `src/components/`
- **3D Rendering**: Three.js scenes in `src/app/renderer/` with GLTFLoader for 3D models
- **Angular Setup**: This project uses Angular with standalone components.
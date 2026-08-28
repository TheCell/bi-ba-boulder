---
description: Reusable .NET backend patterns for authenticated user resolution and domain services.
applyTo: "**/*.cs"
---

# .NET Memory

Reusable patterns for safe, consistent .NET backend implementation.

## Resolve the Current User as Non-Nullable

When backend code requires the authenticated user, obtain the non-nullable user through the throwing service method:

```csharp
var currentUser = await currentUserService.GetCurrentUserOrThrowAsync();
```

Use the returned `currentUser` directly instead of carrying a nullable user through subsequent logic; the method enforces the authenticated-user invariant by throwing when no current user exists.

## Assert New Models and Their DTOs

When a project uses model assertion helpers, create a dedicated `<ModelName>Assertion` for every new model class. Whenever a DTO is populated with information from that model, verify it with the model's assertion helper so mapped fields remain covered consistently.

## Test Handlers with Unit Tests

Cover CQRS command and query handlers with unit tests by default. Use integration tests when behavior depends on the HTTP pipeline, database integration, or other infrastructure that unit tests cannot exercise in isolation.

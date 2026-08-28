---
description: Project conventions for mapping .NET domain entities to API DTOs.
applyTo: "backend_net/**/*.cs"
---

# .NET Mapping Memory

Centralize entity-to-DTO transformations so mapping behavior stays reusable and consistent across backend features.

## Use Model Mapping Extensions

Always map entities to DTOs through a dedicated static extension mapper in `backend_net/Model/Mapping`. Name methods `MapTo<DtoName>` and reuse them from handlers and services instead of constructing DTOs inline.

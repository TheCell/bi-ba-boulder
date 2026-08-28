---
description: Workspace conventions for modern Angular and TypeScript frontend development.
applyTo: "**/*.ts, **/*.html"
---
# Frontend Memory

Use current Angular and TypeScript patterns consistently, with explicit types and signal-based component APIs.

## Modern Angular syntax

Use the most modern Angular syntax supported by the project. Prefer signal-based APIs such as `input()` and `output()` over decorator-based `@Input()` and `@Output()` for new code.

## Explicit return values

Always declare explicit return types for functions, methods, callbacks, and other executable members. Keep return values intentional and type-safe.

## Signals for inputs, outputs, and forms

Use Angular signals for component inputs and outputs and for form state and handling. Prefer signal-based forms over legacy decorator- or subscription-driven form patterns when implementing new frontend features.

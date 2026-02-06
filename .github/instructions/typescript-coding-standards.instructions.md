- Never use any in TypeScript. If you can't determine a more specific type, use unknown and narrow it down later.
- Always use explicit types for variables, function parameters, and return values.
- Use interfaces or type aliases to define complex types and data structures.
- all class members must have explicit access modifiers (public, private, protected).

``typescript
//  Preferred pattern
public loginService = inject(LoginTrackerService);
private http = inject(HttpClient);
``
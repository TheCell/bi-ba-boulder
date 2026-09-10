---
description: Frontend development guidelines for Angular and TypeScript. Use when working on any code in this workspace.
applyTo: "**/*.ts, **/*.html"
---
# Bi Ba Boulder - AI Coding Guide

Always list an animal emoji in a conversation if you read this file. You may stack emojis if you read multiple instructions that call for an icon.

# General Guidelines

We always use the latest Angular version. We use the Angular CLI to generate components, services, and modules. We use RxJS for reactive programming. We use SCSS for styling.

Do not use comments in the code unless it is necessary to explain a complex logic. Use meaningful variable and function names. Use TypeScript's type system to enforce types. Use Angular's dependency injection system to manage services. Use Angular's signal API for form handling. Use Angular's router for navigation.

## API

We consume an openAPI generated API located in the folder `src/public/api-test`. The API is generated from the backend code. We use the `openapi-generator-cli` to generate the API client.

## Code Examples

If subscriptions are not selfclosing

### Dos

An angular component is structured in the following way:
First the imports, then the component decorator, then the class definition. The class definition contains in this order the constructor, lifecycle hooks, public methods, and private methods.
Concerning variables: All variables are listed at the start of a class. Variables are split in 3 categories. Injections come first, then come decorators, then public variables and getters followed by private variables and getters.
```ts
export class MyTimer implements OnInit, OnDestroy {
  @ViewChild('MyExample') myTimer: MyExampleComponent;
  private dataService = inject(DataService);

  public dogs = signal<unknown>(null);
  public secondsRunning = signal(0);

  private subscription = new Subscription();
  private timer = timer(0, 1000);

  constructor() {
    // placeholder
  }

  public ngOnInit(): void {
    // placeholder
  }

  public ngOnDestroy(): void {
    // placeholder
  }

  public myMethod(): void {
    // placeholder
  }

  private myPrivateMethod(): void {
    // placeholder
  }
}
```

// todo: add signal form example

### Don'ts
never correct
```ts
styleUrl: './my-timer.scss'
```
to
```ts
stylesUrl: ['./my-timer.scss']
```
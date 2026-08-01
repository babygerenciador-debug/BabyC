# Coding Conventions

**Analysis Date:** 2026-08-01

## Naming Patterns

**Backend (C#/.NET):**
- **Classes:** PascalCase - `DriverService`, `VehicleRepository`, `TripAggregate`
- **Interfaces:** Prefix with `I` - `IDriverRepository`, `ITripService`, `IUnitOfWork`
- **Methods:** PascalCase - `CreateDriver()`, `CalculateFuelAverage()`, `ScheduleTrip()`
- **Variables:** camelCase - `driverName`, `totalDistance`, `cancellationToken`
- **Constants:** PascalCase for strongly-typed constants; UPPER_SNAKE_CASE only for external integrations
- **Files:** Match the main class name - `DriverService.cs`, `TripController.cs`
- **Namespaces:** File-scoped namespaces (`namespace FleetOS.Domain.Common;`)
- **Records:** PascalCase - `LoginCommand`, `UserDto`, `Error`

**Frontend (TypeScript/React):**
- **Components:** PascalCase - `DriverFormModal`, `MainLayout`, `DashboardPage`
- **Functions:** camelCase - `handleSubmit`, `onSubmit`, `toggleMenu`
- **Variables:** camelCase - `queryClient`, `isMobileMenuOpen`, `accessToken`
- **Types/Interfaces:** PascalCase - `AuthUser`, `Driver`, `Trip`, `ApiResponse<T>`
- **Hooks:** camelCase with `use` prefix - `useAuthStore`, `useSignalR`, `useThemeStore`
- **Files:** Match component/function name - `DriverFormModal.tsx`, `api.ts`, `useAuthStore.ts`
- **Constants:** UPPER_SNAKE_CASE or camelCase - `API_BASE_URL`, `FORM_ID`

## Code Style

**Backend:**
- **File-scoped namespaces** preferred over block-scoped
- **Primary constructors** for dependency injection in handlers and services
- **Sealed classes** for commands, handlers, and DTOs to prevent inheritance
- **Readonly collections** - `IReadOnlyList<T>` for exposing lists
- **Async/await** throughout with `CancellationToken` parameter
- **Nullable reference types** enabled (`<Nullable>enable</Nullable>`)
- **Implicit usings** enabled (`<ImplicitUsings>enable</ImplicitUsings>`)

**Frontend:**
- **TypeScript strict mode** enabled
- **Functional components** only (no class components)
- **Arrow functions** for component definitions - `export default function ComponentName() {}`
- **Destructuring** for props and hook returns
- **Optional chaining** and **nullish coalescing** - `user?.name`, `data ?? []`
- **Type inference** with `z.infer<typeof schema>` for form data
- **Interface definitions** inline or in dedicated `types/` directory

## Import Organization

**Backend:**
1. `System.*` namespaces
2. `Microsoft.*` namespaces
3. Third-party packages (`MediatR`, `FluentValidation`, `EntityFrameworkCore`)
4. Project namespaces (`FleetOS.Domain.*`, `FleetOS.Application.*`, `FleetOS.Shared.*`)

**Frontend:**
1. React and React libraries (`react`, `react-router-dom`, `react-hook-form`)
2. Third-party libraries (`@tanstack/react-query`, `axios`, `zod`, `zustand`, `lucide-react`)
3. Internal services (`../../services/api`)
4. Internal stores (`../../store/useAuthStore`)
5. Internal components (`../../components/shared/BaseModal`)
6. Internal hooks (`../../hooks/useSignalR`)
7. Styles (`./ComponentName.css`)

**Path Aliases:** Not configured - use relative paths

## Error Handling

**Backend - Result Pattern:**
```csharp
// Use Result<T> for business logic, NOT exceptions
public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken ct)
{
    if (user == null)
        return Result.Failure<LoginResponse>(Error.Auth.InvalidCredentials);
    
    if (user.IsLockedOut())
        return Result.Failure<LoginResponse>(Error.Auth.UserBlocked);
    
    // Success case
    return Result.Success(response);
}
```

**Error Definition:**
```csharp
// In FleetOS.Shared/Results/Error.cs
public sealed record Error(string Code, string Description)
{
    public static class Auth
    {
        public static readonly Error InvalidCredentials = 
            new("Auth.InvalidCredentials", "Invalid email/CPF or password.");
        public static readonly Error UserBlocked = 
            new("Auth.UserBlocked", "This account has been blocked.");
    }
    
    public static Error NotFound(string entity, object id) =>
        new($"{entity}.NotFound", $"{entity} with id '{id}' was not found.");
}
```

**Controller Pattern:**
```csharp
var result = await Mediator.Send(command, cancellationToken);
return result.IsSuccess 
    ? Ok(result.Value) 
    : BadRequest(result.Error);
```

**Frontend - API Error Handling:**
```typescript
// In services/api.ts - Axios interceptors
api.interceptors.response.use(
  (response) => {
    // Success toast for mutations
    if (method !== 'get' && response.status >= 200 && response.status < 300) {
      const msg = getSuccessMessage(method, response.config.url ?? '');
      if (msg) toast.success(msg);
    }
    return response;
  },
  async (error) => {
    // Auto-refresh token on 401
    if (error.response?.status === 401 && !originalRequest._retry) {
      // Refresh token logic
    }
    // Show error toast
    const msg = error.response?.data?.title || error.message || 'Erro inesperado';
    toast.error(msg);
    return Promise.reject(error);
  }
);
```

## Validation

**Backend - FluentValidation:**
```csharp
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Identifier)
            .NotEmpty().WithMessage("Identifier is required.");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
        
        RuleFor(x => x.TenantSlug)
            .NotEmpty()
            .When(x => IsCpf(x.Identifier))
            .WithMessage("Tenant is required for driver login.");
    }
}
```

**Frontend - Zod + React Hook Form:**
```typescript
const driverSchema = z.object({
  name: z.string().min(3, 'Nome é obrigatório'),
  email: z.string().email('Email inválido'),
  password: z.string().min(6, 'Senha deve ter no mínimo 6 caracteres'),
  cpf: z.string().min(11, 'CPF é obrigatório'),
});

type DriverFormData = z.infer<typeof driverSchema>;

const { register, handleSubmit, formState: { errors } } = useForm<DriverFormData>({
  resolver: zodResolver(driverSchema)
});

// In JSX
<input {...register('name')} />
{errors.name && <span className="error-msg">{errors.name.message}</span>}
```

## Logging

**Backend - Serilog:**
- Required fields: `CorrelationId`, `TenantId`, `UserId`, `Timestamp`, `Action`, `Duration`
- Never log: passwords, tokens, sensitive data
- Use structured logging with message templates

**Frontend:**
- Limited console usage (only in hooks for connection status)
- Use `sonner` toast notifications for user-facing messages
- Avoid `console.log` in production code

## Comments

**Backend:**
- XML documentation for public APIs and base classes
```csharp
/// <summary>
/// Aggregate root with domain events support.
/// All aggregate roots MUST inherit from this class.
/// </summary>
public abstract class AggregateRoot : Entity { }
```

**Frontend:**
- Minimal comments
- Self-documenting code preferred
- JSDoc not enforced

## Function Design

**Backend:**
- **Async methods** return `Task<T>` or `Task<Result<T>>`
- **CancellationToken** as last parameter with default value
- **Single responsibility** - one handler per command/query
- **Max 3-4 parameters** - use records/DTOs for more

**Frontend:**
- **Component size:** Keep under 200 lines, extract sub-components
- **Hook composition:** Break complex logic into custom hooks
- **Props:** Use TypeScript interfaces, max 5-6 props
- **Return:** JSX or null, use early returns for guards

## Module Design

**Backend:**
- **Feature-based folders** - `Features/Drivers/`, `Features/Vehicles/`
- **CQRS separation** - `Commands/` and `Queries/` subfolders
- **One command/query per file** - `CreateDriverCommand.cs`, `GetDriversQuery.cs`
- **Handler colocation** - `CreateDriverCommandHandler.cs` next to command
- **Validator colocation** - `CreateDriverCommandValidator.cs` next to command

**Frontend:**
- **Feature-based pages** - `pages/drivers/`, `pages/fleet/`
- **Component colocation** - `pages/drivers/components/DriverList.tsx`
- **Shared components** - `components/shared/BaseModal.tsx`
- **Layout components** - `components/layout/MainLayout.tsx`
- **Services** - `services/api.ts`
- **Stores** - `store/useAuthStore.ts`
- **Hooks** - `hooks/useSignalR.ts`
- **Types** - `types/index.ts`

## Git Conventions

**Branch naming:**
```
main
develop
feature/*
fix/*
hotfix/*
```

**Commit messages** - Conventional Commits:
```
feat(drivers): add driver registration
fix(trips): validate overlapping schedules
refactor(finance): simplify cash flow service
docs(api): update trip endpoints
```

## Code Review Checklist

Before approving a PR, verify:
- [ ] Architecture respected (Clean Architecture, DDD)
- [ ] Naming conventions followed
- [ ] Domain rules implemented correctly
- [ ] DTOs created (not exposing entities)
- [ ] Validators implemented with FluentValidation
- [ ] Endpoints documented
- [ ] Tests written (unit, integration, validation, authorization)
- [ ] Documentation updated
- [ ] Multi-tenant fields present (TenantId, OrganizationId, BusinessUnitId)
- [ ] Permissions applied (`[Authorize(Roles = "...")]`)
- [ ] No dead code
- [ ] No unnecessary dependencies
- [ ] No obvious duplication

---

*Convention analysis: 2026-08-01*

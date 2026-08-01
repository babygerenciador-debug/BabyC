# Testing Patterns

**Analysis Date:** 2026-08-01

## Test Framework

**Backend (.NET):**
- **Runner:** xUnit 2.9.2
- **Config:** `backend/tests/FleetOS.Tests/FleetOS.Tests.csproj`
- **Assertion Library:** FluentAssertions 6.12.2
- **Mocking:** Moq 4.20.72
- **Test Data:** Bogus 35.6.1 (fake data generation)
- **Database:** Microsoft.EntityFrameworkCore.InMemory 10.0.4

**Frontend (React/TypeScript):**
- **Status:** No test framework configured
- **Recommended:** Vitest + React Testing Library + MSW
- **Current state:** Zero test files exist

## Test File Organization

**Backend:**
```
backend/tests/FleetOS.Tests/
├── FleetOS.Tests.csproj
├── Application/
│   ├── Auth/
│   │   └── Commands/
│   │       └── Login/
│   │           └── LoginCommandHandlerTests.cs
│   ├── Fleet/
│   │   ├── Fuel/
│   │   │   └── Commands/
│   │   │       └── CreateFuelLogCommandHandlerTests.cs
│   │   ├── Vehicles/
│   │   │   └── Commands/
│   │   │       └── CreateVehicleCommandHandlerTests.cs
│   │   └── Maintenance/
│   └── Operations/
│       ├── Drivers/
│       └── Trips/
├── Domain/
│   ├── Common/
│   │   └── ValueObjects/
│   │       └── CpfTests.cs
│   └── Core/
│       ├── Users/
│       │   └── UserTests.cs
│       └── Fleet/
└── Infrastructure/
    └── Persistence/
        └── Repositories/
            └── DriverRepositoryTests.cs
```

**Frontend (to be established):**
```
frontend/src/
├── components/
│   └── shared/
│       └── BaseModal.test.tsx
├── pages/
│   └── drivers/
│       ├── DriversPage.test.tsx
│       └── components/
│           ├── DriverList.test.tsx
│           └── DriverFormModal.test.tsx
├── hooks/
│   └── useSignalR.test.ts
├── services/
│   └── api.test.ts
└── store/
    └── useAuthStore.test.ts
```

**Naming:**
- Backend: `{ClassName}Tests.cs` - `LoginCommandHandlerTests.cs`
- Frontend: `{ComponentName}.test.tsx` - `DriverFormModal.test.tsx`

## Test Structure

**Backend - xUnit Pattern:**
```csharp
using FluentAssertions;
using Moq;
using Xunit;

namespace FleetOS.Tests.Application.Auth.Commands.Login;

public sealed class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ITenantRepository> _tenantRepositoryMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly LoginCommandHandler _sut;

    public LoginCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _tenantRepositoryMock = new Mock<ITenantRepository>();
        _passwordServiceMock = new Mock<IPasswordService>();
        _jwtServiceMock = new Mock<IJwtService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _tenantRepositoryMock.Object,
            _passwordServiceMock.Object,
            _jwtServiceMock.Object,
            _unitOfWorkMock.Object,
            new Mock<IConfiguration>().Object
        );
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnAccessToken()
    {
        // Arrange
        var command = new LoginCommand("user@example.com", "password123", null);
        var user = CreateTestUser();
        
        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Identifier, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        
        _passwordServiceMock
            .Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().NotBeNullOrEmpty();
        result.Value.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ShouldReturnAuthError()
    {
        // Arrange
        var command = new LoginCommand("user@example.com", "wrongpassword", null);
        var user = CreateTestUser();
        
        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Identifier, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        
        _passwordServiceMock
            .Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
    }

    private User CreateTestUser()
    {
        // Use Bogus for realistic test data
        var faker = new Bogus Faker();
        return new User(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            faker.Person.FullName,
            faker.Internet.Email(),
            "hashed_password",
            UserRole.Manager,
            UserStatus.Active
        );
    }
}
```

**Frontend - Vitest + React Testing Library (to be established):**
```typescript
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import DriverFormModal from './DriverFormModal';
import { api } from '../../services/api';

// Mock the API module
vi.mock('../../services/api', () => ({
  api: {
    post: vi.fn(),
  },
}));

describe('DriverFormModal', () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false } },
    });
    vi.clearAllMocks();
  });

  const renderWithProviders = (ui: React.ReactElement) => {
    return render(
      <QueryClientProvider client={queryClient}>
        {ui}
      </QueryClientProvider>
    );
  };

  it('should render form fields', () => {
    renderWithProviders(<DriverFormModal onClose={vi.fn()} />);
    
    expect(screen.getByLabelText(/nome completo/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/senha/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/cpf/i)).toBeInTheDocument();
  });

  it('should show validation errors for empty required fields', async () => {
    renderWithProviders(<DriverFormModal onClose={vi.fn()} />);
    
    fireEvent.click(screen.getByRole('button', { name: /salvar motorista/i }));
    
    await waitFor(() => {
      expect(screen.getByText(/nome é obrigatório/i)).toBeInTheDocument();
      expect(screen.getByText(/email inválido/i)).toBeInTheDocument();
    });
  });

  it('should submit form with valid data', async () => {
    const mockOnClose = vi.fn();
    const mockPost = vi.mocked(api.post);
    mockPost.mockResolvedValueOnce({ data: { id: '123' } });

    renderWithProviders(<DriverFormModal onClose={mockOnClose} />);
    
    fireEvent.change(screen.getByLabelText(/nome completo/i), {
      target: { value: 'João Silva' },
    });
    fireEvent.change(screen.getByLabelText(/email/i), {
      target: { value: 'joao@example.com' },
    });
    // ... fill other fields
    
    fireEvent.click(screen.getByRole('button', { name: /salvar motorista/i }));
    
    await waitFor(() => {
      expect(mockPost).toHaveBeenCalledWith('/drivers', expect.objectContaining({
        name: 'João Silva',
        email: 'joao@example.com',
      }));
      expect(mockOnClose).toHaveBeenCalled();
    });
  });
});
```

## Mocking

**Backend - Moq:**
```csharp
// Mock repository
var mockRepo = new Mock<IDriverRepository>();
mockRepo
    .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync((Driver?)null);

// Mock with specific return value
mockRepo
    .Setup(x => x.GetByCnhAsync("123456", It.IsAny<CancellationToken>()))
    .ReturnsAsync(new Driver { CnhNumber = "123456" });

// Verify method was called
mockRepo.Verify(
    x => x.AddAsync(It.IsAny<Driver>(), It.IsAny<CancellationToken>()),
    Times.Once
);

// Mock with callback
mockRepo
    .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
    .Callback<CancellationToken>(ct => { /* side effect */ })
    .Returns(Task.CompletedTask);
```

**Frontend - MSW (Mock Service Worker) for API mocking:**
```typescript
// src/mocks/handlers.ts
import { http, HttpResponse } from 'msw';

export const handlers = [
  http.get('/api/v1/drivers', () => {
    return HttpResponse.json({
      items: [
        { id: '1', name: 'João Silva', status: 'Active' },
        { id: '2', name: 'Maria Santos', status: 'Active' },
      ],
      pageNumber: 1,
      pageSize: 10,
      totalCount: 2,
      totalPages: 1,
    });
  }),

  http.post('/api/v1/drivers', async ({ request }) => {
    const body = await request.json();
    return HttpResponse.json(
      { id: '123', ...body },
      { status: 201 }
    );
  }),
];

// src/mocks/server.ts
import { setupServer } from 'msw/node';
import { handlers } from './handlers';

export const server = setupServer(...handlers);

// src/test/setup.ts
import { beforeAll, afterEach, afterAll } from 'vitest';
import { server } from '../mocks/server';

beforeAll(() => server.listen({ onUnhandledRequest: 'error' }));
afterEach(() => server.resetHandlers());
afterAll(() => server.close());
```

## Fixtures and Factories

**Backend - Bogus for Test Data:**
```csharp
public static class TestFakers
{
    public static Faker<Driver> DriverFaker = new Faker<Driver>()
        .RuleFor(d => d.Id, f => Guid.NewGuid())
        .RuleFor(d => d.TenantId, f => Guid.NewGuid())
        .RuleFor(d => d.BusinessUnitId, f => Guid.NewGuid())
        .RuleFor(d => d.Name, f => f.Person.FullName)
        .RuleFor(d => d.CpfLast4, f => f.Random.Replace("####"))
        .RuleFor(d => d.Status, f => f.PickRandom<DriverStatus>())
        .RuleFor(d => d.CnhNumber, f => f.Random.Replace("#########"))
        .RuleFor(d => d.CnhCategory, f => f.PickRandom<string>("A", "B", "C", "D", "E"))
        .RuleFor(d => d.CnhExpiry, f => f.Date.Future());

    public static Faker<Vehicle> VehicleFaker = new Faker<Vehicle>()
        .RuleFor(v => v.Id, f => Guid.NewGuid())
        .RuleFor(v => v.Nickname, f => f.Vehicle.Model())
        .RuleFor(v => v.Plate, f => f.Random.Replace("???-####"))
        .RuleFor(v => v.Brand, f => f.Vehicle.Manufacturer())
        .RuleFor(v => v.Year, f => f.Random.Int(2015, 2024));
}

// Usage in tests
var driver = TestFakers.DriverFaker.Generate();
var drivers = TestFakers.DriverFaker.Generate(5); // Generate 5 drivers
```

**Frontend - Factory Functions:**
```typescript
// src/test/factories.ts
import { faker } from '@faker-js/faker';
import type { Driver, Vehicle, Trip } from '../types';

export const createMockDriver = (overrides?: Partial<Driver>): Driver => ({
  id: faker.string.uuid(),
  tenantId: faker.string.uuid(),
  businessUnitId: faker.string.uuid(),
  userId: faker.string.uuid(),
  name: faker.person.fullName(),
  cpfLast4: faker.string.numeric(4),
  status: 'Active',
  isCnhExpired: false,
  isAvailable: true,
  createdAt: faker.date.recent().toISOString(),
  ...overrides,
});

export const createMockVehicle = (overrides?: Partial<Vehicle>): Vehicle => ({
  id: faker.string.uuid(),
  tenantId: faker.string.uuid(),
  nickname: faker.vehicle.model(),
  plate: faker.string.alpha(7).toUpperCase(),
  brand: faker.vehicle.manufacturer(),
  model: faker.vehicle.model(),
  year: faker.number.int({ min: 2015, max: 2024 }),
  capacity: faker.number.int({ min: 4, max: 50 }),
  status: 'Active',
  isAvailableForTrip: true,
  createdAt: faker.date.recent().toISOString(),
  ...overrides,
});

// Usage in tests
const driver = createMockDriver({ name: 'João Silva' });
const drivers = Array.from({ length: 5 }, () => createMockDriver());
```

## Coverage

**Backend:**
- **Target:** 80%+ coverage for Application layer (command/query handlers)
- **Domain:** 100% coverage for business rules and value objects
- **Infrastructure:** 60%+ coverage for repositories

**Frontend:**
- **Target:** 70%+ coverage for critical user flows
- **Components:** Focus on forms, modals, and data display
- **Hooks:** 100% coverage for custom hooks

**View Coverage:**
```bash
# Backend
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html

# Frontend (when configured)
npm run test -- --coverage
```

## Test Types

**Unit Tests:**
- **Backend:** Test command/query handlers in isolation with mocked dependencies
- **Frontend:** Test individual components and hooks
- **Scope:** Single class/function, mock all dependencies

**Integration Tests:**
- **Backend:** Test repository implementations with in-memory database
- **Backend:** Test API endpoints end-to-end with test server
- **Frontend:** Test component interactions with mocked API responses

**Validation Tests:**
- **Backend:** Test FluentValidation rules
- **Frontend:** Test Zod schema validation and form error display

**Authorization Tests:**
- **Backend:** Test `[Authorize]` attributes and role-based access
- **Verify:** Different user roles can/cannot access endpoints

## Common Patterns

**Async Testing - Backend:**
```csharp
[Fact]
public async Task Handle_ShouldSaveDriver_WhenCommandIsValid()
{
    // Arrange
    var command = new CreateDriverCommand { Name = "João", Cpf = "12345678900" };
    
    // Act
    var result = await _sut.Handle(command, CancellationToken.None);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    
    _unitOfWorkMock.Verify(
        x => x.CommitAsync(
            It.IsAny<Guid>(),  // tenantId
            It.IsAny<Guid>(),  // userId
            It.IsAny<CancellationToken>()
        ),
        Times.Once
    );
}
```

**Async Testing - Frontend:**
```typescript
it('should fetch and display drivers', async () => {
  const mockDrivers = [
    createMockDriver({ name: 'João' }),
    createMockDriver({ name: 'Maria' }),
  ];
  
  server.use(
    http.get('/api/v1/drivers', () => {
      return HttpResponse.json({ items: mockDrivers });
    })
  );
  
  render(<DriverList />);
  
  // Wait for async data to load
  await waitFor(() => {
    expect(screen.getByText('João')).toBeInTheDocument();
    expect(screen.getByText('Maria')).toBeInTheDocument();
  });
});
```

**Error Testing - Backend:**
```csharp
[Fact]
public async Task Handle_WithDuplicateCpf_ShouldReturnConflictError()
{
    // Arrange
    var command = new CreateDriverCommand { Cpf = "12345678900" };
    
    _driverRepoMock
        .Setup(x => x.GetByCpfAsync(command.Cpf, It.IsAny<CancellationToken>()))
        .ReturnsAsync(new Driver()); // Driver exists
    
    // Act
    var result = await _sut.Handle(command, CancellationToken.None);
    
    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Code.Should().Be("Driver.CpfAlreadyExists");
}
```

**Error Testing - Frontend:**
```typescript
it('should display error message on API failure', async () => {
  server.use(
    http.post('/api/v1/drivers', () => {
      return HttpResponse.json(
        { code: 'Driver.CpfAlreadyExists', description: 'CPF já cadastrado' },
        { status: 400 }
      );
    })
  );
  
  render(<DriverFormModal onClose={vi.fn()} />);
  
  // Fill form and submit
  fireEvent.click(screen.getByRole('button', { name: /salvar/i }));
  
  await waitFor(() => {
    expect(screen.getByText(/cpf já cadastrado/i)).toBeInTheDocument();
  });
});
```

## Run Commands

**Backend:**
```bash
# Run all tests
dotnet test

# Run with verbosity
dotnet test --verbosity normal

# Run specific test class
dotnet test --filter "FullyQualifiedName~LoginCommandHandlerTests"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Watch mode (requires dotnet-watch)
dotnet watch test
```

**Frontend (to be established):**
```bash
# Run all tests
npm test

# Watch mode
npm run test:watch

# Coverage
npm run test:coverage

# Run specific file
npx vitest src/pages/drivers/DriversPage.test.tsx
```

## Current Status

**⚠️ Critical Gap:**
- Backend test project exists with all dependencies configured but **contains zero test files**
- Frontend has **no test infrastructure** (no test files, no test scripts, no test dependencies)

**Priority Actions:**
1. Add unit tests for critical command/query handlers (Auth, Drivers, Vehicles, Trips)
2. Add integration tests for repository implementations
3. Set up frontend test infrastructure (Vitest + RTL + MSW)
4. Add component tests for forms and critical UI flows
5. Configure CI pipeline to run tests and enforce coverage thresholds

---

*Testing analysis: 2026-08-01*

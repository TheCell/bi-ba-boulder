---
description: Workspace conventions for .NET testing in the Angular application.
applyTo: "**/*.cs"
---
## .NET Testing Guidelines

### Integration tests and unit tests

We always write integration tests and unit tests. The integration tests always has an Unauthorized / Forbidden Test where applicable and an OK test. All other tests (testing all codebranches, errors etc. is done via unittests)
All tests are ordered in the same folder structure as they are located in the solution structure. Tests for the project TheCell.Bibaboulder.Indoor are located in a folder called indoor.

We have 3 C# projects for tests

- TheCell.Bibaboulder.Integrationtests
    - This is where the integrationtests are written
- TheCell.Bibaboulder.Unittests
    - This is where the unittests are written
- TheCell.Bibaboulder.Sharedtests
    - Here are all common Classes like asserters and builders etc.

#### integration tests

The integrationtests are structured per controller. Each controller has 1 testclass ending in with the name "...ControllerTest.cs".
At the top of the integration Test we always have the endpoint uri.
Tests in the class are always grouped by the endpoint in the controller. First the anonymous test then the ok test.

This is an example integration test class

```csharp
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Indoor.Handler;
using Thecell.Bibaboulder.Model.Authorization;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Integrationtests.Indoor;

[Collection(nameof(CollectionForIntegrationTests))]
public class FeedbacksControllerTest : BaseTest
{
    private readonly string _baseUrl = "/api/Feedbacks";
    private readonly Faker _bogus;

    public FeedbacksControllerTest(IntegrationTestFactory factory) : base(factory)
    {
        _bogus = new Faker("de_CH");
    }

    [Fact]
    public async Task SendFeedback_Anonymous_Unauthorized()
    {
        var body = new { Feedback = "Some feedback" };

        var response = await Client().PostAsync(
            $"{_baseUrl}/send",
            GetJsonHttpBody(body),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SendFeedback_Authenticated_Ok()
    {
        var user = new UserBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(user);

        var client = AuthenticatedClient(
            userId: user.OidcSubject,
            role: AuthorizationRoles.User,
            username: user.Username);

        var feedbackText = _bogus.Lorem.Sentence();
        var body = new SendFeedbackCommand { Feedback = feedbackText };
        var timestamp = DateTime.UtcNow;

        var response = await client.PostAsync(
            $"{_baseUrl}/send",
            GetJsonHttpBody(body),
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();

        Assert.Equal(1, EmailService.SendCount);
        Assert.Equal("info@bibaboulder.com", EmailService.LastRecipient);
        Assert.Contains(feedbackText, EmailService.LastBody);

        var savedMail = await BiBaBoulderDbContext.Emails
            .Where(m => m.To == "info@bibaboulder.com")
            .FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(savedMail);
        Assert.Equal("info@bibaboulder.com", savedMail.To);
        Assert.Contains(user.Email, savedMail.Subject);
        Assert.Contains(feedbackText, savedMail.Body);
        Assert.True(savedMail.SentAt > timestamp);
    }
}

```

#### unit tests


The unittests are structured per class / handler. Each class has one unittestclass ending in with the name "{Class/Handlername}Test.cs".
Tests in the class always start with the error tests then the ok tests.

This is an example unit test class

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Services;
using Thecell.Bibaboulder.Indoor.Handler;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Indoor;

public class CreateSpraywallProblemTest
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly CurrentUserServiceMock _currentUserServiceMock;
    private readonly Mock<ISpraywallImageService> _imageServiceMock;

    private const string ImageData = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
    private const string ValidBase64Png = $"data:image/png;base64,{ImageData}";

    public CreateSpraywallProblemTest()
    {
        _currentUserServiceMock = new CurrentUserServiceMock();
        _imageServiceMock = new Mock<ISpraywallImageService>();
        _imageServiceMock.Setup(imageServiceMock => imageServiceMock.SaveImageAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<byte[]>()))
            .Returns(Task.CompletedTask);
        _dbContext = new DbContextMock().Build();
    }

    [Fact]
    public async Task CreateSpraywallProblem_NoUser_NotFoundException()
    {
        var spraywall = new SpraywallBuilder().Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(spraywall);
        var user = new UserBuilder()
            .SetRoles(AuthorizationRoles.Editor)
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);

        var guid = Guid.NewGuid();
        var command = new CreateSpraywallProblemCommand
        {
            Id = guid,
            SpraywallId = spraywall.Id,
            Name = "New Problem",
            Description = "A test problem",
            Image = ValidBase64Png,
            FontGrade = FontGrade.Three,
            IsCircuit = true,
            NoMatch = true,
            FreeFeet = true
        };
        var handler = new CreateSpraywallProblemCommandHandler(_dbContext, _currentUserServiceMock, _imageServiceMock.Object);

        var ex = await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.HandleAsync(command));
        Assert.Equal("No current user configured in mock.", ex.Message);

        _imageServiceMock.Verify(m => m.SaveImageAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), Convert.FromBase64String(ImageData)), Times.Never);
    }

    [Fact]
    public async Task CreateSpraywallProblem_NotEditor_UnauthorizedAccessException()
    {
        var spraywall = new SpraywallBuilder().Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(spraywall);
        var user = new UserBuilder()
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);

        var guid = Guid.NewGuid();
        var command = new CreateSpraywallProblemCommand
        {
            Id = guid,
            SpraywallId = spraywall.Id,
            Name = "New Problem",
            Description = "A test problem",
            Image = ValidBase64Png,
            FontGrade = FontGrade.Three,
            IsCircuit = true,
            NoMatch = true,
            FreeFeet = true
        };
        _currentUserServiceMock.WithUser(user);
        var handler = new CreateSpraywallProblemCommandHandler(_dbContext, _currentUserServiceMock, _imageServiceMock.Object);

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await handler.HandleAsync(command));
        Assert.Equal("User does not have the required role.", ex.Message);

        _imageServiceMock.Verify(m => m.SaveImageAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), Convert.FromBase64String(ImageData)), Times.Never);
    }

    [Fact]
    public async Task CreateSpraywallProblem_MissingImage_ArgumentException()
    {
        var spraywall = new SpraywallBuilder().Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(spraywall);
        var user = new UserBuilder()
            .SetRoles(AuthorizationRoles.Editor)
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);

        var guid = Guid.NewGuid();
        var command = new CreateSpraywallProblemCommand
        {
            Id = guid,
            SpraywallId = spraywall.Id,
            Name = "New Problem",
            Description = "A test problem",
            Image = "",
            FontGrade = FontGrade.Three,
            IsCircuit = true,
            NoMatch = true,
            FreeFeet = true
        };
        _currentUserServiceMock.WithUser(user);
        var handler = new CreateSpraywallProblemCommandHandler(_dbContext, _currentUserServiceMock, _imageServiceMock.Object);

        var ex = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await handler.HandleAsync(command));
        Assert.Equal("Image is required", ex.Message);

        _imageServiceMock.Verify(m => m.SaveImageAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), Convert.FromBase64String(ImageData)), Times.Never);
    }

    [Fact]
    public async Task CreateSpraywallProblem_NotBase64Image_ArgumentException()
    {
        var spraywall = new SpraywallBuilder().Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(spraywall);
        var user = new UserBuilder()
            .SetRoles(AuthorizationRoles.Editor)
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);

        var guid = Guid.NewGuid();
        var command = new CreateSpraywallProblemCommand
        {
            Id = guid,
            SpraywallId = spraywall.Id,
            Name = "New Problem",
            Description = "A test problem",
            Image = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==",
            FontGrade = FontGrade.Three,
            IsCircuit = true,
            NoMatch = true,
            FreeFeet = true
        };
        _currentUserServiceMock.WithUser(user);
        var handler = new CreateSpraywallProblemCommandHandler(_dbContext, _currentUserServiceMock, _imageServiceMock.Object);

        var ex = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await handler.HandleAsync(command));
        Assert.Equal("Image must be a valid base64 PNG string with data:image/png;base64, prefix", ex.Message);

        _imageServiceMock.Verify(m => m.SaveImageAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), Convert.FromBase64String(ImageData)), Times.Never);
    }

    [Fact]
    public async Task CreateSpraywallProblem_InvalidSpraywall_NotFoundException()
    {
        var user = new UserBuilder()
            .SetRoles(AuthorizationRoles.Editor)
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);

        var guid = Guid.NewGuid();
        var command = new CreateSpraywallProblemCommand
        {
            Id = Guid.NewGuid(),
            SpraywallId = guid,
            Name = "New Problem",
            Description = "A test problem",
            Image = ValidBase64Png,
            FontGrade = FontGrade.Three,
            IsCircuit = true,
            NoMatch = true,
            FreeFeet = true
        };
        _currentUserServiceMock.WithUser(user);
        var handler = new CreateSpraywallProblemCommandHandler(_dbContext, _currentUserServiceMock, _imageServiceMock.Object);

        var ex = await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.HandleAsync(command));
        Assert.Equal($"Spraywall not found. (Id: {guid})", ex.Message);

        _imageServiceMock.Verify(m => m.SaveImageAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), Convert.FromBase64String(ImageData)), Times.Never);
    }

    [Fact]
    public async Task CreateSpraywallProblem_Ok()
    {
        var spraywall = new SpraywallBuilder().Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(spraywall);

        var user = new UserBuilder()
            .SetRoles(AuthorizationRoles.Editor)
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);

        var guid = Guid.NewGuid();
        var command = new CreateSpraywallProblemCommand
        {
            Id = guid,
            SpraywallId = spraywall.Id,
            Name = "New Problem",
            Description = "A test problem",
            Image = ValidBase64Png,
            FontGrade = FontGrade.Three,
            IsCircuit = true,
            NoMatch = true,
            FreeFeet = true
        };
        _currentUserServiceMock.WithUser(user);
        var handler = new CreateSpraywallProblemCommandHandler(_dbContext, _currentUserServiceMock, _imageServiceMock.Object);
        await handler.HandleAsync(command);

        var problem = await _dbContext.SpraywallProblems.SingleAsync(cancellationToken: TestContext.Current.CancellationToken);
        SpraywallAssertion.Assert(command, problem);

        Assert.NotEqual(guid, problem.Id);
        Assert.Equal(spraywall.Id, problem.SpraywallId);
        Assert.Equal(user.Id, problem.CreatorId);

        _imageServiceMock.Verify(m => m.SaveImageAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), Convert.FromBase64String(ImageData)), Times.Once);
    }

    [Fact]
    public async Task CreateSpraywallProblem_InvalidImage_ThrowsArgumentException()
    {
        var spraywall = new SpraywallBuilder().Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(spraywall);
        var user = new UserBuilder()
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);

        var command = new CreateSpraywallProblemCommand
        {
            SpraywallId = spraywall.Id,
            Name = "New Problem",
            Description = "A test problem",
            Image = "",
            FontGrade = FontGrade.Three
        };

        _currentUserServiceMock.WithUser(user);
        var handler = new CreateSpraywallProblemCommandHandler(_dbContext, _currentUserServiceMock, _imageServiceMock.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await handler.HandleAsync(command));

        _imageServiceMock.Verify(m => m.SaveImageAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), Convert.FromBase64String(ImageData)), Times.Never);
    }
}

```

### Authorization Tests Per Endpoint

Provide authorization tests for every controller endpoint:

- `{MethodName}_Anonymous_Unauthorized()` verifies that an unauthenticated request receives `401 Unauthorized`.
- When a specific user group is not allowed, `{MethodName}_WrongUser_Forbidden()` verifies that the request receives `403 Forbidden`.

These tests guard against authorization regressions regardless of the endpoint's business logic.

```csharp
[Fact]
public async Task GetSelf_Anonymous_Unauthorized()
{
	var response = await Client().GetAsync($"{_baseUrl}/me", TestContext.Current.CancellationToken);

	Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
}

[Fact]
public async Task GetUserById_NonAdmin_Forbidden()
{
	var user = await PrepareUser();

	var client = AuthenticatedClient(
		userId: user.OidcSubject,
		role: AuthorizationRoles.User,
		username: user.Username);

	var response = await client.GetAsync($"{_baseUrl}/{Guid.CreateVersion7()}", TestContext.Current.CancellationToken);

	Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
}
```

### Test Method Naming

Name test methods with two or three parts: `Part1_Part2_Part3()`.

- Part 1: the service operation being tested.
- Part 2 (optional): the relevant parameters or test-data constellation.
- Part 3: the expected HTTP status code, written out using the `System.Net.HttpStatusCode` name.

Examples: `DevAuthController.DevLogin_NotLocalHost_NotFound()`, `UserControllerTest.GetSelf_Ok()`.

### Builders

- Name builder classes `[ClassName]Builder`.
- Store the instance being built in a private `_instance` field.
- Make setters fluent by returning `this`.
- Name setter parameters `value`.
- Provide meaningful default values and a `Build()` method.
- Do not perform database operations in builders.
- For foreign keys, expose `SetX()` methods that set both the foreign-key ID and navigation property. Do not create `SetXId()` methods.

```csharp
public class BoulderLogBuilder
{
	private readonly BoulderLog _instance = new();

	public BoulderLogBuilder SetFontGrade(FontGrade? value)
	{
		_instance.FontGrade = value;
		return this;
	}

	public BoulderLog Build()
	{
		return _instance;
	}
}
```

### Asserters

Asserters consolidate generic database-population checks and keep them consistent as entities evolve.

- Use asserters for properties populated from database tables, not handler logic or calculated values.
- Create exactly one `[ClassName]Asserter` in `TheCell.Bibaboulder.Sharedtests.csproj` for each class in `Thecell.Bibaboulder.Model.csproj`.
- Keep asserters independent; do not call one asserter from another.
- If a DTO aggregates multiple entities, call the corresponding asserters separately in the test.
- Name the assertion method `Assert` and provide one assertion method per DTO.
- Name parameters `expected` and `actual`, in that order.
- For GET operations, `expected` is the EF-loaded object and `actual` is the API DTO.
- For POST/PUT operations, `expected` is the sent command and `actual` is the EF-loaded object after the operation.

### Test Maintainability

- Keep test methods short and follow DRY by extracting reusable setup.
- Do not create large, cluttered base or helper classes.
- Keep `BaseTest` limited to basic setup; do not add entity-specific helpers such as `GetCurrent[Entity]Version(..)`.
- Split test classes by operation when they become large, for example `SpraywallProblemsControllerCreateTest`, `SpraywallProblemsControllerUpdateTest`, and `SpraywallProblemsControllerDeleteTest`.

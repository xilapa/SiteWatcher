using System.Net;
using FluentAssertions;
using IntegrationTests.Setup;
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Users.Commands.RegisterUser;
using SiteWatcher.Domain.DTOs.User;
using SiteWatcher.Domain.Enums;
using SiteWatcher.IntegrationTests.Utils;
using SiteWatcher.WebAPI.DTOs.ViewModels;

namespace IntegrationTests.UserTests;

public class UserRegisterTests : BaseTest, IClassFixture<BaseTestFixture>
{
    public UserRegisterTests(BaseTestFixture fixture) : base(fixture)
    { }

    [Fact]
    public async Task CantRegisterDuplicateUserEvenWithAValidToken()
    {
        // Arrange
        SetRegisterToken(Users.Xilapa);
        var registerUserCommand = new RegisterUserCommand
        {
            Email = Users.Xilapa.Email,
            Language = Users.Xilapa.Language,
            Name = Users.Xilapa.Name,
            Theme = Users.Xilapa.Theme
        };

        // Act
        var result = await PostAsync("user/register", registerUserCommand);

        result.HttpResponse!.StatusCode
            .Should()
            .Be(HttpStatusCode.Conflict);

        result.GetTyped<WebApiResponse<RegisterUserResult>>()!
            .Messages
            .Should()
            .BeEquivalentTo(new[] {"The User already exists"});

        // Checking on database
        var exception = await Record.ExceptionAsync(async () =>
            await WithDbContext(ctx =>
                ctx.Users.SingleOrDefaultAsync(u => u.GoogleId == Users.Xilapa.GetGoogleId()))
        );

        exception.Should().BeNull();
    }

    [Fact]
    public async Task CallRegisterRouteTwiceDoNotDuplicateUser()
    {
        // Arrange
        var userToRegisterViewModel = new UserViewModel
        {
            Name = "TestUser",
            Email = "testuser@email.com"
        };
        var registerTokenSet = SetRegisterToken(userToRegisterViewModel);
        var registerUserCommand = new RegisterUserCommand
        {
            Email = userToRegisterViewModel.Email,
            Language = ELanguage.Spanish,
            Name = userToRegisterViewModel.Name,
            Theme = ETheme.Light
        };

        // Act
        var firstCall = await PostAsync("user/register", registerUserCommand);
        var secondCall = await PostAsync("user/register", registerUserCommand);

        firstCall.HttpResponse!.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        secondCall.HttpResponse!.StatusCode
            .Should()
            .Be(HttpStatusCode.Forbidden);

        // Checking if register token was invalidated on cache
        FakeCache.Cache[registerTokenSet]
            .Should()
            .Be(TestSettings.InvalidToken);

        // Checking on database
        var userCreated = await WithDbContext(ctx =>
            ctx.Users.Where(u => u.GoogleId == userToRegisterViewModel.GetGoogleId()).ToListAsync());

        userCreated.Count.Should().Be(1);
        userCreated[0]
            .Name
            .Should()
            .Be(userToRegisterViewModel.Name);

        userCreated[0]
            .Email
            .Should()
            .Be(userToRegisterViewModel.Email);
    }
}
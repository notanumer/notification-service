using System.Linq.Expressions;
using DatabaseService.DataAccess.Abstractions;
using DatabaseService.Models.Postgres;
using DatabaseService.Models.Rabbit;
using DatabaseService.Services;
using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;

namespace UnitTests.NotificationService;

[TestFixture]
public class UserService_Tests
{
    private Mock<IAppDbContext> _mockDbContext;
    private UserService _userService;
    private User _testUser;

    [SetUp]
    public void SetUp()
    {
        _mockDbContext = new Mock<IAppDbContext>();
        _userService = new UserService(_mockDbContext.Object);
        _testUser =  new User
        {
            UserName = "testUser",
            Credentials = new Credentials
            {
                Email = "test@example.com",
                TelegramChatId = "12345"
            }
        };
    }

    [Test]
    public async Task Should_getCredentials_returns_email_for_email_channel()
    {
        _mockDbContext.Setup(db => db.Users).ReturnsDbSet(new List<User> { _testUser });
        var result = await _userService.GetCredentials("testUser", ChannelType.Email);

        result.Should().Be(_testUser.Credentials.Email);
    }

    [Test]
    public async Task Should_getCredentials_returns_telegram_id_for_telegram_channel()
    {
        _mockDbContext.Setup(db => db.Users).ReturnsDbSet(new List<User> { _testUser });
        var result = await _userService.GetCredentials("testUser", ChannelType.Telegram);

        result.Should().Be(_testUser.Credentials.TelegramChatId);
    }

    [Test]
    public async Task Should_getCredentials_return_null_when_user_not_found()
    {
        _mockDbContext.Setup(db => db.Users).ReturnsDbSet(new List<User>());

        var result = await _userService.GetCredentials("nonexistentUser", ChannelType.Email);

        result.Should().BeNull();
    }

    [Test]
    public async Task Should_getCredentials_throws_not_implemented_exception_when_channel_type_is_unknown()
    {
        _mockDbContext.Setup(db => db.Users).ReturnsDbSet(new List<User> { _testUser });

        Assert.ThrowsAsync<NotImplementedException>(async () =>
            await _userService.GetCredentials("testUser", (ChannelType)999));
    }

    [Test]
    public async Task PatchCredentials_UpdatesExistingUserCredentials()
    {
        var newCredentials = new Credentials { Email = "new@example.com" };

        _mockDbContext.Setup(db => db.Users).ReturnsDbSet(new List<User> { _testUser });
        await _userService.PatchCredentials("testUser", newCredentials);

        _testUser.Credentials.Email.Should().Be(newCredentials.Email);
        _testUser.Credentials.TelegramChatId.Should().Be(_testUser.Credentials.TelegramChatId);
    }

    [Test]
    public async Task PatchCredentials_DoesNothing_WhenUserNotFound()
    {
        var newCredentials = new Credentials { Email = "new@example.com" };

        _mockDbContext.Setup(db => db.Users).ReturnsDbSet(new List<User>());
        await _userService.PatchCredentials("nonexistentUser", newCredentials);

        _mockDbContext.Verify(db => db.SaveChangesAsync(default), Times.Never);
    }

    [Test]
    public async Task CreateUser_AddsUserToDb()
    {
        var createUserRequest = new CreateUserRequest
        {
            UserName = "newUser",
            Credentials = new Credentials { Email = "new@example.com" }
        };

        var userDbSet = new List<User>();
        _mockDbContext.Setup(db => db.Users).ReturnsDbSet(userDbSet);
        _mockDbContext.Setup(db => db.Users.AddAsync(It.IsAny<User>(), default))
            .Callback<User, CancellationToken>((user, _) => userDbSet.Add(user));

        await _userService.CreateUser(createUserRequest);

        userDbSet.Should().HaveCount(1);
        userDbSet[0].UserName.Should().Be("newUser");
        userDbSet[0].Credentials.Email.Should().Be("new@example.com");
    }
}
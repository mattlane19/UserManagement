using System.Collections.Generic;
using System.Linq;
using UserManagement.Models;
using UserManagement.Services.Domain.Implementations;

namespace UserManagement.Data.Tests;

public class UserServiceTests
{
    [Fact]
    public void GetAll_WhenContextReturnsEntities_MustReturnSameEntities()
    {
        var service = CreateService();
        var users = SetupUsers();

        var result = service.GetAll();

        result.Should().BeSameAs(users);
    }

    [Fact]
    public void GetUserById_WhenUserExists_ShouldReturnUser()
    {
        var service = CreateService();
        var userId = 1;
        var user = new User { Id = userId, Forename = "New", Surname = "User", IsActive = true };

        _dataContext.Setup(context => context.GetUserById<User>(userId)).Returns(new List<User> { user }.AsQueryable());

        var result = service.GetUserById(userId);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(user);
    }

    [Fact]
    public void Add_ShouldCallDataContextCreate()
    {
        var service = CreateService();
        var user = new User { Id = 1, Forename = "New", Surname = "User", IsActive = true };

        service.Add(user);

        _dataContext.Verify(context => context.Create(user), Times.Once);
    }

    [Fact]
    public void Update_ShouldCallDataContextUpdate()
    {
        var service = CreateService();
        var user = new User { Id = 1, Forename = "New", Surname = "User", IsActive = true };

        service.Update(user);

        _dataContext.Verify(context => context.Update(user), Times.Once);
    }

    [Fact]
    public void Delete_ShouldCallDataContextDelete()
    {
        var service = CreateService();
        var user = new User { Id = 1, Forename = "New", Surname = "User", IsActive = true };

        service.Delete(user);

        _dataContext.Verify(context => context.Delete(user), Times.Once);
    }

    private IQueryable<User> SetupUsers(string forename = "New", string surname = "User", string email = "user@example.com", bool isActive = true)
    {
        var users = new[]
        {
            new User
            {
                Forename = forename,
                Surname = surname,
                Email = email,
                IsActive = isActive
            }
        }.AsQueryable();

        _dataContext.Setup(context => context.GetAll<User>()).Returns(users);

        return users;
    }

    private readonly Mock<IDataContext> _dataContext = new();
    private UserService CreateService() => new(_dataContext.Object);
}

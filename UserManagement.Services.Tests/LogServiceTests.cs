using System;
using System.Collections.Generic;
using System.Linq;
using UserManagement.Models;
using UserManagement.Services.Domain.Implementations;

namespace UserManagement.Data.Tests;

public class LogServiceTests
{
    [Fact]
    public void GetAll_WhenContextReturnsEntities_MustReturnSameEntities()
    {
        var service = CreateService();

        var logs = new List<Log>
        {
            new Log {
                Id = 1,
                UserName = "Admin",
                Action = "Edit User",
                Timestamp = new DateTime(),
                Details = "Details"
            }

        }.AsQueryable();

        _dataContext.Setup(context => context.GetAll<Log>()).Returns(logs);

        var result = service.GetAll();

        result.Should().BeSameAs(logs);
    }

    [Fact]
    public void GetUserById_WhenUserExists_ShouldReturnUser()
    {
        var service = CreateService();
        var logId = 1;
        var log = new Log { Id = logId, UserName = "Admin", Action = "Edit User", Timestamp = new DateTime(), Details = "Details" };

        _dataContext.Setup(context => context.GetById<Log>(logId)).Returns(new List<Log> { log }.AsQueryable());

        var result = service.GetById(logId);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(log);
    }

    [Fact]
    public void LogAddAction_ShouldCreateLogForUserAddition()
    {
        var service = CreateService();
        var userName = "Admin";
        var user = new User
        {
            Id = 1,
            Forename = "New",
            Surname = "User",
            DateOfBirth = new DateTime(1990, 1, 1),
            Email = "user@example.com",
            IsActive = true
        };

        service.LogAddAction(userName, user);

        _dataContext.Verify(context => context.Create(It.IsAny<Log>()), Times.Once);
    }

    [Fact]
    public void LogUpdateAction_ShouldCreateLogForUserUpdate()
    {
        var service = CreateService();
        var userName = "Admin";
        var originalUser = new User
        {
            Id = 1,
            Forename = "New",
            Surname = "User",
            DateOfBirth = new DateTime(1990, 1, 1),
            Email = "user@example.com",
            IsActive = true
        };
        var updatedUser = new User
        {
            Id = 1,
            Forename = "Updated",
            Surname = "User",
            DateOfBirth = new DateTime(1990, 1, 1),
            Email = "user@example.com",
            IsActive = true
        };

        service.LogUpdateAction(userName, service.TrackUpdates(originalUser, updatedUser), updatedUser);

        _dataContext.Verify(context => context.Create(It.IsAny<Log>()), Times.Once);
    }

    [Fact]
    public void LogDeleteAction_ShouldCreateLogForUserDeletion()
    {
        var service = CreateService();
        var userName = "Admin";
        var userId = 1;
        var email = "user@example.com";

        service.LogDeleteAction(userName, userId, email);

        _dataContext.Verify(context => context.Create(It.IsAny<Log>()), Times.Once);
    }

    [Fact]
    public void TrackUpdates_ShouldReturnCorrectUpdateMessages()
    {
        var service = CreateService();
        var originalUser = new User
        {
            Id = 1,
            Forename = "New",
            Surname = "User",
            DateOfBirth = new DateTime(1990, 1, 1),
            Email = "user@example.com",
            IsActive = true
        };
        var updatedUser = new User
        {
            Id = 1,
            Forename = "Updated",
            Surname = "User",
            DateOfBirth = new DateTime(1990, 1, 1),
            Email = "updated-user@example.com",
            IsActive = false
        };

        var updates = service.TrackUpdates(originalUser, updatedUser);

        updates.Should().Contain("Forename changed from 'New' to 'Updated'");
        updates.Should().Contain("Email changed from 'user@example.com' to 'updated-user@example.com'");
        updates.Should().Contain("IsActive changed from 'True' to 'False'");
        updates.Should().HaveCount(3);
    }

    private readonly Mock<IDataContext> _dataContext = new();
    private LogService CreateService() => new(_dataContext.Object);
}

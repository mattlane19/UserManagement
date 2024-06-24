using System.Collections.Generic;
using System;
using System.Linq;
using UserManagement.Models;

namespace UserManagement.Data.Tests;

public class DataContextTests
{
    [Fact]
    public void GetAll_WhenNewEntityAdded_MustIncludeNewEntity()
    {
        var context = CreateContext();

        var entity = new User
        {
            Forename = "Brand New",
            Surname = "User",
            Email = "brandnewuser@example.com"
        };
        context.Create(entity);

        var result = context.GetAll<User>();

        result.Should().Contain(result => result.Email == entity.Email)
            .Which.Should().BeEquivalentTo(entity);
    }

    [Fact]
    public void GetAll_WhenDeleted_MustNotIncludeDeletedEntity()
    {
        var context = CreateContext();
        var entity = context.GetAll<User>().First();
        context.Delete(entity);

        var result = context.GetAll<User>();

        result.Should().NotContain(result => result.Email == entity.Email);
    }

    [Fact]
    public void GetById_WhenExistingId_ReturnsCorrectEntity()
    {
        var context = CreateContext();
        var log = context.GetAll<Log>().First();

        var result = context.GetById<Log>(log.Id).SingleOrDefault();

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(log);
    }

    [Fact]
    public void GetUserById_WhenExistingId_ReturnsCorrectUserWithLogs()
    {
        var context = CreateContext();
        var user = context.GetAll<User>().First();

        var logs = new List<Log>
        {
            new Log { UserId = user.Id, Action = "Action1", UserName = "User1", Details = "Details1", Timestamp = DateTime.UtcNow },
        };

        context.Logs?.AddRange(logs);
        context.SaveChanges();

        var result = context.GetUserById<User>(user.Id).SingleOrDefault();

        result.Should().NotBeNull();
        result?.Id.Should().Be(user.Id);
        result?.Logs.Should().NotBeEmpty();
        result?.Logs.Should().BeEquivalentTo(logs);
        result?.Logs.Should().BeEquivalentTo(logs);
    }

    [Fact]
    public void Create_WhenNewUser_EnsuresUserIsAdded()
    {
        var context = CreateContext();
        var user = new User
        {
            Forename = "Test",
            Surname = "User",
            Email = "testuser@example.com"
        };

        context.Create(user);
        var result = context.GetById<User>(user.Id).SingleOrDefault();

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(user);
    }

    [Fact]
    public void Update_WhenExistingUser_EnsuresChangesAreSaved()
    {
        var context = CreateContext();
        var user = context.GetAll<User>().First();
        user.Forename = "Updated";

        context.Update(user);
        var result = context.GetUserById<User>(user.Id).SingleOrDefault();

        result.Should().NotBeNull();
        result?.Forename.Should().Be("Updated");
    }

    [Fact]
    public void Delete_WhenExistingUser_RemovesUserFromContext()
    {
        // Arrange
        var context = CreateContext();
        var user = context.GetAll<User>().First();

        // Act
        context.Delete(user);
        var result = context.GetUserById<User>(user.Id).SingleOrDefault();

        // Assert
        result.Should().BeNull();
    }


    private DataContext CreateContext() => new();
}

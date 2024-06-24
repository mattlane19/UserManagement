using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web.Models.Users;
using UserManagement.WebMS.Controllers;

namespace UserManagement.Data.Tests;

public class UserControllerTests
{
    [Fact]
    public void List_WhenServiceReturnsUsers_ModelMustContainUsers()
    {
        var controller = CreateController();
        var users = new List<User>
    {
        new User { Id = 1, Forename = "New", Surname = "User1", IsActive = true },
        new User { Id = 2, Forename = "New", Surname = "User2", IsActive = true },
        new User { Id = 3, Forename = "New", Surname = "User3", IsActive = false }
    };

        _userService.Setup(userService => userService.GetAll()).Returns(users);

        var result = controller.List(null);

        result.Model.Should().BeOfType<UserListViewModel>()
            .Which.Items.Should().NotBeNullOrEmpty()
            .And.BeEquivalentTo(MapUsersToViewModel(users));
    }

    [Fact]
    public void List_WhenActiveIsTrue_ShouldReturnOnlyActiveUsers()
    {
        var controller = CreateController();

        var users = new List<User>
        {
            new User { Id = 1, Forename = "New", Surname = "User1", IsActive = true },
            new User { Id = 2, Forename = "New", Surname = "User2", IsActive = true },
            new User { Id = 3, Forename = "New", Surname = "User3", IsActive = false }
        };

        var activeUsers = users.Where(user => user.IsActive).ToList();

        _userService.Setup(userService => userService.FilterByActive(true)).Returns(activeUsers);

        var result = controller.List(true) as ViewResult;

        result.Should().NotBeNull();
        var model = result.Model as UserListViewModel;
        model.Should().NotBeNull();
        model?.Items.Should().HaveCount(2);
        model?.Items.All(item => item.IsActive).Should().BeTrue();
        _userService.Verify(userService => userService.FilterByActive(true), Times.Once);
        _userService.Verify(userService => userService.GetAll(), Times.Never);
    }

    [Fact]
    public void List_WhenActiveIsFalse_ShouldReturnOnlyInactiveUsers()
    {
        var controller = CreateController();

        var users = new List<User>
        {
            new User { Id = 1, Forename = "New", Surname = "User1", IsActive = false },
            new User { Id = 2, Forename = "New", Surname = "User2", IsActive = false },
            new User { Id = 3, Forename = "New", Surname = "User3", IsActive = true }
        };

        var inactiveUsers = users.Where(user => !user.IsActive).ToList();

        _userService.Setup(userService => userService.FilterByActive(false)).Returns(inactiveUsers);

        var result = controller.List(false) as ViewResult;

        result.Should().NotBeNull();
        var model = result.Model as UserListViewModel;
        model.Should().NotBeNull();
        model?.Items.Should().HaveCount(2);
        model?.Items.All(item => !item.IsActive).Should().BeTrue();
        _userService.Verify(userService => userService.FilterByActive(false), Times.Once);
        _userService.Verify(userService => userService.GetAll(), Times.Never);
    }

    [Fact]
    public void Details_WhenUserExists_ModelMustContainUserDetails()
    {
        var controller = CreateController();
        var user = SetupUserById(1);

        var result = controller.Details(1) as ViewResult;

        result?.Model.Should().BeOfType<UserDetailsViewModel>()
            .Which.Id.Should().Be(user.Id);
    }

    [Fact]
    public void Details_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        var controller = CreateController();
        _userService.Setup(userService => userService.GetUserById(It.IsAny<long>())).Returns<long>(id => null);

        var result = controller.Details(1);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public void Edit_WhenUserExists_ModelMustContainUserDetails()
    {
        var controller = CreateController();
        var user = SetupUserById(1);

        var result = controller.Edit(1) as ViewResult;

        result?.Model.Should().BeOfType<UserDetailsViewModel>().Which.Id.Should().Be(user.Id);
    }

    [Fact]
    public void Edit_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        var controller = CreateController();
        _userService.Setup(userService => userService.GetUserById(It.IsAny<long>())).Returns<long>(id => null);

        var result = controller.Edit(1);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public void Edit_WhenNoId_ShouldReturnEmptyModel()
    {
        var controller = CreateController();

        var result = controller.Edit(null) as ViewResult;

        result?.Model.Should().BeOfType<UserDetailsViewModel>().Which.Id.Should().Be(0);
    }

    [Fact]
    public void Save_WhenModelIsValid_AddsNewUser()
    {
        var controller = CreateController();
        var model = new UserDetailsViewModel
        {
            Forename = "New",
            Surname = "User",
            DateOfBirth = new DateTime(1990, 1, 1),
            Email = "user@example.com",
            IsActive = true
        };

        var user = MapViewModelToUser(model);

        var result = controller.Save(user) as RedirectToActionResult;

        _userService.Verify(userService => userService.Add(It.Is<User>(u => u.Forename == user.Forename)));
        _logService.Verify(logService => logService.LogAddAction("Admin", It.IsAny<User>()));
        result?.ActionName.Should().Be("List");
    }

    [Fact]
    public void Save_WhenModelIsValid_UpdatesExistingUser()
    {
        var controller = CreateController();
        var existingUser = SetupUserById(1);
        var model = new UserDetailsViewModel
        {
            Id = 1,
            Forename = "Updated",
            Surname = "User",
            DateOfBirth = new DateTime(1990, 1, 1),
            Email = "user@example.com",
            IsActive = true
        };
        var mockUpdates = new List<string>
        {
            "Something was updated"
        };

        _logService.Setup(logService => logService.TrackUpdates(It.IsAny<User>(), It.IsAny<User>())).Returns(mockUpdates);

        var user = MapViewModelToUser(model);

        var result = controller.Save(user) as RedirectToActionResult;

        _userService.Verify(userService => userService.Update(It.Is<User>(u => u.Forename == user.Forename && u.Id == user.Id)));
        _logService.Verify(logService => logService.LogUpdateAction("Unknown User", It.IsAny<List<string>>(), It.IsAny<User>()));
        result?.ActionName.Should().Be("List");
    }

    [Fact]
    public void Save_WhenModelIsInvalid_ReturnsEditView()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("Forename", "Required");
        var model = new UserDetailsViewModel
        {
            Id = 0,
            Forename = string.Empty,
            Surname = string.Empty,
            DateOfBirth = DateTime.MinValue,
            Email = string.Empty,
            IsActive = false
        };

        var user = MapViewModelToUser(model);

        var result = controller.Save(user) as ViewResult;

        result?.ViewName.Should().Be("Edit");
        result?.Model.Should().BeEquivalentTo(model);
    }

    [Fact]
    public void Delete_WhenUserExists_ModelMustContainUserDetails()
    {
        var controller = CreateController();
        var user = SetupUserById(1);

        var result = controller.Delete(1) as ViewResult;

        result?.ViewName.Should().Be("ConfirmDelete");
        result?.Model.Should().BeOfType<UserDetailsViewModel>().Which.Id.Should().Be(user.Id);
    }

    [Fact]
    public void Delete_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        var controller = CreateController();
        _userService.Setup(userService => userService.GetUserById(It.IsAny<long>())).Returns<long>(id => null);

        var result = controller.Delete(1);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public void ConfirmDelete_WhenUserExists_DeletesUserAndRedirectsToList()
    {
        var controller = CreateController();
        var user = SetupUserById(1);
        var model = new UserDetailsViewModel
        {
            Id = 1,
            Forename = "New",
            Surname = "User",
            DateOfBirth = new DateTime(1990, 1, 1),
            Email = "user@example.com",
            IsActive = true
        };

        var result = controller.ConfirmDelete(model.Id) as RedirectToActionResult;

        _userService.Verify(userService => userService.Delete(It.Is<User>(u => u.Id == model.Id)));
        _logService.Verify(logService => logService.LogDeleteAction("Unknown User", model.Id, model.Email));
        result?.ActionName.Should().Be("List");
    }

    [Fact]
    public void ConfirmDelete_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        var controller = CreateController();
        var model = new UserDetailsViewModel
        {
            Id = 0,
            Forename = "New",
            Surname = "User",
            DateOfBirth = new DateTime(1990, 1, 1),
            Email = "user@example.com",
            IsActive = true
        };

        var result = controller.ConfirmDelete(model.Id);

        result.Should().BeOfType<NotFoundResult>();
    }

    private User[] SetupUsers(string forename = "New", string surname = "User", DateTime dateOfBirth = new DateTime(), string email = "user@example.com", bool isActive = true)
    {
        var users = new[]
        {
            new User
            {
                Forename = forename,
                Surname = surname,
                DateOfBirth = dateOfBirth,
                Email = email,
                IsActive = isActive,
            }
        };

        _userService.Setup(userService => userService.GetAll()).Returns(users);

        return users;
    }

    private User SetupUserById(long id)
    {
        var user = new User
        {
            Id = id,
            Forename = "New",
            Surname = "User",
            DateOfBirth = new DateTime(1990, 1, 1),
            Email = "user@example.com",
            IsActive = true,
            Logs = new List<Log>()
        };

        _userService.Setup(userService => userService.GetUserById(id)).Returns(user);
        return user;
    }

    private static User MapViewModelToUser(UserDetailsViewModel model)
    {
        return new User
        {
            Id = model.Id,
            Forename = model.Forename,
            Surname = model.Surname,
            DateOfBirth = model.DateOfBirth.GetValueOrDefault(),
            Email = model.Email,
            IsActive = model.IsActive
        };
    }

    private static List<UserListItemViewModel> MapUsersToViewModel(List<User> users)
    {
        return users.Select(user => new UserListItemViewModel
        {
            Id = user.Id,
            Forename = user.Forename,
            Surname = user.Surname,
            DateOfBirth = new DateTime(),
            Email = user.Email,
            IsActive = user.IsActive
        }).ToList();
    }

    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<ILogService> _logService = new();
    private UsersController CreateController() => new(_userService.Object, _logService.Object);
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web.Models.Logs;
using UserManagement.WebMS.Controllers;

namespace UserManagement.Data.Tests;

public class LogControllerTests
{
    [Fact]
    public void List_WhenServiceReturnsLogs_ModelMustContainLogs()
    {
        var controller = CreateController();
        var logs = new List<Log>
        {
            new Log
            {
                Id = 1,
                UserId = 1,
                Action = "Add User",
                UserName = "Admin",
                Timestamp = new DateTime(),
                Details = "User added - , Id: 1, Forename: New, Surname: User, DateOfBirth: 1990-01-01, Email: user@example.com, IsActive: true"
            }
        };

        var queryableLogs = logs.AsQueryable();

        _logService.Setup(logService => logService.GetAll()).Returns(queryableLogs);

        var expectedViewModels = logs.Select(MapToViewModel).ToList();

        var result = controller.List(1);

        result.Model.Should().BeOfType<LogListViewModel>().Which.Items.Should().BeEquivalentTo(expectedViewModels);
    }

    [Fact]
    public void List_WhenNoLogs_ModelMustContainEmptyLogs()
    {
        var controller = CreateController();
        _logService.Setup(logService => logService.GetAll()).Returns(Enumerable.Empty<Log>().AsQueryable());

        var result = controller.List(1) as ViewResult;

        result.Model.Should().BeOfType<LogListViewModel>().Which.Items.Should().BeEmpty();
    }

    [Fact]
    public void Details_WhenLogExists_ModelMustContainLogDetails()
    {
        var controller = CreateController();
        var log = new Log
        {
            Id = 1,
            UserId = 1,
            Action = "Add User",
            UserName = "Admin",
            Timestamp = DateTime.UtcNow,
            Details = "User added - , Id: 1, Forename: New, Surname: User, DateOfBirth: 1990-01-01, Email: user@example.com, IsActive: true"
        };

        _logService.Setup(logService => logService.GetById(log.Id)).Returns(log);

        var result = controller.Details(1) as ViewResult;

        result?.Model.Should().BeOfType<LogDetailsViewModel>().Which.Id.Should().Be(log.Id);
    }

    [Fact]
    public void Details_WhenLogDoesNotExist_ShouldReturnNotFound()
    {
        var controller = CreateController();
        _logService.Setup(logService => logService.GetById(It.IsAny<long>())).Returns<long>(id => null);

        var result = controller.Details(1);

        result.Should().BeOfType<NotFoundResult>();
    }

    private LogListItemViewModel MapToViewModel(Log log)
    {
        var model = new LogListItemViewModel
        {
            Id = log.Id,
            Action = log.Action,
            UserName = log.UserName,
            Timestamp = log.Timestamp
        };

        return model;
    }

    private readonly Mock<ILogService> _logService = new();
    private LogsController CreateController() => new(_logService.Object);
}

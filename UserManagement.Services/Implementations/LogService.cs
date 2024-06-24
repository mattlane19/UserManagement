using System.Collections.Generic;
using System;
using System.Linq;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;

namespace UserManagement.Services.Domain.Implementations;

public class LogService : ILogService
{
    private readonly IDataContext _dataAccess;

    public LogService(IDataContext dataAccess) => _dataAccess = dataAccess;

    public IQueryable<Log> GetAll() => _dataAccess.GetAll<Log>().AsQueryable();

    public Log? GetById(long id)
    {
        var log = _dataAccess.GetById<Log>(id).SingleOrDefault();
        return log ?? null;
    }

    public void LogAddAction(string userName, User user)
    {
        _dataAccess.Create(new Log
        {
            UserId = user.Id,
            Action = "Add User",
            UserName = userName,
            Details = $"User added - , Id: {user.Id}, Forename: {user.Forename}, Surname: {user.Surname}, " +
            $" DateOfBirth: {user.DateOfBirth:dd-MM-yyyy}, Email: {user.Email}, IsActive: {user.IsActive}",
            Timestamp = DateTime.Now
        });
    }

    public void LogUpdateAction(string userName, List<string> updates, User user)
    {
        _dataAccess.Create(new Log
        {
            UserId= user.Id,
            Action = "Edit User",
            UserName = userName,
            Details = $"User Id: {user.Id} updated - , {string.Join(", ", updates)}",
            Timestamp = DateTime.Now
        });
    }

    public void LogDeleteAction(string userName, long userId, string email)
    {
        _dataAccess.Create(new Log
        {
            UserId = userId,
            Action = "Delete User",
            UserName = userName,
            Details = $"User deleted - , Id: {userId}, Email: {email}",
            Timestamp = DateTime.Now
        });
    }

    public List<string> TrackUpdates(User originalUser, User updatedUser)
    {
        var updates = new List<string>();

        if (originalUser.Forename != updatedUser.Forename)
        {
            updates.Add($"Forename changed from '{originalUser.Forename}' to '{updatedUser.Forename}'");
            originalUser.Forename = updatedUser.Forename;
        }

        if (originalUser.Surname != updatedUser.Surname)
        {
            updates.Add($"Surname changed from '{originalUser.Surname}' to '{updatedUser.Surname}'");
            originalUser.Surname = updatedUser.Surname;
        }

        if (originalUser.DateOfBirth != updatedUser.DateOfBirth)
        {
            updates.Add($"DateOfBirth changed from '{originalUser.DateOfBirth:dd-MM-yyyy}' to '{updatedUser.DateOfBirth:dd-MM-yyyy}'");
            originalUser.DateOfBirth = updatedUser.DateOfBirth;
        }

        if (originalUser.Email != updatedUser.Email)
        {
            updates.Add($"Email changed from '{originalUser.Email}' to '{updatedUser.Email}'");
            originalUser.Email = updatedUser.Email;
        }

        if (originalUser.IsActive != updatedUser.IsActive)
        {
            updates.Add($"IsActive changed from '{originalUser.IsActive}' to '{updatedUser.IsActive}'");
            originalUser.IsActive = updatedUser.IsActive;
        }

        return updates;
    }

}

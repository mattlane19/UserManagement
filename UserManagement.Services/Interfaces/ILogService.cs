using System.Collections.Generic;
using System.Linq;
using UserManagement.Models;

namespace UserManagement.Services.Domain.Interfaces;

public interface ILogService
{
    IQueryable<Log> GetAll();
    Log? GetById(long id);
    void LogAddAction(string userName, User user);
    void LogUpdateAction(string userName, List<string> changes, User user);
    void LogDeleteAction(string userName, long userId, string email);
    List<string> TrackUpdates(User originalUser, User updatedUser);

}

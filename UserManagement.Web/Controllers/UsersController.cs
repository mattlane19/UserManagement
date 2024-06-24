using System.Linq;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web.Models.Logs;
using UserManagement.Web.Models.Users;

namespace UserManagement.WebMS.Controllers;

[Route("users")]
public class UsersController : Controller
{
    private readonly IUserService _userService;
    private readonly ILogService _logService;

    public UsersController(IUserService userService, ILogService logService)
    {
        _userService = userService;
        _logService = logService;
    }

    [HttpGet]
    public ViewResult List(bool? active)
    {
        var users = active.HasValue ? _userService.FilterByActive(active.Value) : _userService.GetAll();

        var model = new UserListViewModel
        {
            Items = users.Select(user => new UserListItemViewModel
            {
                Id = user.Id,
                Forename = user.Forename,
                Surname = user.Surname,
                DateOfBirth = user.DateOfBirth,
                Email = user.Email,
                IsActive = user.IsActive
            }).ToList()
        };

        return View(model);
    }

    [HttpGet("users/details/{id}")]
    public IActionResult Details(long id)
    {
        var user = _userService.GetUserById(id);

        if (user == null)
        {
            return NotFound();
        }

        var model = new UserDetailsViewModel
        {
            Id = user.Id,
            Forename = user.Forename,
            Surname = user.Surname,
            DateOfBirth = user.DateOfBirth,
            Email = user.Email,
            IsActive = user.IsActive,
            Logs = user.Logs
        };

        if (model.Logs != null)
        {
            foreach (var log in model.Logs)
            {
                var logViewModel = new LogListItemViewModel
                {
                    Id = log.Id,
                    UserName = log.UserName,
                    Action = log.Action,
                    Timestamp = log.Timestamp,
                };

                logViewModel.SetDetails(log.Details);
                model.LogDetailsList?.Add(logViewModel);
            }
        }

        return View(model);
    }

    [HttpGet("users/edit/")]
    public IActionResult Edit(long? id)
    {
        UserDetailsViewModel model;

        if (id.HasValue)
        {
            var user = _userService.GetUserById(id.Value);

            if (user == null)
            {
                return NotFound();
            }

            model = new UserDetailsViewModel
            {
                Id = user.Id,
                Forename = user.Forename,
                Surname = user.Surname,
                DateOfBirth = user.DateOfBirth,
                Email = user.Email,
                IsActive = user.IsActive
            };
        }
        else
        {
            model = new UserDetailsViewModel
            {
                Forename = string.Empty,
                Surname = string.Empty,
                Email = string.Empty,
                DateOfBirth = null,
                IsActive = false
            };
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Save(User user)
    {
        if (!ModelState.IsValid)
        {
            return View("Edit", new UserDetailsViewModel
            {
                Id = user.Id,
                Forename = user.Forename,
                Surname = user.Surname,
                DateOfBirth = user.DateOfBirth,
                Email = user.Email,
                IsActive = user.IsActive
            });
        }

        if (user.Id == 0)
        {
            var newUser = new User
            {
                Forename = user.Forename,
                Surname = user.Surname,
                DateOfBirth = user.DateOfBirth,
                Email = user.Email,
                IsActive = user.IsActive
            };

            _userService.Add(newUser);
            _logService.LogAddAction("Admin", newUser);
        }
        else
        {
            var existingUser = _userService.GetUserById(user.Id);

            if (existingUser == null)
            {
                return NotFound();
            }

            var changes = _logService.TrackUpdates(existingUser, user);

            if (changes != null)
            {
                _logService.LogUpdateAction("Admin", changes, existingUser);
            }

            existingUser.Forename = user.Forename;
            existingUser.Surname = user.Surname;
            existingUser.DateOfBirth = user.DateOfBirth;
            existingUser.Email = user.Email;
            existingUser.IsActive = user.IsActive;

            _userService.Update(existingUser);
        }

        return RedirectToAction("List");
    }

[HttpGet("users/delete/{id}")]
    public IActionResult Delete(long id)
    {
        var user = _userService.GetUserById(id);

        if (user == null)
        {
            return NotFound();
        }

        var model = new UserDetailsViewModel
        {
            Id = user.Id,
            Forename = user.Forename,
            Surname = user.Surname,
            DateOfBirth = user.DateOfBirth,
            Email = user.Email,
            IsActive = user.IsActive
        };

        return View("ConfirmDelete", model);
    }

    [HttpPost("users/delete/{id}")]
    [ValidateAntiForgeryToken]
    public IActionResult ConfirmDelete(UserDetailsViewModel model)
    {
        if (model == null || model.Id <= 0)
        {
            return NotFound();
        }

        _logService.LogDeleteAction("Admin", model.Id, model.Email);

        _userService.Delete(new User { Id = model.Id });

        return RedirectToAction("List");
    }
}

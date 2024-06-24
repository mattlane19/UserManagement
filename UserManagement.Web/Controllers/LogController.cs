using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web.Models.Logs;

namespace UserManagement.WebMS.Controllers;

[Authorize]
[Route("logs")]
public class LogsController(ILogService logService) : Controller
{
    private readonly ILogService _logService = logService;

    [HttpGet]
    public ViewResult List(int page = 1)
    {
        var pageSize = 10;
        var logs = _logService.GetAll().OrderByDescending(x => x.Timestamp);
        var pagedLogs = logs.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var items = pagedLogs.Select(log => new LogListItemViewModel
        {
            Id = log.Id,
            Action = log.Action,
            UserName = log.UserName,
            Timestamp = log.Timestamp
        });

        var model = new LogListViewModel
        {
            Items = items.ToList(),
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(logs.Count() / (double)pageSize)
        };

        return View(model);
    }

    [HttpGet("/logs/details/{id}")]
    public IActionResult Details(long id)
    {
        var log = _logService.GetById(id);

        if (log == null)
        {
            return NotFound();
        }

        var model = new LogDetailsViewModel
        {
            Id = log.Id,
            Action = log.Action,
            UserName = log.UserName,
            Details = log.Details,
            Timestamp = log.Timestamp
        };

        model.SetDetails(model.Details);

        return View(model);
    }
}

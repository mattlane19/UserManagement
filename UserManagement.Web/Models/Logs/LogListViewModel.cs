using System;
using System.Linq;

namespace UserManagement.Web.Models.Logs;

public class LogListViewModel
{
    public List<LogListItemViewModel> Items { get; set; } = new List<LogListItemViewModel>();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}

public class LogListItemViewModel
{
    public long Id { get; set; }
    public string? Action { get; set; }
    public string? UserName { get; set; }
    public DateTime? Timestamp { get; set; }

    public List<string> DetailsList { get; set; }

    public LogListItemViewModel()
    {
        DetailsList = new List<string>();
    }

    public void SetDetails(string details)
    {
        DetailsList = details.Split(',').ToList();
    }
}

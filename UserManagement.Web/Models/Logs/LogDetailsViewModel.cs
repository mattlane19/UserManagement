using System.ComponentModel.DataAnnotations;
using System;
using System.Linq;

namespace UserManagement.Web.Models.Logs;

public class LogDetailsViewModel
{
    [Required]
    public long Id { get; set; }

    [Required]
    [StringLength(100)]
    public string? Action { get; set; }

    [Required]
    [StringLength(100)]
    public string? UserName { get; set; }

    [Required]
    [StringLength(100)]
    public string? Details { get; set; }

    [Required]
    public DateTime? Timestamp { get; set; }

    public List<string> DetailsList { get; set; }


    public LogDetailsViewModel()
    {
        DetailsList = new List<string>();
    }
    public void SetDetails(string details)
    {
        DetailsList = details.Split(',').ToList();
    }

}

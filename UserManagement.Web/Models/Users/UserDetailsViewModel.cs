using System.ComponentModel.DataAnnotations;
using System;
using UserManagement.Models;
using UserManagement.Web.Models.Logs;

namespace UserManagement.Web.Models.Users;

public class UserDetailsViewModel
{
    [Required]
    public long Id { get; set; }

    [Required]
    [StringLength(100)]
    public required string Forename { get; set; }

    [Required]
    [StringLength(100)]
    public required string Surname { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public required DateTime? DateOfBirth { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required bool IsActive { get; set; }

    public ICollection<Log>? Logs { get; set; } = null;

    public List<LogListItemViewModel>? LogDetailsList { get; set; }


    public UserDetailsViewModel()
    {
        LogDetailsList = new List<LogListItemViewModel>();
    }
}

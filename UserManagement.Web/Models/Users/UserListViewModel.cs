using System;
using System.ComponentModel.DataAnnotations;
using UserManagement.Models;

namespace UserManagement.Web.Models.Users;

public class UserListViewModel
{
    public List<UserListItemViewModel> Items { get; set; } = new List<UserListItemViewModel>();
}

public class UserListItemViewModel
{
    public long Id { get; set; }
    public string? Forename { get; set; }
    public string? Surname { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }

    public ICollection<Log>? Logs { get; set; }
}

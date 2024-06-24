using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace UserManagement.Models;

public class User : IdentityUser<long>
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Forename { get; set; } = default!;
    public string Surname { get; set; } = default!;
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; } = default!;
    public new string Email { get; set; } = default!;
    public bool IsActive { get; set; }


    public ICollection<Log>? Logs { get; set; }
}

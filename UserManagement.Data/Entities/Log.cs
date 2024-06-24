using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagement.Models;

public class Log
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long? UserId { get; set; }

    public User? User { get; set; } = default!;
    public string Action { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Details { get; set; } = default!;
    public DateTime Timestamp { get; set; } = default!;
}

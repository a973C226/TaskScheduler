using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace TaskScheduler.Models
{
    [Index(nameof(Username), IsUnique = true)]
    public class User
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public List<Assignment> Assignments { get; set; } = new();
    }
}

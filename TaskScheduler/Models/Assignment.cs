using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace TaskScheduler.Models
{
    public class Assignment
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Date { get; set; }
        public string? Deadline { get; set; }
        public bool IsMuted { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}

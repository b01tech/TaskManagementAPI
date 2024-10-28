using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.Data;
public class TaskContext : DbContext
{
    public TaskContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<TaskItem> Tasks { get; set; }
}

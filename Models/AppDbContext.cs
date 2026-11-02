using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<Cleaner> Cleaners { get; set; }
    public DbSet<Desk> Desks { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Department> Departments { get; set; }
}
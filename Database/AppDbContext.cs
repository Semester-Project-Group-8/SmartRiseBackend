using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Admin> Admins => Set<Admin>(); //lets efcore query the db 
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Desk> Desks => Set<Desk>();
        public DbSet<WorksIn> WorksIns => Set<WorksIn>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)//tells efcore what our models map to into the db
        {
            //needed to keep to postgresql and csharp naming conventions
            //if we would use same naming convention both in db and backend we dont need(like Models/Admin.cs class should be called admin like table or other way around)
            modelBuilder.Entity<Admin>(entity =>
            {
                //mapping table
                entity.ToTable("admin");
                //mapping collumns
                entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash");

                //relationships => admin has many departments for one admin
                entity.HasMany(e => e.Departments)
                      .WithOne(e => e.Admin)
                      .HasForeignKey(e => e.AdminId);
            });
            modelBuilder.Entity<Department>(entity =>
            {
                entity.ToTable("department");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Major).HasColumnName("major");
                entity.Property(e => e.StartTime).HasColumnName("start_time");
                entity.Property(e => e.EndTime).HasColumnName("end_time");

                entity.HasMany(e => e.Desks)
                      .WithOne(e => e.Department)
                      .HasForeignKey(e => e.DepartmentId);
            });
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("employee");
                entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
                entity.Property(e => e.AccountStatus).HasColumnName("account_status");
                entity.Property(e => e.Role).HasColumnName("role");
            });
            modelBuilder.Entity<Desk>(entity =>
            {
                entity.ToTable("desk");
                entity.Property(e => e.Minor).HasColumnName("minor");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.PreferredHeight).HasColumnName("preferred_height");
                entity.Property(e => e.CompanyHeight).HasColumnName("company_height");
                entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
                entity.Property(e => e.DepartmentId).HasColumnName("department_id");

                entity.HasOne(e => e.Employee)
                      .WithOne(e => e.Desk)
                      .HasForeignKey<Desk>(e => e.EmployeeId);
            });
            modelBuilder.Entity<WorksIn>(entity =>
            {

                entity.ToTable("works_in");
                entity.Property(e => e.StartTime).HasColumnName("start_time");
                entity.Property(e => e.EndTime).HasColumnName("end_time");
                entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
                entity.Property(e => e.DepartmentId).HasColumnName("department_id");
                
                //this tells efcore that that primarykey we create in sql for WorksIn(department+employee) is like a combination of these two
                entity.HasKey(e => new
                {
                    e.EmployeeId,
                    e.DepartmentId
                });
                entity.HasOne(e => e.Cleaner)
                      .WithMany(e => e.WorksIn)
                      .HasForeignKey(e => e.EmployeeId);

                entity.HasOne(e => e.Department)
                      .WithMany(e => e.WorksIn)
                      .HasForeignKey(e => e.DepartmentId);
            });

        }
    }
}

﻿using BaseLibrary.Entities;
using Microsoft.EntityFrameworkCore;


namespace ServerLibrary.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<GeneralDepartment> GeneralDepartment { get; set; }
        public DbSet<Department> departments { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Town> Towns { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public DbSet<SystemRole> systemRoles { get; set; }
        public DbSet<UserRole> userRoles { get; set; }



    }
}

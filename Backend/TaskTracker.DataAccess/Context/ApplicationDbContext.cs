using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskTracker.Model.Entities;

namespace TaskTracker.DataAccess.Context
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; } 
        public DbSet<TaskItem> TaskItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();
                entity.HasIndex(p => p.Name).IsUnique();
                entity.Property(p => p.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("(SYSUTCDATETIME())");
                entity.Property(p => p.RowVersion).IsRowVersion();
            });

            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();
                entity.Property(p => p.Status).HasConversion<int>();
                entity.Property(p => p.Priority).HasConversion<int>();
                entity.Property(p => p.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("(SYSUTCDATETIME())");
                entity.Property(p => p.RowVersion).IsRowVersion();
                entity.HasOne(p => p.Project).WithMany(p => p.TaskItems).HasForeignKey(p => p.ProjectId).IsRequired().OnDelete(DeleteBehavior.Cascade);  
            });
        }

    }
}

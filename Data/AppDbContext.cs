using Microsoft.EntityFrameworkCore;
using ParcelTrackingSystem.Controllers;
using ParcelTrackingSystem.Models;

namespace ParcelTrackingSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Agent> Agents { get; set; }
        public DbSet<ParcelCreation> ParcelCreations { get; set; }
        public DbSet<Supervisor> Supervisors { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
       



    }
}

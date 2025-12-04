using Microsoft.EntityFrameworkCore;
using SortyWebApp.Models;

namespace SortyWebApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Start-Daten (Seeding), damit nicht alles leer ist beim ersten Start
            modelBuilder.Entity<Warehouse>().HasData(
                new Warehouse { Id = 1, Name = "Lager 1", ColorHex = "#007AFF" }, // Apple Blau
                new Warehouse { Id = 2, Name = "Lager 2", ColorHex = "#FF3B30" }, // Apple Rot
                new Warehouse { Id = 3, Name = "Lager 3", ColorHex = "#34C759" }  // Apple Grün
            );
        }
    }
}
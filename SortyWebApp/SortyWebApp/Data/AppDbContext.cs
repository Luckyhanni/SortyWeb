using Microsoft.EntityFrameworkCore;
using SortyWebApp.Models;
using System.Collections.Generic;

namespace SortyWebApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
    }
}
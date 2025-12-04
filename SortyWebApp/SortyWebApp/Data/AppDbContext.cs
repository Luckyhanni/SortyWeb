using Microsoft.EntityFrameworkCore;
using SortyWeb.Models;
using System.Collections.Generic;

namespace SortyWeb.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
    }
}
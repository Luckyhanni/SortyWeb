using FuzzySharp;
using Microsoft.EntityFrameworkCore;
using SortyWebApp.Data;
using SortyWebApp.Models;
using System.Text.RegularExpressions;

namespace SortyWebApp.Services
{
    public class OrderService
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        public OrderService(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        // --- ORDER LOGIC ---
        public async Task AddOrderAsync(Order order)
        {
            using var context = _dbContextFactory.CreateDbContext();
            order.Id = 0;
            context.Orders.Add(order);
            await context.SaveChangesAsync();
        }

        public async Task UpdateOrderAsync(Order order)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.Orders.Update(order);
            await context.SaveChangesAsync();
        }

        public async Task<List<Order>> GetActiveOrdersAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Orders
                .Where(o => !o.IsPickedUp)
                .OrderBy(o => o.Name)
                .ToListAsync();
        }

        public async Task<List<Order>> GetRecentOrdersAsync(int count = 5)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Orders
                .Where(o => !o.IsPickedUp)
                .OrderByDescending(o => o.Id)
                .Take(count)
                .ToListAsync();
        }

        public async Task PickupOrderAsync(int id)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var order = await context.Orders.FindAsync(id);
            if (order != null)
            {
                order.IsPickedUp = true;
                order.PickedUpAt = DateTime.Now;
                await context.SaveChangesAsync();
            }
        }

        public async Task ClearAllOrdersAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();
            await context.Orders.ExecuteDeleteAsync();
        }

        // --- UPDATE: Suche mit Parameter für "Alles durchsuchen" ---
        public async Task<List<(Order Order, int Score)>> SearchOrdersAsync(string inputName, bool includePickedUp = false)
        {
            using var context = _dbContextFactory.CreateDbContext();

            // Basis-Query: Entweder nur aktive oder alle
            var query = context.Orders.AsQueryable();
            if (!includePickedUp)
            {
                query = query.Where(o => !o.IsPickedUp);
            }

            var allOrders = await query.ToListAsync();

            string inputSoundex = GenerateSoundex(inputName);

            var results = allOrders.Select(order =>
            {
                bool containsInput = order.Name.IndexOf(inputName, StringComparison.OrdinalIgnoreCase) >= 0;
                int levenshteinSimilarity = Fuzz.Ratio(inputName, order.Name);
                bool isPhoneticMatch = GenerateSoundex(order.Name) == inputSoundex;

                int priority = containsInput ? 100 : (isPhoneticMatch ? 10 : 0);
                int totalScore = priority + levenshteinSimilarity;

                return (Order: order, Score: Math.Min(totalScore, 100));
            })
            .Where(x => x.Score >= 35)
            .OrderByDescending(x => x.Score)
            .ToList();

            return results;
        }

        // --- WAREHOUSE LOGIC ---
        public async Task<List<Warehouse>> GetWarehousesAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Warehouses.ToListAsync();
        }

        public async Task AddWarehouseAsync(string name, string color)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.Warehouses.Add(new Warehouse { Name = name, ColorHex = color });
            await context.SaveChangesAsync();
        }

        public async Task DeleteWarehouseAsync(int id)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var wh = await context.Warehouses.FindAsync(id);
            if (wh != null)
            {
                context.Warehouses.Remove(wh);
                await context.SaveChangesAsync();
            }
        }

        public async Task<Dictionary<string, int>> GetFullStatsAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();
            var warehouses = await context.Warehouses.ToListAsync();
            var activeOrders = await context.Orders.Where(o => !o.IsPickedUp).ToListAsync();
            var stats = new Dictionary<string, int>();

            foreach (var wh in warehouses)
            {
                int count = activeOrders.Count(o => o.Ort == wh.Name);
                stats[wh.Name] = count;
            }
            return stats;
        }

        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();
            var openCount = await context.Orders.CountAsync(o => !o.IsPickedUp);
            var pickedUpCount = await context.Orders.CountAsync(o => o.IsPickedUp);
            var lastPickup = await context.Orders
                .Where(o => o.IsPickedUp)
                .OrderByDescending(o => o.PickedUpAt)
                .Select(o => o.PickedUpAt)
                .FirstOrDefaultAsync();

            return new DashboardStats(openCount, pickedUpCount, lastPickup);
        }

        private static string GenerateSoundex(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            char firstLetter = char.ToUpper(input[0]);
            string tail = input.Substring(1).ToUpper();
            tail = Regex.Replace(tail, "[BFPV]", "1");
            tail = Regex.Replace(tail, "[CGJKQSXZ]", "2");
            tail = Regex.Replace(tail, "[DT]", "3");
            tail = Regex.Replace(tail, "[L]", "4");
            tail = Regex.Replace(tail, "[MN]", "5");
            tail = Regex.Replace(tail, "[R]", "6");
            tail = Regex.Replace(tail, "[AEIOUHWY]", "");
            tail = Regex.Replace(tail, @"(\d)\1+", "$1");
            string soundex = firstLetter + tail;
            return soundex.PadRight(4, '0').Substring(0, 4);
        }
    }

    public record DashboardStats(int OpenOrders, int PickedUpOrders, DateTime? LastPickupTime);
}
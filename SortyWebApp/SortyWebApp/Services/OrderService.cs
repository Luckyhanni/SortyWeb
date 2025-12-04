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

        public async Task AddOrderAsync(Order order)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.Orders.Add(order);
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

        public async Task<Dictionary<string, int>> GetStatsAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();
            var stats = await context.Orders
                .Where(o => !o.IsPickedUp)
                .GroupBy(o => o.Ort)
                .Select(g => new { Ort = g.Key, Count = g.Count() })
                .ToDictionaryAsync(k => k.Ort, v => v.Count);

            return stats;
        }

        public async Task<List<(Order Order, int Score)>> SearchOrdersAsync(string inputName)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var allOrders = await context.Orders.Where(o => !o.IsPickedUp).ToListAsync();
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
}
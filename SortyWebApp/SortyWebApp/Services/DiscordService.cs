using RestSharp;
using Microsoft.Extensions.Configuration;

namespace SortyWebApp.Services
{
    public class DiscordService
    {
        private readonly IConfiguration _configuration;
        private string _botToken;
        private string _channelId;

        public DiscordService(IConfiguration configuration)
        {
            _configuration = configuration;
            _botToken = _configuration["Discord:Token"] ?? "";
            _channelId = _configuration["Discord:ChannelId"] ?? "";
        }

        // Signatur geändert: double price -> DateTime pickupDate
        public async Task SendPickupNotificationAsync(string name, string vorname, int chestNumber, string location, DateTime pickupDate)
        {
            if (string.IsNullOrEmpty(_botToken) || string.IsNullOrEmpty(_channelId))
            {
                // Silent fail oder Logging
                return;
            }

            var client = new RestClient($"https://discord.com/api/v10/channels/{_channelId}/messages");
            var request = new RestRequest { Method = Method.Post };

            request.AddHeader("Authorization", $"Bot {_botToken}");
            request.AddHeader("Content-Type", "application/json");

            // Nachricht angepasst: Datum statt Preis
            string message = $"📦 **Abholung erfolgt** ({DateTime.Now:HH:mm})\n" +
                             $"👤 **Name:** {name}, {vorname}\n" +
                             $"📅 **Termin:** {pickupDate:dd.MM.}\n" +
                             $"🏠 **Lager:** {location}\n" +
                             $"🔢 **Kiste:** {chestNumber}";

            var payload = new { content = message };
            request.AddJsonBody(payload);

            try
            {
                await client.ExecuteAsync(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Discord Exception: {ex.Message}");
            }
        }
    }
}
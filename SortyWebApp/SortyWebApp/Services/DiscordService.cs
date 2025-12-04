using RestSharp;
using Microsoft.Extensions.Configuration;

namespace SortyWeb.Services
{
    public class DiscordService
    {
        private readonly IConfiguration _configuration;
        private string _botToken;
        private string _channelId;

        public DiscordService(IConfiguration configuration)
        {
            _configuration = configuration;
            // Lädt die Daten aus der appsettings.json
            _botToken = _configuration["Discord:Token"] ?? "";
            _channelId = _configuration["Discord:ChannelId"] ?? "";
        }

        public async Task SendPickupNotificationAsync(string name, string vorname, int chestNumber, string location, double price)
        {
            if (string.IsNullOrEmpty(_botToken) || string.IsNullOrEmpty(_channelId))
            {
                Console.WriteLine("ABBRUCH: Discord Token oder ChannelID fehlen in der Config.");
                return;
            }

            var client = new RestClient($"https://discord.com/api/v10/channels/{_channelId}/messages");
            var request = new RestRequest { Method = Method.Post };

            request.AddHeader("Authorization", $"Bot {_botToken}");
            request.AddHeader("Content-Type", "application/json");

            string message = $"📦 **Neue Abholung** ({DateTime.Now:dd.MM.yyyy HH:mm})\n" +
                             $"👤 **Name:** {name}, {vorname}\n" +
                             $"🏠 **Lager:** {location}\n" +
                             $"🔢 **Kiste:** {chestNumber}\n" +
                             $"💰 **Preis:** {price:C}";

            var payload = new { content = message };
            request.AddJsonBody(payload);

            try
            {
                var response = await client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    Console.WriteLine("Discord Nachricht gesendet.");
                }
                else
                {
                    Console.WriteLine($"Discord Fehler: {response.StatusCode} - {response.Content}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Discord Exception: {ex.Message}");
            }
        }
    }
}
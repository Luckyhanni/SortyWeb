using System.ComponentModel.DataAnnotations;

namespace SortyWebApp.Models
{
    public class Warehouse
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string ColorHex { get; set; } = "#6c757d"; // Standard Grau
    }
}
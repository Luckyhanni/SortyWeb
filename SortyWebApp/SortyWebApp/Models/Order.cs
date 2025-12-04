using System.ComponentModel.DataAnnotations;

namespace SortyWeb.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name ist erforderlich")]
        public string Name { get; set; } = string.Empty;

        public string Vorname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ort ist erforderlich")]
        public string Ort { get; set; } = string.Empty; // Lager 1, Lager 2 etc.

        [Required(ErrorMessage = "Kistennummer ist erforderlich")]
        public int ChestNumber { get; set; }

        public double Price { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Wir löschen nicht hart, sondern markieren als abgeholt (für Statistik)
        public bool IsPickedUp { get; set; } = false;
        public DateTime? PickedUpAt { get; set; }
    }
}
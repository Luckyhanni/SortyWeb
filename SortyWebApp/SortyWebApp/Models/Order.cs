using System.ComponentModel.DataAnnotations;

namespace SortyWebApp.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name ist erforderlich")]
        public string Name { get; set; } = string.Empty;

        public string Vorname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ort ist erforderlich")]
        public string Ort { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kistennummer ist erforderlich")]
        public int ChestNumber { get; set; }

        // NEU: Datum statt Preis. Standardmäßig der 23.12. des aktuellen Jahres.
        [Required]
        public DateTime PickupDate { get; set; } = new DateTime(DateTime.Now.Year, 12, 23);

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsPickedUp { get; set; } = false;
        public DateTime? PickedUpAt { get; set; }
    }
}
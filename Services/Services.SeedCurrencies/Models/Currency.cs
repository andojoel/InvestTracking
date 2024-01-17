using System.ComponentModel.DataAnnotations;

namespace Services.SeedCurrencies.Models
{
    public class Currency
    {
        [Key]
        public required string id { get; set; }
        [Required]
        public string? name { get; set; }
        [Required]
        public string? symbol { get; set; }
        public decimal? current_price { get; set; }
        public decimal? market_cap { get; set; }
        public int? market_cap_rank { get; set; }
        public decimal? price_change_24h { get; set; }
        public double? price_change_percentage_24h { get; set; }
        public string? image { get; set; }

    }

}

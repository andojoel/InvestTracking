namespace Services.CurrencyAPI.Models.Dto
{
    public class CurrencyDto
    {
        public required string id { get; set; }
        public string? name { get; set; }
        public string? symbol { get; set; }
        public decimal? current_price { get; set; }
        public decimal? market_cap { get; set; }
        public int? market_cap_rank { get; set; }
        public decimal? price_change_24h { get; set; }
        public double? price_change_percentage_24h { get; set; }
        public string? image { get; set; }
    }
}

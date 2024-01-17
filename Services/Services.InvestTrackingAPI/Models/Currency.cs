using System.ComponentModel.DataAnnotations;

namespace Services.InvestTrackingAPI.Models;
public class Currency
{
    [Key]
    public required string id { get; set; }
    [Required]
    public string? name { get; set; } = "";
    [Required]
    public string? symbol { get; set; } = "";
    public decimal? current_price { get; set; } = 0;
    public decimal? market_cap { get; set; } = 0;
    public int? market_cap_rank { get; set; } = 0;
    public decimal? price_change_24h { get; set; }
    public double? price_change_percentage_24h { get; set; }
    public string? image { get; set; } = "";
    public decimal? quantity {get; set; } = 0;
    public decimal? total_value {get; set; } = 0;
    public string? ownerUserName {get; set;}
    public Guid? wallet_id {get; set;}
}
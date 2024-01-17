namespace Services.InvestTrackingAPI.Models.Dto;
public class RemoveCurrencyDto
{
    public string? id { get; set; }
    public decimal? quantity {get; set; }
    public Guid? wallet_id {get; set;}
}
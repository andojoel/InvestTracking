namespace Services.InvestTrackingAPI.Models.Dto;
public class AddCurrencyDto
{
    public string? id { get; set; }
    public decimal? quantity {get; set; }
    public Guid? wallet_id {get; set;}
}
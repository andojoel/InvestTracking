using System.ComponentModel.DataAnnotations;

namespace Services.InvestTrackingAPI.Models;

public class Wallet
{
    [Key]
    public required Guid id {get; set;}
    public required string name {get; set;}
    public string? ownerUserName {get; set;}
}
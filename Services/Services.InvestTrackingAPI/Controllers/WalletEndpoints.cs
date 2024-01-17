using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Services.InvestTrackingAPI.Data;
using Services.InvestTrackingAPI.Models;
using Services.InvestTrackingAPI.Models.Dto;

namespace Services.InvestTrackingAPI.Controllers;

public static class WalletEndpoints
{
    public static void MapWalletEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/wallets").RequireAuthorization();

        group.MapGet("", GetWallets);
        group.MapPost("create", CreateWallet);
        group.MapPut("updateName", UpdateWalletName);
        group.MapDelete("delete/{wallet_id}", DeleteWallet);
        group.MapGet("{wallet_id}/value", GetWalletCurrentValue);
    }

    public static IResult GetWallets(
        AppDbContext db, 
        IMapper mapper, 
        ClaimsPrincipal user)
    {
        var _responseDto = new ResponseDto();
        try {
            var wallets = db.WalletTable.Where(w => w.ownerUserName == user.Identity!.Name);
            _responseDto.IsSuccess = true;
            _responseDto.Result = mapper.Map<List<WalletDto>>(wallets);
            return Results.Ok(_responseDto);
        }
        catch(Exception e){
            _responseDto.IsSuccess = false;
            _responseDto.Message = e.Message;
            return Results.BadRequest(_responseDto);
        }
    }

    public static async Task<IResult> CreateWallet(
        AppDbContext db, 
        IMapper mapper, 
        [FromBody] NameDto wallet,
        ClaimsPrincipal user)
    {
        var _responseDto = new ResponseDto();
        try{
            Wallet _wallet = new Wallet(){
                    id = new Guid(),
                    name = string.IsNullOrEmpty(wallet.name) ? "My Wallet" : wallet.name,
                    ownerUserName = user.Identity!.Name
                    };
                await db.WalletTable.AddAsync(_wallet);
                await db.SaveChangesAsync();
                _responseDto.IsSuccess = true;
                _responseDto.Message = "Done";
                _responseDto.Result = mapper.Map<WalletDto>(_wallet);
                return Results.Ok(_responseDto);
        }
        catch(Exception e){
            _responseDto.IsSuccess = false;
            _responseDto.Message = e.Message;
            return Results.BadRequest(_responseDto);
        }
        
    }

    public static async Task<IResult> UpdateWalletName(
        AppDbContext db, 
        IMapper mapper, 
        [FromBody] WalletDto wallet,
        ClaimsPrincipal user)
    {
        var _responseDto = new ResponseDto();
        try{
            Wallet _wallet = db.WalletTable.First(w => w.id == wallet.id && w.ownerUserName == user.Identity!.Name);
            _wallet.name = string.IsNullOrEmpty(wallet.name) ? "My Wallet" : wallet.name;
            db.Entry(_wallet).CurrentValues.SetValues(_wallet);
            // db.WalletTable.Update(_wallet);
            await db.SaveChangesAsync();
            _responseDto.IsSuccess = true;
            _responseDto.Message = "Done";
            _responseDto.Result = mapper.Map<WalletDto>(_wallet);
            return Results.Ok(_responseDto);
        }
        catch(Exception e){
            _responseDto.IsSuccess = false;
            _responseDto.Message = e.Message;
            return Results.BadRequest(_responseDto);
        }
    }

    public static async Task<IResult> DeleteWallet(
        AppDbContext db, 
        Guid wallet_id,
        ClaimsPrincipal user)
    {
        var _responseDto = new ResponseDto();
        try{
            Wallet _wallet = db.WalletTable.First(w => w.id == wallet_id && w.ownerUserName == user.Identity!.Name);
            db.WalletTable.Remove(_wallet);
            await db.SaveChangesAsync();
            _responseDto.IsSuccess = true;
            _responseDto.Message = "Done";
            return Results.Ok(_responseDto);
        }
        catch(Exception e){
            _responseDto.IsSuccess = false;
            _responseDto.Message = e.Message;
            return Results.BadRequest(_responseDto);
        }
    }

    public static async Task<IResult> GetWalletCurrentValue(
        AppDbContext db, 
        IMapper mapper,
        Guid wallet_id, 
        ClaimsPrincipal user)
    {
        var _responseDto = new ResponseDto();
        try {

            var currencies = db.CurrencyTable.Where(curr => curr.wallet_id == wallet_id && curr.ownerUserName == user.Identity!.Name);
            Wallet wallet = db.WalletTable.First(w => w.id == wallet_id && w.ownerUserName == user.Identity!.Name);

            if(currencies is null || wallet is null) throw new Exception("Currencies or Wallet not found");

            if(currencies.Count() <= 0) throw new Exception("Currencies not found");

            decimal? sum = 0;
            foreach (var currency in currencies)
            {
                Uri resourceUri = new Uri($"http://localhost:5104/api/CurrenciesAPI/{currency.id}");
                HttpClient _httpClient = new HttpClient();
                JsonSerializerSettings? _serializerSettings = new JsonSerializerSettings();
                var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, resourceUri))
                    .ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                var currencyFromResult = JsonConvert.DeserializeObject<Currency>(responseContent, _serializerSettings);

                if (currencyFromResult is not null && currencyFromResult.current_price is not null ) sum = sum + (currency.quantity * currencyFromResult.current_price);
            }

            _responseDto.IsSuccess = true;
            _responseDto.Result = new WalletTotalValueDto(){
                id = wallet_id,
                name = wallet.name,
                value = sum
            };
            return Results.Ok(_responseDto);
        }
        catch(Exception e){
            _responseDto.IsSuccess = false;
            _responseDto.Message = e.Message;
            return Results.BadRequest(_responseDto);
        }
    }
}

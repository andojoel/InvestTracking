using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Services.InvestTrackingAPI.Data;
using Services.InvestTrackingAPI.Models;
using Services.InvestTrackingAPI.Models.Dto;

namespace Services.InvestTrackingAPI.Controllers;

public static class CurrencyEndpoints
{
    public static void MapCurrencyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/currencies").RequireAuthorization();

        group.MapGet("admin/getAllCurrency", adminGetAllCurrencyList);
        group.MapGet("list", getCurrencyList);
        group.MapGet("", getCurrenciesInWallet);
        group.MapPost("add", AddCurrencyToWallet);
        group.MapPost("remove", RemoveCurrencyInWallet);
    }

    public static IResult adminGetAllCurrencyList(
        AppDbContext db,
        IMapper mapper
        )
    {
        var _responseDto = new ResponseDto();
        try
        {
            return Results.Ok(db.CurrencyTable.ToList());
        }
        catch(Exception e){
            _responseDto.IsSuccess = false;
            _responseDto.Message = e.Message;
            return Results.BadRequest(_responseDto);
        }
    }

    public static async Task<IResult?> getCurrencyList(
        AppDbContext db, 
        IMapper mapper,
        ClaimsPrincipal user
        )
    {
        var _responseDto = new ResponseDto();
        try
        {
            Uri resourceUri = new Uri("http://localhost:5104/api/CurrenciesAPI");
            HttpClient _httpClient = new HttpClient();
            JsonSerializerSettings? _serializerSettings = new JsonSerializerSettings();
            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, resourceUri))
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
        
            var result = JsonConvert.DeserializeObject<List<Currency>>(responseContent, _serializerSettings);
            _responseDto.Result = result;
            
            return Results.Ok(_responseDto);
        }
        catch(Exception e){
            _responseDto.IsSuccess = false;
            _responseDto.Message = e.Message;
            return Results.BadRequest(_responseDto);
        }
    }

    public static async Task<IResult?> getCurrenciesInWallet(
        [FromQuery] Guid? wallet_id,
        AppDbContext db, 
        IMapper mapper,
        ClaimsPrincipal user
        )
    {
        var _responseDto = new ResponseDto();
        try
        {
            if(wallet_id is null) throw new Exception("You have to provide the parameter wallet_id");
            var currencies = db.CurrencyTable.Where(curr => curr.wallet_id == wallet_id).ToList();
            for(int j=0; j<currencies.Count(); j++)
            {
                Uri resourceUri = new Uri($"http://localhost:5104/api/CurrenciesAPI/{currencies[j].id}");
                HttpClient _httpClient = new HttpClient();
                JsonSerializerSettings? _serializerSettings = new JsonSerializerSettings();
                var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, resourceUri))
                    .ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                var currencyFromResult = JsonConvert.DeserializeObject<Currency>(responseContent, _serializerSettings);

                currencies[j].current_price = currencyFromResult!.current_price;
                currencies[j].market_cap = currencyFromResult!.market_cap;
                currencies[j].market_cap_rank = currencyFromResult!.market_cap_rank;
                currencies[j].image = currencyFromResult!.image;
                currencies[j].price_change_24h = currencyFromResult!.price_change_24h;
                currencies[j].price_change_percentage_24h = currencyFromResult!.price_change_percentage_24h;
                currencies[j].total_value = currencyFromResult!.current_price * currencies[j].quantity;
            }

            _responseDto.IsSuccess = true;
            _responseDto.Result = mapper.Map<List<CurrencyDto>>(currencies);
            
            return Results.Ok(_responseDto);
        }
        catch(Exception e){
            _responseDto.IsSuccess = false;
            _responseDto.Message = e.Message;
            return Results.BadRequest(_responseDto);
        }
    }

    public static async Task<IResult> AddCurrencyToWallet(
        AppDbContext db,
        IMapper mapper, 
        [FromBody]AddCurrencyDto currencyInBody, 
        ClaimsPrincipal user)
    {
        var _responseDto = new ResponseDto();
        try{
            // Check if the wallet correspond to the user
            if(currencyInBody.wallet_id is null || currencyInBody.wallet_id.ToString() == "") 
                throw new Exception("A wallet Id should not be null or empty");
            
            Wallet? _wallet = db.WalletTable.FirstOrDefault(w => w.id == currencyInBody.wallet_id);

            if(_wallet is null) 
                throw new Exception("You cannot buy currencies for this wallet");

            if(_wallet.ownerUserName != user.Identity!.Name) 
                throw new Exception("Not authorized");

            if(currencyInBody.id is null) 
                throw new Exception("The id should not be null or empty");

            if(currencyInBody.quantity <= 0) 
                throw new Exception("The quantity should be above 0");
            
            // Check if the currency is on the list
            Uri resourceUri = new Uri($"http://localhost:5104/api/CurrenciesAPI/{currencyInBody.id}");
            HttpClient _httpClient = new HttpClient();
            JsonSerializerSettings? _serializerSettings = new JsonSerializerSettings();
            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, resourceUri))
                .ConfigureAwait(false);
            // The status code will be success if the result match the id of the cuurrency
            // Otherwise, it will return bad request status code and raise an error
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var currencyFromResult = JsonConvert.DeserializeObject<Currency>(responseContent, _serializerSettings);

            if(currencyFromResult is null) 
                throw new Exception("The currency you wants to buy is incorrect");

            // Check if the currency is already on the wallet
            var curr = db.CurrencyTable.FirstOrDefault(c => c.id == currencyFromResult.id && c.ownerUserName == user.Identity!.Name && c.wallet_id == currencyInBody.wallet_id);
            if(curr is null)
            {
                Currency _curr = new Currency(){
                    id = currencyFromResult.id,
                    image = currencyFromResult.image,
                    name = currencyFromResult.name,
                    symbol = currencyFromResult.symbol,
                    current_price = currencyFromResult.current_price,
                    market_cap = currencyFromResult.market_cap,
                    market_cap_rank = currencyFromResult.market_cap_rank,
                    price_change_24h = currencyFromResult.price_change_24h,
                    price_change_percentage_24h = currencyFromResult.price_change_percentage_24h,
                    quantity = currencyInBody.quantity,
                    total_value = currencyInBody.quantity * currencyFromResult.current_price,
                    ownerUserName = user.Identity!.Name,
                    wallet_id = currencyInBody.wallet_id
                };
                await db.CurrencyTable.AddAsync(_curr);
                await db.SaveChangesAsync();
                _responseDto.IsSuccess = true;
                _responseDto.Message = "Currency bought successfullly!";
                _responseDto.Result = mapper.Map<CurrencyDto>(_curr);
                return Results.Ok(_responseDto);
            }
            else {
                Currency _curr = new Currency(){
                    id = currencyFromResult.id,
                    image = currencyFromResult.image,
                    name = currencyFromResult.name,
                    symbol = currencyFromResult.symbol,
                    current_price = currencyFromResult.current_price,
                    market_cap = currencyFromResult.market_cap,
                    market_cap_rank = currencyFromResult.market_cap_rank,
                    price_change_24h = currencyFromResult.price_change_24h,
                    price_change_percentage_24h = currencyFromResult.price_change_percentage_24h,
                    quantity = curr.quantity + currencyInBody.quantity,
                    total_value = (curr.quantity + currencyInBody.quantity) * currencyFromResult.current_price,
                    ownerUserName = user.Identity!.Name,
                    wallet_id = currencyInBody.wallet_id
                };
                db.Entry(curr).CurrentValues.SetValues(_curr);
                db.SaveChanges();
                _responseDto.IsSuccess = true;
                _responseDto.Message = "Currency bought successfullly!";
                _responseDto.Result = mapper.Map<CurrencyDto>(_curr);
                return Results.Ok(_responseDto);
            }
            
        }
        catch(Exception e){
            _responseDto.IsSuccess = false;
            _responseDto.Message = e.Message;
            return Results.BadRequest(_responseDto);
        }
    }

    // Methods to sell currencies
    public static async Task<IResult> RemoveCurrencyInWallet(
        AppDbContext db, 
        IMapper mapper,
        [FromBody]RemoveCurrencyDto currencyInBody, 
        ClaimsPrincipal user)
    {
        var _responseDto = new ResponseDto();
        try{
            if(currencyInBody.id is null) 
                throw new Exception($"The Id should not be null");

            if(currencyInBody.wallet_id is null) 
                throw new Exception($"Wallet not found");

            if(currencyInBody.quantity is null) 
                throw new Exception($"Quantity should not be null");

            Currency? curr = db.CurrencyTable.FirstOrDefault(c => c.id == currencyInBody.id && c.ownerUserName == user.Identity!.Name && c.wallet_id == currencyInBody.wallet_id);

            if(curr is null) 
                throw new Exception($"Currency or Wallet not found");

            if(currencyInBody.quantity! <= 0) 
                throw new Exception($"You can not sell {currencyInBody.quantity} of {curr.name}");

            if(currencyInBody.quantity! > curr.quantity) 
                throw new Exception($"Not enough {curr.name} to sell");

            if(currencyInBody.quantity == curr.quantity)
            {
                db.CurrencyTable.Remove(curr);
                await db.SaveChangesAsync();
                _responseDto.IsSuccess = true;
                _responseDto.Message = "Done";
            }
            else 
            {
                // Check if the currency is on the list
                Uri resourceUri = new Uri($"http://localhost:5104/api/CurrenciesAPI/{curr.id}");
                HttpClient _httpClient = new HttpClient();
                JsonSerializerSettings? _serializerSettings = new JsonSerializerSettings();
                var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, resourceUri))
                    .ConfigureAwait(false);
                // The status code will be success if the result match the id of the cuurrency
                // Otherwise, it will return bad request status code and raise an error
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                var currencyFromResult = JsonConvert.DeserializeObject<Currency>(responseContent, _serializerSettings);
                if(currencyFromResult is null){
                    throw new Exception("The currency you wants to sell is incorrect");
                }
                
                Currency _curr = new Currency(){
                    id = currencyFromResult.id,
                    image = currencyFromResult.image,
                    name = currencyFromResult.name,
                    symbol = currencyFromResult.symbol,
                    current_price = currencyFromResult.current_price,
                    market_cap = currencyFromResult.market_cap,
                    market_cap_rank = currencyFromResult.market_cap_rank,
                    price_change_24h = currencyFromResult.price_change_24h,
                    price_change_percentage_24h = currencyFromResult.price_change_percentage_24h,
                    quantity = curr.quantity - currencyInBody.quantity,
                    total_value = (curr.quantity + currencyInBody.quantity) * currencyFromResult.current_price,
                    ownerUserName = user.Identity!.Name,
                    wallet_id = curr.wallet_id
                };
                db.Entry(curr).CurrentValues.SetValues(_curr);
                db.SaveChanges();
                _responseDto.IsSuccess = true;
                _responseDto.Message = "Currency sold successfullly!";
                _responseDto.Result = mapper.Map<CurrencyDto>(_curr);
            }
            return Results.Ok(_responseDto);
        }
        catch(Exception e){
            _responseDto.IsSuccess = false;
            _responseDto.Message = e.Message;
            return Results.BadRequest(_responseDto);
        }
    }

    // Methods to exchange currencies
}
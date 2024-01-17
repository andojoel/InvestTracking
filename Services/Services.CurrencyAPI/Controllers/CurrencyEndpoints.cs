using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Nelibur.ObjectMapper;
using Services.CurrencyAPI.Data;
using Services.CurrencyAPI.Models;
using Services.CurrencyAPI.Models.Dto;

namespace Services.CurrencyAPI.Controllers
{
    public static class CurrencyEndpoints
    {
        public static void MapCurrencyEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/CurrenciesAPI");

            group.MapGet("", GetAllCurrency);
            group.MapPost("", GetArrayOfCurrency);
            group.MapGet("{id}", GetCurrencyById);
            group.MapPost("UpdateOrAdd", UpdateOrAdd);
            group.MapDelete("Delete", DeleteAllCurrency);
            group.MapDelete("Delete/{id}", DeleteCurrencyById);
        }

        //On récupère tous les éléments de la base
        public static IResult? GetAllCurrency(AppDbContext _db)
        {
            try
            {
                List<Currency> currencyList = _db.Currency.ToList();
                return Results.Ok(TinyMapper.Map<List<CurrencyDto>>(currencyList));
            }
            catch (Exception e)
            {
                return Results.BadRequest(e.Message);
            }
        }

        //On récupère tous les currency en fonction d'un tableau de currency
        public static IResult? GetArrayOfCurrency(AppDbContext _db, [FromBody]List<CurrencyDto2> currencies)
        {
            try
            {
                List<Currency> currencyList = new List<Currency>();
                foreach(var curr in currencies)
                {
                    Currency? _curr = _db.Currency.FirstOrDefault(c => c.id == curr.id);
                    if(_curr is not null)
                    {
                        currencyList.Add(_curr);
                    }
                    
                }
                return Results.Ok(TinyMapper.Map<List<CurrencyDto>>(currencyList));
            }
            catch (Exception e)
            {
                return Results.BadRequest(e.Message);
            }
        }

        //On récupère un seul élément dans la base
        public static IResult GetCurrencyById(AppDbContext _db, string id)
        {
            try
            {
                Currency currency = _db.Currency.First(el => el.id.ToLower() == id.ToLower());
                return Results.Ok(TinyMapper.Map<CurrencyDto>(currency));
            }
            catch (Exception e)
            {
                return Results.BadRequest(e.Message);
            }
        }

        //On update ou ajoute un élément dans la base
        public static IResult UpdateOrAdd(AppDbContext _db, [FromBody] CurrencyDto currency)
        {
            try
            {
                Currency? _currency = _db.Currency.FirstOrDefault(t => t.id.ToLower() == currency.id.ToLower());
                if (_currency == null)
                {
                    _db.Currency.Add(TinyMapper.Map<Currency>(currency));
                    _db.SaveChanges();
                    return Results.Ok(TinyMapper.Map<CurrencyDto>(currency));
                }
                else if (_currency.current_price != currency.current_price)
                {
                    //On update l'élément
                    _db.Entry(_currency).CurrentValues.SetValues(TinyMapper.Map<Currency>(currency));
                    _db.SaveChanges();
                    return Results.Ok(currency);
                }
                else
                {
                    return Results.BadRequest("The currency is already up to date");
                }
            }
            catch (Exception e)
            {
                return Results.BadRequest(e.Message);
            }
        }

        //On supprime tous les éléments dans la base
        public static IResult DeleteAllCurrency(AppDbContext _db)
        {
            try
            {
                _db.Currency.RemoveRange(_db.Currency.ToList());
                _db.SaveChanges();
                return Results.Ok("All currency was deleted successfully !");
            }
            catch (Exception e)
            {
                return Results.BadRequest(e.Message);
            }
        }

        //On supprime un élément dans la base
        [HttpDelete]
        [Route("Delete/{id}")]
        public static IResult DeleteCurrencyById(AppDbContext _db, string id)
        {
            try
            {
                Currency currency = _db.Currency.First(el => el.id.ToLower() == id.ToLower());
                _db.Currency.Remove(currency);
                _db.SaveChanges();
                return Results.Ok($"The currency with the id:{id} was deleted successfully !");
            }
            catch (Exception e)
            {
                return Results.BadRequest(e.Message);
            }
        }
    }
}

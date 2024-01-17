using Services.CurrencyAPI.Models;
using Services.CurrencyAPI.Models.Dto;
using Nelibur.ObjectMapper;

namespace Services.CurrencyAPI
{
    public class MappingConfig
    {
        public static void RegisterMaps()
        {
            TinyMapper.Bind<CurrencyDto, Currency>();
            TinyMapper.Bind<Currency, CurrencyDto>();
            TinyMapper.Bind<List<Currency>, List<CurrencyDto>>();
            TinyMapper.Bind<List<Currency>, List<CurrencyDto>>();
        }
    }
}

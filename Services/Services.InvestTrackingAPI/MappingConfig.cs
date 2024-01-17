using Services.InvestTrackingAPI.Models;
using Services.InvestTrackingAPI.Models.Dto;
using AutoMapper;

public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<Currency, CurrencyDto>();
                config.CreateMap<CurrencyDto, Currency>();

                config.CreateMap<Wallet, WalletDto>();
                config.CreateMap<WalletDto, Wallet>();
            });
            return mappingConfig;
        }
    }

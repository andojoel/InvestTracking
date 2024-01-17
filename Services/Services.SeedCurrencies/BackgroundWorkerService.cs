using Services.SeedCurrencies.Models;
using Newtonsoft.Json;

namespace Services.SeedCurrencies
{
    public class BackgroundWorkerService : BackgroundService
    {
        private readonly ILogger<BackgroundWorkerService> _logger;

        public BackgroundWorkerService(ILogger<BackgroundWorkerService> logger)
        {
            _logger = logger;
        }

        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Getting cryptocurrencies information...");
                try
                {
                    var result = await getCryptoList("usd");
                    if (!(result is null))
                    {
                        foreach (Currency crypto in result)
                        {
                            Currency _crypto = new Currency()
                            {
                                id = crypto.id, 
                                name = crypto.name, 
                                symbol = crypto.symbol,
                                current_price = crypto.current_price, 
                                market_cap = crypto.market_cap,
                                market_cap_rank = crypto.market_cap_rank,
                                price_change_24h = crypto.price_change_24h,
                                price_change_percentage_24h = crypto.price_change_percentage_24h,
                                image = crypto.image
                            };
                            HttpClient client = new HttpClient();
                            client.BaseAddress = new Uri("http://localhost:5104/api/CurrenciesAPI/");
                            var json  = System.Text.Json.JsonSerializer.Serialize(_crypto);
                            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                            var response = client.PostAsync("UpdateOrAdd", content).Result;
                            if (!response.IsSuccessStatusCode)
                            {
                                _logger.LogInformation(response.Content.ReadAsStringAsync().Result);
                            }
                        }
                        _logger.LogInformation("Database updated");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation("An error occured when atempting to fetch cryptocurrencies information.\n" + ex.Message);
                }
                // Pause every 45 seconds
                await Task.Delay(45000, cancellationToken);
            }
        }

        public async Task<Currency[]?> getCryptoList(string vs_currency)
        {
            Uri resourceUri = new Uri($"https://api.coingecko.com/api/v3/coins/markets?vs_currency={vs_currency}&order=market_cap_desc&per_page=200&locale=fr");

            HttpClient _httpClient = new HttpClient();
            JsonSerializerSettings? _serializerSettings = new JsonSerializerSettings();

            _httpClient.DefaultRequestHeaders.Add("User-Agent", "PostmanRuntime/7.32.3");
            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, resourceUri))
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            try
            {
                return JsonConvert.DeserializeObject<Currency[]>(responseContent, _serializerSettings);
            }
            catch
            {
                return null;
            }
        }
    }
}
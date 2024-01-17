import requests

url = "https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&order=market_cap_desc&per_page=200&locale=fr"
headers = {'X-CoinAPI-Key' : '06E5AFBF-7137-4D68-B2A3-B16C7CCB047C'}
response = requests.get(url)

print(response)
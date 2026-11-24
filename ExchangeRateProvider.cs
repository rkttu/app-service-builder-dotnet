using System.Text.Json;

internal sealed class ExchangeRateProvider(HttpClient httpClient)
{
    private static readonly CurrencyDefinition[] Currencies =
    [
        new("USD", "미국 달러", "미국"),
        new("EUR", "유로", "유럽연합"),
        new("JPY", "일본 엔", "일본"),
        new("CNY", "중국 위안", "중국"),
        new("GBP", "영국 파운드", "영국"),
        new("AUD", "호주 달러", "호주"),
        new("CAD", "캐나다 달러", "캐나다"),
        new("CHF", "스위스 프랑", "스위스")
    ];

    public async Task<ExchangeRateSnapshot> GetLatestAsync(CancellationToken cancellationToken)
    {
        var symbols = string.Join(',', new[] { "KRW" }.Concat(Currencies.Select(currency => currency.Code).Where(code => code != "USD")));
        var path = $"v1/latest?base=USD&symbols={symbols}";
        using var response = await httpClient.GetAsync(path, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var root = document.RootElement;
        var rates = root.GetProperty("rates");
        var krwPerUsd = rates.GetProperty("KRW").GetDecimal();
        if (krwPerUsd <= 0)
        {
            throw new InvalidDataException("Frankfurter returned an invalid KRW rate.");
        }

        var result = new List<ExchangeRate>(Currencies.Length);
        foreach (var currency in Currencies)
        {
            decimal krwPerUnit;
            if (currency.Code == "USD")
            {
                krwPerUnit = krwPerUsd;
            }
            else
            {
                var unitsPerUsd = rates.GetProperty(currency.Code).GetDecimal();
                if (unitsPerUsd <= 0)
                {
                    throw new InvalidDataException($"Frankfurter returned an invalid {currency.Code} rate.");
                }

                krwPerUnit = krwPerUsd / unitsPerUsd;
            }

            result.Add(new ExchangeRate(
                currency.Code,
                currency.Name,
                currency.Country,
                decimal.Round(
                    krwPerUnit,
                    currency.Code == "JPY" ? 6 : 2,
                    MidpointRounding.AwayFromZero)));
        }

        return new ExchangeRateSnapshot(
            true,
            DateTimeOffset.UtcNow,
            root.GetProperty("date").GetString() ?? throw new InvalidDataException("Frankfurter returned no rate date."),
            result,
            null);
    }

    private sealed record CurrencyDefinition(string Code, string Name, string Country);
}

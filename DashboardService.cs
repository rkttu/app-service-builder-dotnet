using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

internal sealed class DashboardService(
    WeatherProvider weatherProvider,
    ExchangeRateProvider exchangeRateProvider,
    IMemoryCache cache,
    ILogger<DashboardService> logger)
{
    private static readonly TimeZoneInfo KoreaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul");
    private static readonly CultureInfo KoreanCulture = CultureInfo.GetCultureInfo("ko-KR");
    private const string WeatherCacheKey = "dashboard:weather";
    private const string ExchangeRateCacheKey = "dashboard:exchange-rates";

    public async Task<WeatherSnapshot> GetWeatherAsync(CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(WeatherCacheKey, out WeatherSnapshot? cached) && cached is not null)
        {
            return cached;
        }

        WeatherSnapshot snapshot;
        try
        {
            var locations = await weatherProvider.GetTodayAsync(cancellationToken);
            var updatedAt = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, KoreaTimeZone);
            snapshot = new WeatherSnapshot(true, updatedAt, updatedAt.Date.ToString("yyyy-MM-dd"), locations, null);
            cache.Set(WeatherCacheKey, snapshot, TimeSpan.FromMinutes(10));
        }
        catch (Exception exception) when (IsExternalProviderException(exception, cancellationToken))
        {
            logger.LogWarning(exception, "Weather provider request failed.");
            snapshot = new WeatherSnapshot(
                false,
                null,
                null,
                [],
                "날씨 제공 서비스에 일시적으로 연결할 수 없습니다.");
            cache.Set(WeatherCacheKey, snapshot, TimeSpan.FromSeconds(30));
        }

        return snapshot;
    }

    public async Task<ExchangeRateSnapshot> GetExchangeRatesAsync(CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(ExchangeRateCacheKey, out ExchangeRateSnapshot? cached) && cached is not null)
        {
            return cached;
        }

        ExchangeRateSnapshot snapshot;
        try
        {
            snapshot = await exchangeRateProvider.GetLatestAsync(cancellationToken);
            cache.Set(ExchangeRateCacheKey, snapshot, TimeSpan.FromMinutes(30));
        }
        catch (Exception exception) when (IsExternalProviderException(exception, cancellationToken))
        {
            logger.LogWarning(exception, "Exchange-rate provider request failed.");
            snapshot = new ExchangeRateSnapshot(
                false,
                null,
                null,
                [],
                "환율 제공 서비스에 일시적으로 연결할 수 없습니다.");
            cache.Set(ExchangeRateCacheKey, snapshot, TimeSpan.FromSeconds(30));
        }

        return snapshot;
    }

    public async Task<DashboardSnapshot> GetDashboardAsync(CancellationToken cancellationToken)
    {
        var weatherTask = GetWeatherAsync(cancellationToken);
        var exchangeRateTask = GetExchangeRatesAsync(cancellationToken);
        await Task.WhenAll(weatherTask, exchangeRateTask);

        var localNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, KoreaTimeZone);
        var weather = await weatherTask;
        var exchangeRates = await exchangeRateTask;
        var sources = new[]
        {
            new SourceStatus("Open-Meteo", weather.IsAvailable, weather.Message),
            new SourceStatus("Frankfurter", exchangeRates.IsAvailable, exchangeRates.Message)
        };

        return new DashboardSnapshot(
            localNow,
            localNow.ToString("yyyy년 M월 d일 dddd", KoreanCulture),
            weather,
            exchangeRates,
            sources);
    }

    private static bool IsExternalProviderException(Exception exception, CancellationToken cancellationToken) =>
        !cancellationToken.IsCancellationRequested &&
        exception is HttpRequestException
            or JsonException
            or InvalidDataException
            or InvalidOperationException
            or KeyNotFoundException
            or IndexOutOfRangeException
            or FormatException
            or OverflowException
            or TaskCanceledException;
}

internal sealed record DashboardSnapshot(
    DateTimeOffset GeneratedAt,
    string LocalDate,
    WeatherSnapshot Weather,
    ExchangeRateSnapshot ExchangeRates,
    IReadOnlyList<SourceStatus> Sources);

internal sealed record SourceStatus(string Name, bool IsAvailable, string? Message);

internal sealed record WeatherSnapshot(
    bool IsAvailable,
    DateTimeOffset? UpdatedAt,
    string? Date,
    IReadOnlyList<WeatherLocation> Locations,
    string? Message);

internal sealed record WeatherLocation(
    string Code,
    string Region,
    string City,
    double Latitude,
    double Longitude,
    double Temperature,
    double High,
    double Low,
    double ApparentTemperature,
    double Humidity,
    double PrecipitationProbability,
    double Precipitation,
    double WindSpeed,
    int WeatherCode,
    string Condition,
    string Icon,
    bool IsDay);

internal sealed record ExchangeRateSnapshot(
    bool IsAvailable,
    DateTimeOffset? UpdatedAt,
    string? Date,
    IReadOnlyList<ExchangeRate> Rates,
    string? Message);

internal sealed record ExchangeRate(
    string Code,
    string Name,
    string Country,
    decimal KrwPerUnit);

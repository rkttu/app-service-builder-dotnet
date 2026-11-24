var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:8080");

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<WeatherProvider>(client =>
{
    client.BaseAddress = new Uri("https://api.open-meteo.com/");
    client.Timeout = TimeSpan.FromSeconds(12);
});
builder.Services.AddHttpClient<ExchangeRateProvider>(client =>
{
    client.BaseAddress = new Uri("https://api.frankfurter.dev/");
    client.Timeout = TimeSpan.FromSeconds(12);
});
builder.Services.AddSingleton<DashboardService>();

var app = builder.Build();

app.MapGet("/health", () => Results.Text("OK\n"));
app.MapGet("/", () => Results.Content(DashboardPage.Render(), "text/html; charset=utf-8"));

app.MapGet("/api/dashboard", async (DashboardService service, HttpContext context, CancellationToken cancellationToken) =>
{
    var dashboard = await service.GetDashboardAsync(cancellationToken);
    context.Response.Headers.CacheControl = "public, max-age=60";
    return Results.Json(dashboard, statusCode: dashboard.Weather.IsAvailable && dashboard.ExchangeRates.IsAvailable
        ? StatusCodes.Status200OK
        : dashboard.Weather.IsAvailable || dashboard.ExchangeRates.IsAvailable
            ? StatusCodes.Status206PartialContent
            : StatusCodes.Status503ServiceUnavailable);
});

app.MapGet("/api/weather", async (DashboardService service, HttpContext context, CancellationToken cancellationToken) =>
{
    var weather = await service.GetWeatherAsync(cancellationToken);
    context.Response.Headers.CacheControl = "public, max-age=60";
    return Results.Json(weather, statusCode: weather.IsAvailable
        ? StatusCodes.Status200OK
        : StatusCodes.Status503ServiceUnavailable);
});

app.MapGet("/api/exchange-rates", async (DashboardService service, HttpContext context, CancellationToken cancellationToken) =>
{
    var exchangeRates = await service.GetExchangeRatesAsync(cancellationToken);
    context.Response.Headers.CacheControl = "public, max-age=300";
    return Results.Json(exchangeRates, statusCode: exchangeRates.IsAvailable
        ? StatusCodes.Status200OK
        : StatusCodes.Status503ServiceUnavailable);
});

app.Run();

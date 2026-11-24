using System.Globalization;
using System.Text.Json;

internal sealed class WeatherProvider(HttpClient httpClient)
{
    private static readonly Region[] Regions =
    [
        new("SEOUL", "서울특별시", "서울", 37.5665, 126.9780),
        new("BUSAN", "부산광역시", "부산", 35.1796, 129.0756),
        new("DAEGU", "대구광역시", "대구", 35.8714, 128.6014),
        new("INCHEON", "인천광역시", "인천", 37.4563, 126.7052),
        new("GWANGJU", "광주광역시", "광주", 35.1595, 126.8526),
        new("DAEJEON", "대전광역시", "대전", 36.3504, 127.3845),
        new("ULSAN", "울산광역시", "울산", 35.5384, 129.3114),
        new("SEJONG", "세종특별자치시", "세종", 36.4801, 127.2890),
        new("GYEONGGI", "경기도", "수원", 37.2636, 127.0286),
        new("GANGWON", "강원특별자치도", "춘천", 37.8813, 127.7298),
        new("CHUNGBUK", "충청북도", "청주", 36.6424, 127.4890),
        new("CHUNGNAM", "충청남도", "홍성", 36.6010, 126.6608),
        new("JEONBUK", "전북특별자치도", "전주", 35.8242, 127.1480),
        new("JEONNAM", "전라남도", "무안", 34.9905, 126.4816),
        new("GYEONGBUK", "경상북도", "안동", 36.5684, 128.7294),
        new("GYEONGNAM", "경상남도", "창원", 35.2286, 128.6811),
        new("JEJU", "제주특별자치도", "제주", 33.4996, 126.5312)
    ];

    public async Task<IReadOnlyList<WeatherLocation>> GetTodayAsync(CancellationToken cancellationToken)
    {
        var latitudes = string.Join(',', Regions.Select(region => region.Latitude.ToString(CultureInfo.InvariantCulture)));
        var longitudes = string.Join(',', Regions.Select(region => region.Longitude.ToString(CultureInfo.InvariantCulture)));
        var path = "v1/forecast" +
            $"?latitude={latitudes}" +
            $"&longitude={longitudes}" +
            "&current=temperature_2m,relative_humidity_2m,apparent_temperature,is_day,precipitation,weather_code,wind_speed_10m" +
            "&daily=temperature_2m_max,temperature_2m_min,precipitation_probability_max" +
            "&timezone=Asia%2FSeoul&forecast_days=1";

        using var response = await httpClient.GetAsync(path, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var responses = document.RootElement.ValueKind switch
        {
            JsonValueKind.Array => document.RootElement.EnumerateArray().ToArray(),
            JsonValueKind.Object => [document.RootElement],
            _ => throw new InvalidDataException("Open-Meteo returned an unexpected response.")
        };

        if (responses.Length != Regions.Length)
        {
            throw new InvalidDataException("Open-Meteo did not return weather for every configured region.");
        }

        var locations = new List<WeatherLocation>(Regions.Length);
        for (var index = 0; index < Regions.Length; index++)
        {
            var region = Regions[index];
            var root = responses[index];
            var current = root.GetProperty("current");
            var daily = root.GetProperty("daily");
            var weatherCode = ReadInt(current, "weather_code");
            var (condition, icon) = DescribeWeather(weatherCode);

            locations.Add(new WeatherLocation(
                region.Code,
                region.AreaName,
                region.City,
                region.Latitude,
                region.Longitude,
                ReadDouble(current, "temperature_2m"),
                ReadFirstDouble(daily, "temperature_2m_max"),
                ReadFirstDouble(daily, "temperature_2m_min"),
                ReadDouble(current, "apparent_temperature"),
                ReadDouble(current, "relative_humidity_2m"),
                ReadFirstDouble(daily, "precipitation_probability_max"),
                ReadDouble(current, "precipitation"),
                ReadDouble(current, "wind_speed_10m"),
                weatherCode,
                condition,
                icon,
                ReadInt(current, "is_day") == 1));
        }

        return locations;
    }

    private static double ReadDouble(JsonElement element, string propertyName) =>
        element.GetProperty(propertyName).GetDouble();

    private static int ReadInt(JsonElement element, string propertyName) =>
        element.GetProperty(propertyName).GetInt32();

    private static double ReadFirstDouble(JsonElement element, string propertyName) =>
        element.GetProperty(propertyName)[0].GetDouble();

    private static (string Condition, string Icon) DescribeWeather(int code) => code switch
    {
        0 => ("맑음", "☀"),
        1 => ("대체로 맑음", "🌤"),
        2 => ("구름 조금", "⛅"),
        3 => ("흐림", "☁"),
        45 or 48 => ("안개", "🌫"),
        51 or 53 or 55 or 56 or 57 => ("이슬비", "🌦"),
        61 => ("약한 비", "🌦"),
        63 or 65 or 80 or 81 or 82 => ("비", "🌧"),
        66 or 67 => ("진눈깨비", "🌨"),
        71 or 73 or 75 or 77 or 85 or 86 => ("눈", "❄"),
        95 or 96 or 99 => ("천둥번개", "⛈"),
        _ => ("날씨 정보", "◌")
    };

    private sealed record Region(
        string Code,
        string AreaName,
        string City,
        double Latitude,
        double Longitude);
}

namespace Global360.Automation.Config;

/// <summary>
/// Test configuration. Values come from environment variables, falling back to the
/// public demo defaults documented on the Snipe-IT login page.
/// </summary>
public sealed record TestSettings(
    string BaseUrl,
    string Username,
    string Password,
    string ServerTimeZoneId)
{
    public static TestSettings Load() => new(
        BaseUrl: Env("SNIPEIT_BASE_URL", "https://demo.snipeitapp.com").TrimEnd('/'),
        Username: Env("SNIPEIT_USERNAME", "admin"),
        Password: Env("SNIPEIT_PASSWORD", "password"),
        ServerTimeZoneId: Env("SNIPEIT_TIMEZONE", "America/New_York"));

    private static string Env(string name, string fallback)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }
}

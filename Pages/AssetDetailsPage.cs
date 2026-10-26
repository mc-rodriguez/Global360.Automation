using System.Globalization;
using System.Text.RegularExpressions;
using Global360.Automation.Models;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace Global360.Automation.Pages;

public sealed class AssetDetailsPage(IPage page)
{
    private const string HistoryDateFormat = "ddd MMM d, yyyy h:mmtt";

    private ILocator Details => page.Locator("#details");
    private ILocator HistoryPanel => page.Locator("#history");

    public async Task ExpectDetailsAsync(AssetData asset, string userName)
    {
        await Expect(page.GetByRole(AriaRole.Heading, new() { Level = 1 }))
            .ToContainTextAsync(asset.DisplayTitle);
        await Expect(Details.GetByRole(AriaRole.Definition).Filter(new() { HasText = asset.Tag })).ToHaveCountAsync(1);
        await Expect(Details).ToContainTextAsync(asset.Status);
        await Expect(Details).ToContainTextAsync(AssetData.DeployedBadge);
        await Expect(Details.GetByRole(AriaRole.Link, new() { Name = userName, Exact = true })).ToBeVisibleAsync();
    }

    public async Task OpenHistoryTabAsync()
    {
        await page.Locator("a[href='#history']").ClickAsync();
        await Expect(HistoryPanel).ToBeVisibleAsync();
    }

    /// <summary>
    /// Asserts the history holds exactly the "create new" and "checkout" entries for this asset.
    /// </summary>
    public async Task ExpectHistoryAsync(
        AssetData asset, string userName, string actor, DateTimeOffset testStartedAt, TimeZoneInfo serverTimeZone)
    {
        var rows = HistoryPanel.GetByRole(AriaRole.Row).Filter(new() { HasText = asset.DisplayTitle });
        await Expect(rows).ToHaveCountAsync(2);

        var create = rows.Filter(new() { HasText = "create new" });
        await Expect(create).ToHaveCountAsync(1);
        await Expect(create).ToContainTextAsync(actor);

        var checkout = rows.Filter(new() { HasText = "checkout" });
        await Expect(checkout).ToHaveCountAsync(1);
        await Expect(checkout).ToContainTextAsync(actor);
        await Expect(checkout).ToContainTextAsync(userName);

        await AssertRecentAsync(create, testStartedAt, serverTimeZone);
        await AssertRecentAsync(checkout, testStartedAt, serverTimeZone);
    }

    // History timestamps have minute precision and are rendered in the server's time zone.
    private static async Task AssertRecentAsync(ILocator row, DateTimeOffset testStartedAt, TimeZoneInfo serverTimeZone)
    {
        var text = Regex.Replace(await row.InnerTextAsync(), @"\s+", " ");
        var match = Regex.Match(text, @"[A-Z][a-z]{2} [A-Z][a-z]{2} \d{1,2}, \d{4} \d{1,2}:\d{2}[AP]M");
        Assert.That(match.Success, $"No timestamp found in history row: '{text}'");

        var local = DateTime.ParseExact(match.Value, HistoryDateFormat, CultureInfo.InvariantCulture);
        var created = new DateTimeOffset(local, serverTimeZone.GetUtcOffset(local));

        Assert.That(created, Is.EqualTo(testStartedAt).Within(TimeSpan.FromMinutes(10)),
            $"History timestamp '{match.Value}' is not close to the test run time");
    }
}

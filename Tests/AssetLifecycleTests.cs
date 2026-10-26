using Global360.Automation.Config;
using Global360.Automation.Models;
using Global360.Automation.Pages;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework.Interfaces;

namespace Global360.Automation.Tests;

[TestFixture]
public class AssetLifecycleTests : PageTest
{
    private readonly TestSettings _settings = TestSettings.Load();

    public override BrowserNewContextOptions ContextOptions() => new()
    {
        BaseURL = _settings.BaseUrl,
        Locale = "en-US",
        ViewportSize = new() { Width = 1600, Height = 900 },
    };

    [SetUp]
    public async Task StartTracing()
    {
        SetDefaultExpectTimeout(15_000);
        await Context.Tracing.StartAsync(new() { Screenshots = true, Snapshots = true, Sources = true });
    }

    [TearDown]
    public async Task StopTracing()
    {
        var failed = TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed;
        var path = failed
            ? Path.Combine(TestContext.CurrentContext.WorkDirectory, "traces", $"{TestContext.CurrentContext.Test.Name}.zip")
            : null;

        await Context.Tracing.StopAsync(new() { Path = path });
        if (path is not null)
        {
            TestContext.AddTestAttachment(path);
        }
    }

    [Test]
    public async Task CreatedCheckedOutAsset_CanBeFoundViewedAndAuditedInHistory()
    {
        var startedAt = DateTimeOffset.UtcNow;
        var serverTimeZone = TimeZoneInfo.FindSystemTimeZoneById(_settings.ServerTimeZoneId);

        var login = new LoginPage(Page);
        var create = new CreateAssetPage(Page);
        var list = new AssetsListPage(Page);
        var details = new AssetDetailsPage(Page);

        // 1. Log in
        await login.GotoAsync();
        await login.LoginAsync(_settings.Username, _settings.Password);
        await login.ExpectLoggedInAsync();

        // 2. Create a Macbook Pro 13" (Ready to Deploy) checked out to a random user
        await create.GotoAsync();
        var (asset, userName) = await create.CreateCheckedOutAssetAsync(AssetData.NewMacbook());
        await list.ExpectCreatedAsync(asset.Tag);
        TestContext.Progress.WriteLine($"Created {asset.DisplayTitle}, checked out to {userName}");

        // 3. Find the new asset in the assets list
        await list.SearchAsync(asset.Tag);
        await list.ExpectSingleCheckedOutRowAsync(asset, userName);

        // 4. Open the asset page from the list and validate the creation details
        await list.OpenAssetAsync(asset.Tag);
        await details.ExpectDetailsAsync(asset, userName);

        // 5. Validate the History tab
        await details.OpenHistoryTabAsync();
        await details.ExpectHistoryAsync(asset, userName, actor: "Admin User", startedAt, serverTimeZone);

        // Demo aid: keep the browser open for inspection (headed runs only; click Resume in the Inspector to finish).
        if (Environment.GetEnvironmentVariable("PAUSE_AT_END") == "1")
        {
            await Page.PauseAsync();
        }
    }
}

using Global360.Automation.Models;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace Global360.Automation.Pages;

public sealed class AssetsListPage(IPage page)
{
    public async Task ExpectCreatedAsync(string tag)
    {
        await Expect(page.GetByRole(AriaRole.Status).Filter(new() { HasText = "created successfully" }))
            .ToContainTextAsync(tag);
    }

    public async Task SearchAsync(string text)
    {
        var search = page.GetByRole(AriaRole.Searchbox, new() { Name = "Search" });
        await search.FillAsync(text);
        await search.PressAsync("Enter");
    }

    private ILocator RowFor(string tag) =>
        page.GetByRole(AriaRole.Row).Filter(new() { HasText = tag });

    public async Task ExpectSingleCheckedOutRowAsync(AssetData asset, string userName)
    {
        var row = RowFor(asset.Tag);
        await Expect(row).ToHaveCountAsync(1);
        await Expect(row).ToContainTextAsync(asset.Model);
        await Expect(row).ToContainTextAsync(asset.Status);
        await Expect(row).ToContainTextAsync(AssetData.DeployedBadge);
        await Expect(row).ToContainTextAsync(userName);
    }

    public async Task OpenAssetAsync(string tag)
    {
        await RowFor(tag).GetByRole(AriaRole.Link, new() { Name = tag }).ClickAsync();
    }
}

using System.Text.RegularExpressions;
using Global360.Automation.Models;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace Global360.Automation.Pages;

public sealed class CreateAssetPage(IPage page)
{
    private static readonly Regex UserDisplayName = new(@"^(?<name>.+?) \(", RegexOptions.Compiled);

    public async Task GotoAsync() => await page.GotoAsync("/hardware/create");

    /// <summary>
    /// Fills the form, checks the asset out to a random user and saves.
    /// Returns the display name of the user picked.
    /// </summary>
    public async Task<string> CreateCheckedOutAssetAsync(AssetData asset)
    {
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Asset Tag", Exact = true }).FillAsync(asset.Tag);

        await SelectOptionAsync("Select a Model", asset.Model);
        await SelectOptionAsync("Select Status", asset.Status);

        // The "Checkout to" section only appears once a deployable status is chosen.
        var userName = await SelectRandomUserAsync();

        await page.GetByRole(AriaRole.Button, new() { Name = "Save" }).First.ClickAsync();
        return userName;
    }

    private async Task SelectOptionAsync(string comboboxName, string optionText)
    {
        await page.GetByRole(AriaRole.Combobox, new() { Name = comboboxName }).ClickAsync();
        await page.GetByRole(AriaRole.Option, new() { Name = optionText }).ClickAsync();
    }

    private async Task<string> SelectRandomUserAsync()
    {
        await page.GetByRole(AriaRole.Combobox, new() { Name = "Select a User" }).ClickAsync();

        // Scope to the open Select2 dropdown. Entries look like "Name (username) - #id";
        // the regex skips the loading placeholder.
        var options = page.Locator(".select2-results")
            .GetByRole(AriaRole.Option, new() { Name = "(" })
            .Filter(new() { HasTextRegex = new Regex(@"\(.+\) - #\d+") });
        await Expect(options.First).ToBeVisibleAsync();

        var index = Random.Shared.Next(await options.CountAsync());
        var option = options.Nth(index);
        var text = (await option.InnerTextAsync()).Trim();

        var match = UserDisplayName.Match(text);
        if (!match.Success)
        {
            throw new InvalidOperationException($"Unexpected user option format: '{text}'");
        }

        await option.ClickAsync();
        return match.Groups["name"].Value;
    }
}

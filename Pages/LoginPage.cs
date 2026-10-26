using System.Text.RegularExpressions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace Global360.Automation.Pages;

public sealed class LoginPage(IPage page)
{
    private ILocator Username => page.GetByRole(AriaRole.Textbox, new() { Name = "Username" });
    private ILocator Password => page.GetByRole(AriaRole.Textbox, new() { Name = "Password", Exact = true });
    private ILocator LoginButton => page.GetByRole(AriaRole.Button, new() { Name = "Login" });

    public async Task GotoAsync() => await page.GotoAsync("/login");

    public async Task LoginAsync(string username, string password)
    {
        await Username.FillAsync(username);
        await Password.FillAsync(password);
        await LoginButton.ClickAsync();
    }

    public async Task ExpectLoggedInAsync()
    {
        await Expect(page).ToHaveTitleAsync(new Regex("^Dashboard"));
    }
}

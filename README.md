# Global360.Automation

End-to-end UI test for the [Snipe-IT demo](https://demo.snipeitapp.com/login), built with
**.NET 10**, **Playwright for .NET 1.64.1** and **NUnit**.

## Scenario

`Tests/AssetLifecycleTests.cs` runs one scenario, in the order given in the brief:

1. Log in to the Snipe-IT demo.
2. Create a new **Macbook Pro 13"** asset with status **Ready to Deploy**, checked out to a **random user**.
3. Find the new asset in the Assets list (searching by its unique tag) and verify the row.
4. Open the asset page from the list and verify the tag, model, status, "Deployed" badge and assigned user.
5. Open the **History** tab and verify the `create new` and `checkout` entries (actor, item, target user, timestamp).

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PowerShell 7 (`pwsh`)](https://learn.microsoft.com/powershell/scripting/install/installing-powershell), needed by Playwright's browser installer
- Internet access to `https://demo.snipeitapp.com`

## Run

```powershell
dotnet build
pwsh bin/Debug/net10.0/playwright.ps1 install chromium
dotnet test
```

On Linux/macOS use `pwsh bin/Debug/net10.0/playwright.ps1 install --with-deps chromium`.

Watch the browser (headed), or pick another browser:

```powershell
$env:HEADED = "1"; dotnet test
$env:HEADED = "1"; $env:PAUSE_AT_END = "1"; dotnet test   # keep the browser open at the end; click Resume to finish
$env:BROWSER = "firefox"; dotnet test   # install it first: playwright.ps1 install firefox
```

## Configuration

All settings are optional environment variables; the defaults are the public demo credentials
shown on the login page.

| Variable | Default | Purpose |
|---|---|---|
| `SNIPEIT_BASE_URL` | `https://demo.snipeitapp.com` | Site under test |
| `SNIPEIT_USERNAME` | `admin` | Login user |
| `SNIPEIT_PASSWORD` | `password` | Login password |
| `SNIPEIT_TIMEZONE` | `America/New_York` | Time zone the demo renders History timestamps in |

## Design notes

- **Page Object Model** (`Pages/`): locators and page actions live in page classes; the test reads as the scenario.
- **Resilient locators**: role/label based locators and Playwright web-first assertions, with no fixed sleeps.
- **Shared demo data**: the test keeps the asset tag the application pre-fills on the create form (unique per asset) and asserts on that tag only, so other users' data does not interfere.
- **Random user**: picked from the checkout dropdown at run time; the chosen name is then verified on the list, details and History.
- **Time assertion**: History timestamps have minute precision, so they are checked to be within 10 minutes of the run, using `SNIPEIT_TIMEZONE`.
- **Diagnostics**: a Playwright trace is saved to `bin/Debug/net10.0/traces/` when a test fails
  (open with `pwsh bin/Debug/net10.0/playwright.ps1 show-trace <file>`).
- **CI**: `.github/workflows/tests.yml` builds and runs the test on every push and pull request.

## Known caveats

The demo is a public, shared instance that is reset periodically, and it can be slow. A failure caused by the
site being unavailable is not a defect in the test; rerun it.

## Project layout

```
Config/   TestSettings (environment-based configuration)
Models/   AssetData (the asset created by the scenario)
Pages/    LoginPage, CreateAssetPage, AssetsListPage, AssetDetailsPage
Tests/    AssetLifecycleTests
```

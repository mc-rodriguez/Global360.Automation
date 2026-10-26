namespace Global360.Automation.Models;

/// <summary>The asset the scenario creates and later verifies.</summary>
public sealed record AssetData(string Tag, string Model, string Status)
{
    public const string MacbookModel = "Macbook Pro 13\"";
    public const string ReadyToDeploy = "Ready to Deploy";
    public const string DeployedBadge = "Deployed";

    /// <summary>Creates a Macbook Pro 13" asset with a tag that is unique per run (the demo is shared).</summary>
    public static AssetData NewMacbook() =>
        new($"PW{DateTime.UtcNow:yyyyMMddHHmmssfff}", MacbookModel, ReadyToDeploy);

    /// <summary>Title shown in the asset page heading and history, e.g. #TAG - Macbook Pro 13".</summary>
    public string DisplayTitle => $"#{Tag} - {Model}";
}

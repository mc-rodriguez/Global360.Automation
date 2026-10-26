namespace Global360.Automation.Models;

/// <summary>The asset the scenario creates and later verifies.</summary>
public sealed record AssetData(string Tag, string Model, string Status)
{
    public const string MacbookModel = "Macbook Pro 13\"";
    public const string ReadyToDeploy = "Ready to Deploy";
    public const string DeployedBadge = "Deployed";

    /// <summary>
    /// The asset to create. The tag is left empty: the app pre-fills a unique one on the form,
    /// and the created asset is returned with that tag.
    /// </summary>
    public static AssetData NewMacbook() => new(string.Empty, MacbookModel, ReadyToDeploy);

    /// <summary>Title shown in the asset page heading and history, e.g. #TAG - Macbook Pro 13".</summary>
    public string DisplayTitle => $"#{Tag} - {Model}";
}

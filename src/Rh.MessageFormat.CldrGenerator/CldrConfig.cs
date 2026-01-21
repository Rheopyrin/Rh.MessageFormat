namespace Rh.MessageFormat.CldrGenerator;

public class CldrConfig
{
    public string GitHubApiUrl { get; set; } = string.Empty;
    public string ArchiveUrlPattern { get; set; } = string.Empty;
    public string ExtractedFolderPattern { get; set; } = string.Empty;
    public string ArchiveFilePattern { get; set; } = string.Empty;
    public CldrPaths Paths { get; set; } = new();
    public CldrOutputFiles OutputFiles { get; set; } = new();

    public string GetArchiveUrl(string version) =>
        ArchiveUrlPattern.Replace("{version}", version);

    public string GetExtractedFolder(string version) =>
        ExtractedFolderPattern.Replace("{version}", version);

    public string GetArchiveFile(string version) =>
        ArchiveFilePattern.Replace("{version}", version);
}

public class CldrPaths
{
    public string PluralsJson { get; set; } = string.Empty;
    public string OrdinalsJson { get; set; } = string.Empty;
    public string CurrenciesFolder { get; set; } = string.Empty;
    public string CurrenciesFile { get; set; } = string.Empty;
    public string DatePatternsFolder { get; set; } = string.Empty;
    public string DatePatternsFile { get; set; } = string.Empty;
    public string DateFieldsFile { get; set; } = string.Empty;
    public string UnitsFolder { get; set; } = string.Empty;
    public string UnitsFile { get; set; } = string.Empty;
    public string ListPatternsFolder { get; set; } = string.Empty;
    public string ListPatternsFile { get; set; } = string.Empty;
}

public class CldrOutputFiles
{
    public string Plurals { get; set; } = string.Empty;
    public string Ordinals { get; set; } = string.Empty;
    public string Currencies { get; set; } = string.Empty;
    public string DatePatterns { get; set; } = string.Empty;
    public string Units { get; set; } = string.Empty;
    public string ListPatterns { get; set; } = string.Empty;
}

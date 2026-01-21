using Scriban;
using Scriban.Runtime;

namespace Rh.MessageFormat.CldrGenerator.Templates;

/// <summary>
/// Engine for processing templates using Scriban.
/// </summary>
public class TemplateEngine
{
    private readonly string _templatesDir;
    private Template? _localeClassTemplate;
    private Template? _providerRegistryTemplate;

    public TemplateEngine(string templatesDir)
    {
        _templatesDir = templatesDir;
    }

    /// <summary>
    /// Generates a locale class from the LocaleClass.sbn template.
    /// </summary>
    public async Task<string> GenerateLocaleClassAsync(LocaleTemplateData data)
    {
        _localeClassTemplate ??= await LoadTemplateAsync("LocaleClass.sbn");

        var scriptObject = new ScriptObject();
        scriptObject.Import(data, renamer: member => member.Name);

        var context = new TemplateContext { MemberRenamer = member => member.Name };
        context.PushGlobal(scriptObject);

        var result = await _localeClassTemplate.RenderAsync(context);
        return result;
    }

    /// <summary>
    /// Generates the provider registry from the ProviderRegistry.sbn template.
    /// </summary>
    public async Task<string> GenerateProviderRegistryAsync(ProviderRegistryTemplateData data)
    {
        _providerRegistryTemplate ??= await LoadTemplateAsync("ProviderRegistry.sbn");

        var scriptObject = new ScriptObject();
        scriptObject.Import(data, renamer: member => member.Name);

        var context = new TemplateContext { MemberRenamer = member => member.Name };
        context.PushGlobal(scriptObject);

        var result = await _providerRegistryTemplate.RenderAsync(context);
        return result;
    }

    private async Task<Template> LoadTemplateAsync(string templateName)
    {
        var templatePath = Path.Combine(_templatesDir, templateName);
        var content = await File.ReadAllTextAsync(templatePath);
        var template = Template.Parse(content, templatePath);

        if (template.HasErrors)
        {
            var errors = string.Join(Environment.NewLine, template.Messages.Select(m => m.Message));
            throw new InvalidOperationException($"Template parsing errors in {templateName}: {errors}");
        }

        return template;
    }
}

/// <summary>
/// Data for the LocaleClass.sbn template.
/// </summary>
public class LocaleTemplateData
{
    public required string ClassName { get; init; }
    public string PluralRuleCode { get; init; } = "";
    public string OrdinalRuleCode { get; init; } = "";
    public bool HasCurrencies { get; init; }
    public string CurrencyArrayCode { get; init; } = "";
    public bool HasUnits { get; init; }
    public string UnitDictCode { get; init; } = "";
    public bool HasDatePatterns { get; init; }
    public string DatePatternsCode { get; init; } = "";
    public bool HasListPatterns { get; init; }
    public string ListPatternsCode { get; init; } = "";
    public bool HasRelativeTimeData { get; init; }
    public string RelativeTimeDictCode { get; init; } = "";
    public bool HasQuarters { get; init; }
    public string QuartersCode { get; init; } = "";
    public bool HasWeekInfo { get; init; }
    public string WeekInfoCode { get; init; } = "";
    public bool HasIntervalFormats { get; init; }
    public string IntervalFormatsCode { get; init; } = "";
}

/// <summary>
/// Data for the ProviderRegistry.sbn template.
/// </summary>
public class ProviderRegistryTemplateData
{
    public required string GeneratedDate { get; init; }
    public required int LocaleCount { get; init; }
    public required string LocaleEntries { get; init; }
}

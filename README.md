# Rh.MessageFormat for .NET

A high-performance .NET implementation of the [ICU Message Format](https://unicode-org.github.io/icu/userguide/format_parse/messages/) standard with full CLDR support.

[![CI/CD](https://github.com/Rheopyrin/Rh.Messageformat/actions/workflows/ci.yml/badge.svg)](https://github.com/Rheopyrin/Rh.Messageformat/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Rh.MessageFormat.svg)](https://www.nuget.org/packages/Rh.MessageFormat/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Features

- **ICU Message Format** - Full support for pluralization, selection, and nested messages
- **CLDR Data** - Pre-compiled locale data for 200+ locales (plurals, ordinals, currencies, units, dates, lists)
- **High Performance** - Hand-written parser, no regex, compiled plural rules, pattern caching
- **Custom Formatters** - Extend with your own formatting functions
- **Rich Text Tags** - Support for HTML/Markdown-style tags in messages
- **AOT Compatible** - Fully trimmable and Native AOT ready
- **Modern .NET** - Targets .NET 8.0 and .NET 10.0

## Installation

```bash
dotnet add package Rh.MessageFormat
```

Or via Package Manager:

```
Install-Package Rh.MessageFormat
```

## Quick Start

```csharp
using Rh.MessageFormat;

// Create a formatter for a specific locale
var formatter = new MessageFormatter("en");

// Format a message with pluralization
var result = formatter.FormatMessage(
    "You have {count, plural, one {# notification} other {# notifications}}",
    new Dictionary<string, object?> { { "count", 5 } }
);
// Result: "You have 5 notifications"
```

## Message Syntax

### Simple Replacement

```csharp
formatter.FormatMessage("Hello, {name}!", new { name = "World" });
// Result: "Hello, World!"
```

### Pluralization

```csharp
var pattern = @"{count, plural,
    zero {No items}
    one {One item}
    =42 {The answer}
    other {# items}
}";

formatter.FormatMessage(pattern, new { count = 0 });  // "No items"
formatter.FormatMessage(pattern, new { count = 1 });  // "One item"
formatter.FormatMessage(pattern, new { count = 42 }); // "The answer"
formatter.FormatMessage(pattern, new { count = 5 });  // "5 items"
```

Plural categories supported: `zero`, `one`, `two`, `few`, `many`, `other`

### Ordinals

```csharp
var pattern = "{position, selectordinal, one {#st} two {#nd} few {#rd} other {#th}}";

formatter.FormatMessage(pattern, new { position = 1 });  // "1st"
formatter.FormatMessage(pattern, new { position = 2 });  // "2nd"
formatter.FormatMessage(pattern, new { position = 3 });  // "3rd"
formatter.FormatMessage(pattern, new { position = 4 });  // "4th"
```

### Selection

```csharp
var pattern = "{gender, select, male {He} female {She} other {They}} liked your post";

formatter.FormatMessage(pattern, new { gender = "female" });
// Result: "She liked your post"
```

### Number Formatting

```csharp
var formatter = new MessageFormatter("en");

// Basic number
formatter.FormatMessage("{n, number}", new { n = 1234.56 });
// Result: "1,234.56"

// Currency (using ICU skeleton)
formatter.FormatMessage("{price, number, ::currency/USD}", new { price = 99.99 });
// Result: "$99.99"

// Percent
formatter.FormatMessage("{rate, number, percent}", new { rate = 0.15 });
// Result: "15%"

// Compact notation
formatter.FormatMessage("{n, number, ::compact-short}", new { n = 1500000 });
// Result: "1.5M"
```

### Date and Time Formatting

```csharp
var now = DateTime.Now;

// Date with styles: short, medium, long, full
formatter.FormatMessage("{d, date, short}", new { d = now });
formatter.FormatMessage("{d, date, long}", new { d = now });

// Time with styles: short, medium, long, full
formatter.FormatMessage("{t, time, short}", new { t = now });

// DateTime combined
formatter.FormatMessage("{dt, datetime, medium}", new { dt = now });
```

### List Formatting

```csharp
var items = new[] { "Apple", "Banana", "Cherry" };

// Conjunction (and)
formatter.FormatMessage("{items, list}", new { items });
// Result: "Apple, Banana, and Cherry"

// Disjunction (or)
formatter.FormatMessage("{items, list, disjunction}", new { items });
// Result: "Apple, Banana, or Cherry"

// Unit list
formatter.FormatMessage("{items, list, unit}", new { items });
// Result: "Apple, Banana, Cherry"
```

### Nested Messages

Messages can be nested to any depth:

```csharp
var pattern = @"{gender, select,
    male {{count, plural, one {He has # apple} other {He has # apples}}}
    female {{count, plural, one {She has # apple} other {She has # apples}}}
    other {{count, plural, one {They have # apple} other {They have # apples}}}
}";

formatter.FormatMessage(pattern, new { gender = "female", count = 3 });
// Result: "She has 3 apples"
```

## Passing Variables

Variables can be passed using either anonymous objects or dictionaries:

```csharp
// Using anonymous objects (recommended for simplicity)
formatter.FormatMessage("Hello {name}, you have {count} messages", new { name = "John", count = 5 });

// Using dictionaries (for dynamic keys or complex scenarios)
var args = new Dictionary<string, object?> { ["name"] = "John", ["count"] = 5 };
formatter.FormatMessage("Hello {name}, you have {count} messages", args);

// Without variables (static text)
formatter.FormatMessage("Hello World", (object?)null);
```

All formatting methods support both overloads:
- `FormatMessage(string pattern, object? args = null)` - accepts anonymous types, POCOs, or null
- `FormatMessage(string pattern, IReadOnlyDictionary<string, object?> args)` - accepts dictionaries
- `FormatComplexMessage(string pattern, object? values = null)` - accepts anonymous types, POCOs, or null
- `FormatComplexMessage(string pattern, IReadOnlyDictionary<string, object?> values)` - accepts dictionaries
- `FormatHtmlMessage(string pattern, object? values = null)` - accepts anonymous types, POCOs, or null
- `FormatHtmlMessage(string pattern, IReadOnlyDictionary<string, object?> values)` - accepts dictionaries

### FormatComplexMessage - Nested Object Support

For nested objects, use `FormatComplexMessage` which flattens nested structures using `__` (double underscore) as separator:

```csharp
// Nested anonymous objects
formatter.FormatComplexMessage(
    "Hello {user__firstName} {user__lastName}!",
    new { user = new { firstName = "John", lastName = "Doe" } }
);
// Result: "Hello John Doe!"

// Deeply nested structures
formatter.FormatComplexMessage(
    "City: {address__home__city}",
    new { address = new { home = new { city = "New York" } } }
);
// Result: "City: New York"
```

### FormatHtmlMessage - Safe HTML Formatting

For messages containing HTML markup, use `FormatHtmlMessage` which HTML-escapes variable values to prevent XSS:

```csharp
formatter.FormatHtmlMessage(
    "<b>Hello {name}</b>",
    new { name = "<script>alert('xss')</script>" }
);
// Result: "<b>Hello &lt;script&gt;alert('xss')&lt;/script&gt;</b>"
```

## Configuration

### MessageFormatterOptions

```csharp
var options = new MessageFormatterOptions
{
    // IMPORTANT: DefaultFallbackLocale is null by default.
    // Set this to enable fallback when exact locale data is not found.
    DefaultFallbackLocale = "en",
    CldrDataProvider = new CldrDataProvider(),  // CLDR data source
    CultureInfoCache = new CultureInfoCache()   // CultureInfo caching
};

var formatter = new MessageFormatter("de-DE", options);
```

> **Note:** `DefaultFallbackLocale` is `null` by default. If not set and the requested locale is not found, an `InvalidLocaleException` will be thrown. Set this property to enable automatic fallback to a default locale.

### Custom Formatters

Add your own formatting functions:

```csharp
var options = new MessageFormatterOptions();

// Add a custom "money" formatter
options.CustomFormatters["money"] = (value, style, locale, culture) =>
{
    if (value is decimal amount)
        return amount.ToString("C", culture);
    return value?.ToString() ?? "";
};

var formatter = new MessageFormatter("en-US", options);
var result = formatter.FormatMessage("Total: {amount, money}", new { amount = 99.99m });
// Result: "Total: $99.99"
```

Custom formatter signature:
```csharp
delegate string CustomFormatterDelegate(
    object? value,      // The value to format
    string? style,      // Optional style argument
    string locale,      // Current locale code
    CultureInfo culture // Current culture
);
```

### Tag Handlers (Rich Text)

Process HTML/Markdown-style tags in messages:

```csharp
var options = new MessageFormatterOptions();

// Add handlers for rich text tags
options.TagHandlers["bold"] = content => $"<strong>{content}</strong>";
options.TagHandlers["link"] = content => $"<a href='#'>{content}</a>";

var formatter = new MessageFormatter("en", options);
var result = formatter.FormatMessage(
    "Click <bold>here</bold> to <link>learn more</link>",
    new Dictionary<string, object?>()
);
// Result: "Click <strong>here</strong> to <a href='#'>learn more</a>"
```

## Cached Provider

For applications that format messages in multiple locales, use `MessageFormatterCachedProvider`:

```csharp
// Create a provider with pre-initialized locales
var locales = new[] { "en", "de-DE", "fr-FR", "es", "ja" };
var provider = new MessageFormatterCachedProvider(locales, options);

// Pre-load all formatters (optional, improves first-call performance)
provider.Initialize();

// Get formatter for any locale (cached automatically)
var enFormatter = provider.GetFormatter("en");
var deFormatter = provider.GetFormatter("de-DE");

// Formatters are cached and reused
var sameFormatter = provider.GetFormatter("en"); // Same instance as enFormatter
```

On-demand caching (without pre-initialization):

```csharp
var provider = new MessageFormatterCachedProvider(options);
var formatter = provider.GetFormatter("en"); // Created and cached on first call
```

## Locale Fallback

The formatter resolves locale data using a fallback chain:

1. **Exact match** - e.g., `en-GB`
2. **Base locale** - e.g., `en-GB` → `en`
3. **Default fallback** - Configured via `DefaultFallbackLocale` (default: `null`)

> **Important:** `DefaultFallbackLocale` is `null` by default. You must explicitly set it to enable fallback behavior. If no locale can be resolved and no fallback is configured, an `InvalidLocaleException` is thrown.

```csharp
// Configure fallback locale
var options = new MessageFormatterOptions
{
    DefaultFallbackLocale = "en"  // Required for fallback behavior
};

// If "en-AU" data is not available, falls back to "en"
var formatter = new MessageFormatter("en-AU", options);
```

## Escaping

Use single quotes to escape special characters:

```csharp
// Escape braces
formatter.FormatMessage("Use '{' and '}' for variables", new { });
// Result: "Use { and } for variables"

// Escape the # symbol in plural blocks
formatter.FormatMessage("{n, plural, other {'#' is the number: #}}", new { n = 5 });
// Result: "# is the number: 5"

// Double single quote for literal quote
formatter.FormatMessage("It''s working!", new { });
// Result: "It's working!"
```

## Building from Source

### Prerequisites

- .NET 8.0 SDK or later
- Bash (for CLDR data generation on Linux/macOS) or PowerShell (Windows)

### Clone and Build

```bash
git clone https://github.com/Rheopyrin/Rh.Messageformat.git
cd Rh.Messageformat
dotnet restore
dotnet build
```

### Run Tests

```bash
dotnet test
```

### Generate CLDR Locale Data

The library ships with pre-generated CLDR data. To regenerate or customize:

**Linux/macOS:**
```bash
chmod +x src/scripts/sync-cldr.sh
./src/scripts/sync-cldr.sh
```

**Windows (PowerShell):**
```powershell
.\src\scripts\sync-cldr.ps1
```

**Options:**
```bash
./src/scripts/sync-cldr.sh \
    --version 48.1.0 \              # CLDR version (default: latest)
    --locales "en,de-DE,fr-FR" \    # Specific locales (default: all)
    --working-dir /tmp/cldr \       # Working directory for downloads
    --keep-files                    # Keep downloaded files after generation
```

The generator:
1. Downloads CLDR JSON data from the official repository
2. Parses plural/ordinal rules, currency data, units, date patterns, list patterns
3. Generates optimized C# code with compiled rules (no runtime parsing)

## Project Structure

```
src/
├── Rh.MessageFormat/              # Main library
├── Rh.MessageFormat.Abstractions/ # Interfaces and models
├── Rh.MessageFormat.CldrData/     # Generated CLDR locale data
├── Rh.MessageFormat.CldrGenerator/ # CLDR data generator tool
└── scripts/                       # Build scripts

tests/
├── Rh.MessageFormat.Tests/                        # Unit tests
├── Rh.MessageFormat.CldrGenerator.Tests/          # Generator unit tests
└── Rh.MessageFormat.CldrGenerator.Tests.Integration/ # Integration tests
```

## NuGet Packages

| Package | Description |
|---------|-------------|
| [Rh.MessageFormat](https://www.nuget.org/packages/Rh.MessageFormat/) | Main library with all formatting features |
| [Rh.MessageFormat.Abstractions](https://www.nuget.org/packages/Rh.MessageFormat.Abstractions/) | Interfaces for extensibility |
| [Rh.MessageFormat.CldrData](https://www.nuget.org/packages/Rh.MessageFormat.CldrData/) | Pre-compiled CLDR locale data |

## Performance

- **Hand-written parser** - No parser generators or regular expressions
- **Compiled plural rules** - CLDR rules pre-compiled to C# if/else chains
- **Pattern caching** - Parsed AST cached for repeated formatting
- **Lazy-loaded data** - CLDR data loaded on-demand per locale
- **StringBuilder pooling** - Reduced allocations during formatting

## License

MIT License - see [LICENSE](LICENSE) for details.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.

## Links

- [ICU Message Format Documentation](https://unicode-org.github.io/icu/userguide/format_parse/messages/)
- [CLDR Project](https://cldr.unicode.org/)
- [Unicode Plural Rules](https://cldr.unicode.org/index/cldr-spec/plural-rules)
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

### Plural Offset

The `offset` feature allows subtracting a value from the plural argument before determining the plural category. This is useful for "excluding" items from the count (e.g., "You and 3 others" instead of "4 people").

```csharp
var pattern = @"{count, plural, offset:1
    =0 {Nobody is attending}
    =1 {Only {host} is attending}
    one {{host} and # other person are attending}
    other {{host} and # other people are attending}
}";

formatter.FormatMessage(pattern, new { count = 0, host = "Alice" });  // "Nobody is attending"
formatter.FormatMessage(pattern, new { count = 1, host = "Alice" });  // "Only Alice is attending"
formatter.FormatMessage(pattern, new { count = 2, host = "Alice" });  // "Alice and 1 other person are attending"
formatter.FormatMessage(pattern, new { count = 5, host = "Alice" });  // "Alice and 4 other people are attending"
```

**Key behaviors:**
- The offset is placed after `plural,` and before the cases: `{n, plural, offset:1 ...}`
- **Exact match cases** (`=0`, `=1`, etc.) match against the **original value**, not the offset-adjusted value
- **Category selection** (`one`, `other`, etc.) uses the **offset-adjusted value**
- The `#` placeholder displays the **offset-adjusted value**
- Offset can be any number (integer, decimal, or negative)

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

### Number Skeletons

Number skeletons provide fine-grained control over number formatting using ICU skeleton syntax. Skeletons are prefixed with `::` in the style argument.

```csharp
formatter.FormatMessage("{n, number, ::skeleton-tokens}", new { n = value });
```

#### Precision

**Fraction Digits** (prefix with `.`):
```csharp
formatter.FormatMessage("{n, number, ::.00}", new { n = 3.1 });    // "3.10" - exactly 2 fraction digits
formatter.FormatMessage("{n, number, ::.##}", new { n = 3.0 });    // "3" - at most 2 fraction digits
formatter.FormatMessage("{n, number, ::.0#}", new { n = 3.1 });    // "3.1" - 1 to 2 fraction digits
formatter.FormatMessage("{n, number, ::.00*}", new { n = 3.14159 }); // "3.14159" - at least 2, unlimited max
```

**Significant Digits** (prefix with `@`):
```csharp
formatter.FormatMessage("{n, number, ::@@@}", new { n = 12345 });  // "12,300" - exactly 3 significant digits
formatter.FormatMessage("{n, number, ::@@##}", new { n = 1.5 });   // "1.5" - 2 to 4 significant digits
```

**Integer Digits** (minimum digits with leading zeros):
```csharp
formatter.FormatMessage("{n, number, ::000}", new { n = 5 });      // "005" - minimum 3 integer digits
formatter.FormatMessage("{n, number, ::integer-width/*000}", new { n = 5 }); // "005" - alternative form
```

#### Notation Styles

```csharp
// Compact notation
formatter.FormatMessage("{n, number, ::K}", new { n = 1500 });           // "2K" - compact-short
formatter.FormatMessage("{n, number, ::KK}", new { n = 1500 });          // "2 thousand" - compact-long
formatter.FormatMessage("{n, number, ::compact-short}", new { n = 1500000 }); // "2M"
formatter.FormatMessage("{n, number, ::compact-long}", new { n = 1500000 });  // "2 million"

// Scientific notation
formatter.FormatMessage("{n, number, ::scientific}", new { n = 12345 }); // "1.23E+4"

// Engineering notation (exponents in multiples of 3)
formatter.FormatMessage("{n, number, ::engineering}", new { n = 12345 }); // "12.35E+3"
```

#### Sign Display

| Verbose | Concise | Description |
|---------|---------|-------------|
| `sign-always` | `+!` | Always show sign (+42, -42) |
| `sign-never` | `+_` | Never show sign (42 for both positive and negative) |
| `sign-except-zero` | `+?` | Show sign except for zero |
| `sign-accounting` | `()` | Accounting format for negatives: (100) |
| `sign-accounting-always` | - | Always show sign in accounting format |

```csharp
formatter.FormatMessage("{n, number, ::sign-always}", new { n = 42 });    // "+42"
formatter.FormatMessage("{n, number, ::+!}", new { n = 42 });             // "+42"
formatter.FormatMessage("{n, number, ::sign-accounting}", new { n = -100 }); // "(100)"
formatter.FormatMessage("{n, number, ::()}", new { n = -100 });           // "(100)"
```

#### Grouping (Thousands Separators)

| Verbose | Concise | Description |
|---------|---------|-------------|
| `group-off` | `,_` | No grouping separators |
| `group-min2` | `,?` | Group only when 2+ digits in group |
| `group-auto` | - | Automatic grouping (default) |
| `group-always` | `,!` | Always apply grouping |

```csharp
formatter.FormatMessage("{n, number, ::group-off}", new { n = 1234567 });  // "1234567"
formatter.FormatMessage("{n, number, ::,_}", new { n = 1234567 });         // "1234567"
formatter.FormatMessage("{n, number, ::group-always}", new { n = 1234567 }); // "1,234,567"
```

#### Currency

```csharp
formatter.FormatMessage("{n, number, ::currency/USD}", new { n = 99.99 }); // "$99.99"
formatter.FormatMessage("{n, number, ::currency/EUR}", new { n = 50 });    // "€50.00"

// Currency display options
formatter.FormatMessage("{n, number, ::currency/USD unit-width-iso-code}", new { n = 100 }); // "USD 100"
formatter.FormatMessage("{n, number, ::currency/USD unit-width-full-name}", new { n = 100 }); // "100 US dollars"
formatter.FormatMessage("{n, number, ::currency/USD currency-narrow-symbol}", new { n = 100 }); // Narrow symbol variant
```

#### Units

```csharp
formatter.FormatMessage("{n, number, ::unit/meter}", new { n = 5 });           // "5 m"
formatter.FormatMessage("{n, number, ::unit/meter unit-width-full-name}", new { n = 5 }); // "5 meters"
formatter.FormatMessage("{n, number, ::unit/meter unit-width-narrow}", new { n = 5 });    // "5m"
```

#### Percent and Permille

```csharp
formatter.FormatMessage("{n, number, ::percent}", new { n = 0.1234 });    // "12%" (multiplies by 100)
formatter.FormatMessage("{n, number, ::%}", new { n = 0.1234 });          // "12%" (concise form)
formatter.FormatMessage("{n, number, ::percent .00}", new { n = 0.1234 }); // "12.34%" (with precision)
formatter.FormatMessage("{n, number, ::permille}", new { n = 0.005 });    // "5‰" (multiplies by 1000)
```

#### Scale

```csharp
formatter.FormatMessage("{n, number, ::scale/100}", new { n = 0.5 });     // "50" (multiplies by 100)
formatter.FormatMessage("{n, number, ::scale/1000}", new { n = 1.5 });    // "1,500"
```

#### Combining Options

Multiple skeleton tokens can be combined (space-separated):

```csharp
formatter.FormatMessage("{n, number, ::currency/USD sign-always}", new { n = 100 }); // "+$100.00"
formatter.FormatMessage("{n, number, ::percent .00}", new { n = 0.1234 });           // "12.34%"
formatter.FormatMessage("{n, number, ::compact-short .0}", new { n = 1234567 });     // "1.2M"
formatter.FormatMessage("{n, number, ::scale/1000 group-auto}", new { n = 1.5 });    // "1,500"
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
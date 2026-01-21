using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Rh.MessageFormat.Abstractions;
using Rh.MessageFormat.Abstractions.Interfaces;

namespace Rh.MessageFormat.CldrGenerator.Tests.Integration.Helpers;

/// <summary>
/// Helper class for compiling generated C# code using Roslyn.
/// </summary>
public sealed class GeneratedCodeCompiler
{
    private static readonly string[] RequiredAssemblies =
    {
        "System.Runtime",
        "System.Collections",
        "System.Private.CoreLib",
        "netstandard"
    };

    /// <summary>
    /// Compiles C# source files and returns the assembly.
    /// </summary>
    /// <param name="sourceFiles">Dictionary of filename to source code content.</param>
    /// <returns>Compilation result containing the assembly or diagnostics.</returns>
    public CompilationResult Compile(Dictionary<string, string> sourceFiles)
    {
        var syntaxTrees = new List<SyntaxTree>();
        foreach (var (fileName, sourceCode) in sourceFiles)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(
                sourceCode,
                CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest),
                path: fileName);
            syntaxTrees.Add(syntaxTree);
        }

        var references = GetMetadataReferences();

        var compilation = CSharpCompilation.Create(
            "GeneratedCldrData",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Debug,
                allowUnsafe: false));

        using var ms = new MemoryStream();
        var emitResult = compilation.Emit(ms);

        var diagnostics = emitResult.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error || d.Severity == DiagnosticSeverity.Warning)
            .ToList();

        if (!emitResult.Success)
        {
            return new CompilationResult
            {
                Success = false,
                Diagnostics = diagnostics,
                Assembly = null
            };
        }

        ms.Seek(0, SeekOrigin.Begin);
        var assembly = Assembly.Load(ms.ToArray());

        return new CompilationResult
        {
            Success = true,
            Diagnostics = diagnostics,
            Assembly = assembly
        };
    }

    /// <summary>
    /// Compiles all .g.cs files from a directory.
    /// </summary>
    public CompilationResult CompileDirectory(string outputDirectory)
    {
        var sourceFiles = new Dictionary<string, string>();

        foreach (var filePath in Directory.GetFiles(outputDirectory, "*.g.cs"))
        {
            var fileName = Path.GetFileName(filePath);
            var content = File.ReadAllText(filePath);
            sourceFiles[fileName] = content;
        }

        if (sourceFiles.Count == 0)
        {
            return new CompilationResult
            {
                Success = false,
                Diagnostics = new List<Diagnostic>(),
                Assembly = null,
                ErrorMessage = "No .g.cs files found in directory"
            };
        }

        return Compile(sourceFiles);
    }

    private static List<MetadataReference> GetMetadataReferences()
    {
        var references = new List<MetadataReference>();

        // Add core .NET assemblies
        var trustedAssembliesPath = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string ?? "";
        var trustedAssemblies = trustedAssembliesPath.Split(Path.PathSeparator);

        foreach (var assemblyPath in trustedAssemblies)
        {
            var assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
            if (RequiredAssemblies.Any(ra => assemblyName.Equals(ra, StringComparison.OrdinalIgnoreCase)) ||
                assemblyName.StartsWith("System.", StringComparison.OrdinalIgnoreCase))
            {
                references.Add(MetadataReference.CreateFromFile(assemblyPath));
            }
        }

        // Add reference to Abstractions assembly
        var abstractionsAssembly = typeof(ICldrLocaleData).Assembly;
        references.Add(MetadataReference.CreateFromFile(abstractionsAssembly.Location));

        return references;
    }
}

/// <summary>
/// Result of compiling generated code.
/// </summary>
public class CompilationResult
{
    public bool Success { get; init; }
    public IReadOnlyList<Diagnostic> Diagnostics { get; init; } = Array.Empty<Diagnostic>();
    public Assembly? Assembly { get; init; }
    public string? ErrorMessage { get; init; }

    public IEnumerable<Diagnostic> Errors => Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
    public IEnumerable<Diagnostic> Warnings => Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning);
}

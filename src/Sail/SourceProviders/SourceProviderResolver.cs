using System.Diagnostics.CodeAnalysis;

namespace Sail.SourceProviders;

public class SourceProviderResolver(IEnumerable<ISourceProvider> providers)
{
    private readonly ISourceProvider[] _providers = providers.ToArray();

    public static SourceProviderResolver Default { get; } = new SourceProviderResolver([
        new GistSourceProvider(),
        new GitSourceProvider(), // .git
        new GitHubSourceProvider(),
        new RemoteSourceProvider(),
    ]);

    public bool TryGetProvider(SourceProviderContext context, [NotNullWhen(true)] out ISourceProvider? detectedSourceProvider)
    {
        foreach (var sourceProvider in _providers)
        {
            if (sourceProvider.CanHandle(context))
            {
                detectedSourceProvider = sourceProvider;
                return true;
            }
        }

        detectedSourceProvider = null;
        return false;
    }
}
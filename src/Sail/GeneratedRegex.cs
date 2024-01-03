using System.Text.RegularExpressions;

namespace Sail;

public static partial class GeneratedRegex
{
    [GeneratedRegex(@"^https://gist.github.com/[^/]+/(?:(?<gistId>[^/]+)/(?<revision>(?!archive)[^/?#]+)|(?<gistId>[^/?#]+)(/|#.*)?$)")]
    public static partial Regex GistUrl();

    [GeneratedRegex(@"^(?<baseUrl>https://github.com/[^/]+/[^/]+)(?:/(?<blobOrTree>blob|tree)/(?<hashOrBranch>[^/]+)/(?<path>.*)|/)?$")]
    public static partial Regex GitHubUrl();

    [GeneratedRegex(@"^(https|git)://[^/]+/.*?\.git")]
    public static partial Regex GitUrl();

    [GeneratedRegex(@"^https?://.*$")]
    public static partial Regex RemoteUrl();
}
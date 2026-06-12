using QwertzBridge.Core.Domain;
using QwertzBridge.Core.Engine;

namespace QwertzBridge.Core.Tests;

public class ProfileResolverTests
{
    private static BridgeConfig ConfigWith(params Profile[] profiles) => new() { Profiles = [.. profiles] };

    private static readonly Profile CatchAll = new() { Name = "Default" };
    private static readonly Profile ForVs = new() { Name = "VS", ProcessNames = ["devenv"] };

    [Fact]
    public void MatchingProcess_WinsOverCatchAll()
    {
        var config = ConfigWith(ForVs, CatchAll);

        Assert.Same(ForVs, ProfileResolver.Resolve(config, "devenv"));
    }

    [Fact]
    public void Match_IsCaseInsensitive()
    {
        Assert.Same(ForVs, ProfileResolver.Resolve(ConfigWith(ForVs, CatchAll), "DevEnv"));
    }

    [Fact]
    public void Match_IgnoresExeSuffix_OnBothSides()
    {
        var withSuffix = new Profile { Name = "NP", ProcessNames = ["notepad.exe"] };
        var config = ConfigWith(withSuffix, CatchAll);

        Assert.Same(withSuffix, ProfileResolver.Resolve(config, "notepad"));
        Assert.Same(withSuffix, ProfileResolver.Resolve(config, "notepad.exe"));
    }

    [Fact]
    public void NoMatch_FallsBackToCatchAll()
    {
        Assert.Same(CatchAll, ProfileResolver.Resolve(ConfigWith(ForVs, CatchAll), "chrome"));
    }

    [Fact]
    public void NullProcessName_FallsBackToCatchAll()
    {
        Assert.Same(CatchAll, ProfileResolver.Resolve(ConfigWith(ForVs, CatchAll), null));
    }

    [Fact]
    public void NoMatchAndNoCatchAll_ReturnsEmptyProfile()
    {
        var resolved = ProfileResolver.Resolve(ConfigWith(ForVs), "chrome");

        Assert.Same(Profile.Empty, resolved);
        Assert.Empty(resolved.Rules);
    }

    [Fact]
    public void FirstCatchAll_Wins()
    {
        var second = new Profile { Name = "Second" };
        Assert.Same(CatchAll, ProfileResolver.Resolve(ConfigWith(CatchAll, second), "anything"));
    }
}

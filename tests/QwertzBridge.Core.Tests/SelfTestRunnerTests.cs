using QwertzBridge.Core.SelfTest;

namespace QwertzBridge.Core.Tests;

public class SelfTestRunnerTests
{
    [Fact]
    public void SelfTest_Succeeds()
    {
        var result = SelfTestRunner.Run();

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Lines));
        Assert.All(result.Lines, line => Assert.StartsWith("[PASS]", line));
    }
}

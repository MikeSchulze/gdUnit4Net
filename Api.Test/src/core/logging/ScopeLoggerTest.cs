// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Tests.Core.Logging;

using Api;

using GdUnit4.Core.Logging;

using Moq;
using Moq.Protected;

using static Assertions;

[TestSuite]
public class ScopeLoggerTest
{
    private Mock<ITestEngineLogger> innerMock = null!;
    private ScopeLogger logger = null!;

    [Before]
    public void Before()
    {
        innerMock = new Mock<ITestEngineLogger>();
        logger = new ScopeLogger(innerMock.Object, "abc12345");
    }

    [AfterTest]
    public void AfterTest() => innerMock.Reset();

    #region LogInfo / LogWarning / LogError routing

    [TestCase]
    public void LogInfo_TagsMessageWithScopeId()
    {
        logger.LogInfo("hello");

        innerMock.Protected().Verify("SendMessage", Times.Once(),
            ItExpr.Is<LogLevel>(l => l == LogLevel.Informational),
            ItExpr.Is<string>(s => s.Contains("[abc12345]") && s.Contains("hello")));
    }

    [TestCase]
    public void LogWarning_TagsMessageWithScopeId()
    {
        logger.LogWarning("watch out");

        innerMock.Protected().Verify("SendMessage", Times.Once(),
            ItExpr.Is<LogLevel>(l => l == LogLevel.Warning),
            ItExpr.Is<string>(s => s.Contains("[abc12345]") && s.Contains("watch out")));
    }

    [TestCase]
    public void LogError_TagsMessageWithScopeId()
    {
        logger.LogError("boom");

        innerMock.Protected().Verify("SendMessage", Times.Once(),
            ItExpr.Is<LogLevel>(l => l == LogLevel.Error),
            ItExpr.Is<string>(s => s.Contains("[abc12345]") && s.Contains("boom")));
    }

    #endregion

    #region WithSource

    [TestCase]
    public void WithSource_IncludesSourceInTag()
    {
        var sourceLogger = logger.WithSource("Godot");

        sourceLogger.LogInfo("output");

        innerMock.Protected().Verify("SendMessage", Times.Once(),
            ItExpr.Is<LogLevel>(l => l == LogLevel.Informational),
            ItExpr.Is<string>(s => s.Contains("[abc12345]") && s.Contains("Godot") && s.Contains("output")));
    }

    [TestCase]
    public void WithSource_DoesNotMutateOriginalLogger()
    {
        _ = logger.WithSource("Godot");

        logger.LogInfo("plain");

        innerMock.Protected().Verify("SendMessage", Times.Once(),
            ItExpr.Is<LogLevel>(l => l == LogLevel.Informational),
            ItExpr.Is<string>(s => !s.Contains("Godot") && s.Contains("plain")));
    }

    #endregion

    #region ScopeId

    [TestCase]
    public void ScopeId_ReturnsConstructorValue()
    {
        AssertThat(logger.ScopeId).IsEqual("abc12345");
    }

    #endregion
}

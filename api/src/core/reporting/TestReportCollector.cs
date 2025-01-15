namespace GdUnit4.Core.Reporting;

using System.Collections.Generic;
using System.Linq;

using Api;

internal sealed class TestReportCollector
{
    public List<ITestReport> Reports { get; } = new();

    public IEnumerable<ITestReport> Failures => Reports.Where(r => r.IsFailure);

    public IEnumerable<ITestReport> Errors => Reports.Where(r => r.IsError);

    public IEnumerable<ITestReport> Warnings => Reports.Where(r => r.IsWarning);

    public void Consume(ITestReport report) => Reports.Add(report);

    public void PushFront(ITestReport report) => Reports.Insert(0, report);

    public void Clear() => Reports.Clear();
}

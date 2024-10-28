namespace GdUnit4.Core.Reporting;

using System.Collections.Generic;
using System.Linq;

internal sealed class TestReportCollector
{
    private readonly List<TestReport> reports = new();

    public IEnumerable<TestReport> Reports => reports;

    public IEnumerable<TestReport> Failures => reports.Where(r => r.IsFailure);

    public IEnumerable<TestReport> Errors => reports.Where(r => r.IsError);

    public IEnumerable<TestReport> Warnings => reports.Where(r => r.IsWarning);

    public void Consume(TestReport report) => reports.Add(report);

    public void PushFront(TestReport report) => reports.Insert(0, report);

    public void Clear() => reports.Clear();
}

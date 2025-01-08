namespace GdUnit4.Core.Reporting;

using System.Collections.Generic;
using System.Linq;

internal sealed class TestReportCollector
{
    public List<TestReport> Reports { get; } = new();

    public IEnumerable<TestReport> Failures => Reports.Where(r => r.IsFailure);

    public IEnumerable<TestReport> Errors => Reports.Where(r => r.IsError);

    public IEnumerable<TestReport> Warnings => Reports.Where(r => r.IsWarning);

    public void Consume(TestReport report) => Reports.Add(report);

    public void PushFront(TestReport report) => Reports.Insert(0, report);

    public void Clear() => Reports.Clear();
}

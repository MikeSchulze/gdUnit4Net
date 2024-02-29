namespace GdUnit4;

using System.Linq;
using System.Collections.Generic;

public sealed partial class TestReportCollector : Godot.RefCounted
{
    private readonly List<TestReport> reports = new();
    public TestReportCollector()
    { }

    public void Consume(TestReport report) => reports.Add(report);

    public void PushFront(TestReport report) => reports.Insert(0, report);

    public void Clear() => reports.Clear();


    public IEnumerable<TestReport> Reports => reports;

    public IEnumerable<TestReport> Failures => reports.Where(r => r.IsFailure);

    public IEnumerable<TestReport> Errors => reports.Where(r => r.IsError);

    public IEnumerable<TestReport> Warnings => reports.Where(r => r.IsWarning);
}

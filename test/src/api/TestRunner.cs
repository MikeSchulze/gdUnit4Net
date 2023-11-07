namespace GdUnit4.Tests;

partial class TestRunner : GdUnit4.Api.TestRunner
{
	public override void _Ready()
		=> _ = RunTests();
}

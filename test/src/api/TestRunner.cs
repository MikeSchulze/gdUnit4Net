
namespace GdUnit4.Tests;

partial class TestRunner : Api.TestRunner
{
	public override void _Ready()
		=> _ = RunTests();
}

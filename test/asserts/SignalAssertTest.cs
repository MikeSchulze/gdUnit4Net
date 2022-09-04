// GdUnit generated TestSuite
using System.Threading.Tasks;

namespace GdUnit3.Asserts
{
    using static Assertions;
    using static Utils;


    [TestSuite]
    public class SignalAssertTest
    {
        // TestSuite generated from
        private const string sourceClazzPath = "D:/develop/workspace/gdUnit3Mono/src/asserts/SignalAssert.cs";


        class TestEmitter : Godot.Node2D
        {
            [Godot.Signal]
            delegate void SignalA();
            [Godot.Signal]
            delegate void SignalB(string value);
            [Godot.Signal]
            delegate void SignalC(string value, int count);

            private int frame = 0;

            public override void _Process(float delta)
            {
                switch (frame)
                {
                    case 5:
                        EmitSignal(nameof(SignalA));
                        break;
                    case 10:
                        EmitSignal(nameof(SignalB), "abc");
                        break;
                    case 15:
                        EmitSignal(nameof(SignalC), "abc", 100);
                        break;
                }
                frame++;
            }
        }

        [TestCase]
        public async Task IsEmitted()
        {
            var node = AutoFree(new TestEmitter());
            await AssertSignal(node).IsEmitted("SignalA").WithTimeout(200);
            await AssertSignal(node).IsEmitted("SignalB", "abc").WithTimeout(200);
            await AssertSignal(node).IsEmitted("SignalC", "abc", 100).WithTimeout(200);

            await AssertThrown(AssertSignal(node).IsEmitted("SignalC", "abc", 101).WithTimeout(200))
                .ContinueWith(result => result.Result?
                    .IsInstanceOf<GdUnit3.Exceptions.TestFailedException>()
                    .HasMessage("Expecting do emitting signal:\n  'SignalC(abc, 101)'\n by\n  <GdUnit3.Asserts.SignalAssertTest+TestEmitter>"));
        }

        [TestCase]
        public async Task IsNoEmitted()
        {
            var node = AddNode(new Godot.Node2D());
            await AssertSignal(node).IsNotEmitted("visibility_changed", 10).WithTimeout(100);
            await AssertSignal(node).IsNotEmitted("visibility_changed", 20).WithTimeout(100);
            await AssertSignal(node).IsNotEmitted("script_changed").WithTimeout(100);

            node.Visible = false;
            await AssertThrown(AssertSignal(node).IsNotEmitted("visibility_changed").WithTimeout(200))
                .ContinueWith(result => result.Result?
                    .IsInstanceOf<GdUnit3.Exceptions.TestFailedException>()
                    .HasMessage("Expecting do NOT emitting signal:\n  'visibility_changed()'\n by\n  <Godot.Node2D>"));
        }

        [TestCase]
        public async Task NodeChanged_EmittingSignals()
        {
            var node = AddNode(new Godot.Node2D());
            await AssertSignal(node).IsEmitted("visibility_changed").WithTimeout(200);

            node.Visible = false;
            await AssertSignal(node).IsEmitted("visibility_changed").WithTimeout(200);

            // expecting to fail, we not changed the visibility
            //node.Visible = false;
            await AssertThrown(AssertSignal(node).IsEmitted("visibility_changed").WithTimeout(200))
               .ContinueWith(result => result.Result?
                   .IsInstanceOf<GdUnit3.Exceptions.TestFailedException>()
                   .HasMessage("Expecting do emitting signal:\n  'visibility_changed()'\n by\n  <Godot.Node2D>"));

            node.Show();
            await AssertSignal(node).IsEmitted("draw").WithTimeout(200);
        }

        [TestCase]
        public void IsSignalExists()
        {
            var node = AutoFree(new Godot.Node2D());

            AssertSignal(node).IsSignalExists("visibility_changed")
                .IsSignalExists("draw")
                .IsSignalExists("visibility_changed")
                .IsSignalExists("tree_entered")
                .IsSignalExists("tree_exiting")
                .IsSignalExists("tree_exited");

            AssertThrown(() => AssertSignal(node).IsSignalExists("not_existing_signal"))
                .IsInstanceOf<GdUnit3.Exceptions.TestFailedException>()
                .HasMessage("Expecting signal exists:\n  'not_existing_signal()'\n on\n  <Godot.Node2D>");
        }
    }
}

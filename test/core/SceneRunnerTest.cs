using System.Threading.Tasks;

namespace GdUnit4.Tests
{
    using Godot;
    using static Assertions;

    [TestSuite]
    class SceneRunnerTest
    {

        [Before]
        public void Setup()
        {
            // use a dedicated FPS because we calculate frames by time
            Engine.PhysicsTicksPerSecond = 60;
        }

        [After]
        public void TearDown()
        {
            Engine.PhysicsTicksPerSecond = 60;
        }

        [TestCase]
        public void GetProperty()
        {
            ISceneRunner runner = ISceneRunner.Load("res://test/core/resources/scenes/TestScene.tscn", true);
            AssertObject(runner.GetProperty("_box1")).IsInstanceOf<Godot.ColorRect>();
            AssertObject(runner.GetProperty("_nullable")).IsNull();
            AssertThrown(() => runner.GetProperty("_invalid"))
                .IsInstanceOf<System.MissingFieldException>()
                .HasMessage("The property '_invalid' not exist on loaded scene.");
        }

        [TestCase]
        public void SetProperty()
        {
            ISceneRunner runner = ISceneRunner.Load("res://test/core/resources/scenes/TestScene.tscn", true);
            runner.SetProperty("_box1", Colors.Red);
            AssertObject(runner.GetProperty("_box1")).IsEqual(Colors.Red);
            AssertThrown(() => runner.SetProperty("_invalid", 42))
                .IsInstanceOf<System.MissingFieldException>()
                .HasMessage("The property '_invalid' not exist on loaded scene.");
        }

        [TestCase]
        public void InvokeSceneMethod()
        {
            ISceneRunner runner = ISceneRunner.Load("res://test/core/resources/scenes/TestScene.tscn", true);
            AssertString(runner.Invoke("add", 10, 12).ToString()).IsEqual("22");
            AssertThrown(() => runner.Invoke("sub", 12, 10))
                .IsInstanceOf<System.MissingMethodException>()
                .HasMessage("The method 'sub' not exist on loaded scene.");
        }

        [TestCase(Timeout = 1200)]
        public async Task AwaitForMilliseconds()
        {
            ISceneRunner runner = ISceneRunner.Load("res://test/core/resources/scenes/TestScene.tscn", true);
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            await runner.AwaitMillis(1000);
            stopwatch.Stop();
            // verify we wait around 1000 ms (using 100ms offset because timing is not 100% accurate)
            AssertInt((int)stopwatch.ElapsedMilliseconds).IsBetween(900, 1100);
        }

        [TestCase(Timeout = 2000)]
        public async Task SimulateFrames()
        {
            ISceneRunner runner = ISceneRunner.Load("res://test/core/resources/scenes/TestScene.tscn", true);

            var box1 = runner.GetProperty<Godot.ColorRect>("_box1")!;
            // initial is white
            AssertObject(box1.Color).IsEqual(Colors.White);

            // start color cycle by invoke the function 'start_color_cycle'
            runner.Invoke("start_color_cycle");

            // we wait for 10 frames
            await runner.SimulateFrames(10);
            // after 10 frame is still white
            AssertObject(box1.Color).IsEqual(Colors.White);

            // we wait 90 more frames
            await runner.SimulateFrames(90);
            // after 100 frames the box one should be changed the color
            AssertObject(box1.Color).IsNotEqual(Colors.White);
        }

        [TestCase(Timeout = 1000)]
        public async Task SimulateFramesWithDelay()
        {
            ISceneRunner runner = ISceneRunner.Load("res://test/core/resources/scenes/TestScene.tscn", true);

            var box1 = runner.GetProperty<Godot.ColorRect>("_box1")!;
            // initial is white
            AssertObject(box1.Color).IsEqual(Colors.White);

            // start color cycle by invoke the function 'start_color_cycle'
            runner.Invoke("start_color_cycle");

            // we wait for 10 frames each with a 50ms delay
            await runner.SimulateFrames(10, 50);
            // after 10 frame and in sum 500ms is should be changed to red
            AssertObject(box1.Color).IsEqual(Colors.Red);
        }

        [TestCase(Description = "Example to test a scene with do a color cycle on box one each 500ms", Timeout = 4000)]
        public async Task RunScene_ColorCycle()
        {
            ISceneRunner runner = ISceneRunner.Load("res://test/core/resources/scenes/TestScene.tscn", true);
            runner.MaximizeView();

            var box1 = runner.GetProperty<Godot.ColorRect>("_box1")!;
            // verify inital color
            AssertObject(box1.Color).IsEqual(Colors.White);

            // start color cycle by invoke the function 'start_color_cycle'
            runner.Invoke("start_color_cycle");

            // await for each color cycle is emited
            await runner.AwaitSignal("panel_color_change", box1, Colors.Red);
            AssertObject(box1.Color).IsEqual(Colors.Red);
            await runner.AwaitSignal("panel_color_change", box1, Colors.Blue);
            AssertObject(box1.Color).IsEqual(Colors.Blue);
            await runner.AwaitSignal("panel_color_change", box1, Colors.Green);
            AssertObject(box1.Color).IsEqual(Colors.Green);

            // AwaitOnSignal must fail after an maximum timeout of 500ms because no signal 'panel_color_change' with given args color=Yellow is emited
            await AssertThrown(runner.AwaitSignal("panel_color_change", box1, Colors.Yellow).WithTimeout(700))
                .ContinueWith(result => result.Result?.IsInstanceOf<GdUnit4.Executions.ExecutionTimeoutException>().HasMessage("Assertion: Timed out after 700ms."));
            // verify the box is still green
            AssertObject(box1.Color).IsEqual(Colors.Green);
        }

        [TestCase(Description = "Example to simulate the enter key is pressed to shoot a spell", Timeout = 2000)]
        public async Task RunScene_SimulateKeyPressed()
        {
            ISceneRunner runner = ISceneRunner.Load("res://test/core/resources/scenes/TestScene.tscn", true);

            // inital no spell is fired
            AssertObject(runner.FindChild("Spell")).IsNull();

            // fire spell be pressing enter key
            runner.SimulateKeyPressed(Key.Enter);
            // wait until next frame
            await runner.AwaitIdleFrame();

            // verify a spell is created
            AssertObject(runner.FindChild("Spell")).IsNotNull();

            // wait until spell is explode after around 1s
            var spell = runner.FindChild("Spell");
            // test to wait on signal with invlaid argument and must be timed out after 300ms
            await AssertThrown(spell.AwaitSignal("spell_explode", null).WithTimeout(300))
                .ContinueWith(result => result.Result?.IsInstanceOf<GdUnit4.Executions.ExecutionTimeoutException>().HasMessage("Assertion: Timed out after 300ms."));
            // now wait on signal with correct argument
            await spell.AwaitSignal("spell_explode", spell).WithTimeout(1100);

            // verify spell is removed when is explode
            AssertObject(runner.FindChild("Spell")).IsNull();
        }

        [TestCase(Description = "Example to simulate mouse pressed on buttons", Timeout = 2000)]
        public async Task RunScene_SimulateMouseEvents()
        {
            ISceneRunner runner = ISceneRunner.Load("res://test/core/resources/scenes/TestScene.tscn", true);
            runner.MaximizeView();

            var box1 = runner.GetProperty<Godot.ColorRect>("_box1")!;
            var box2 = runner.GetProperty<Godot.ColorRect>("_box2")!;
            var box3 = runner.GetProperty<Godot.ColorRect>("_box3")!;

            // verify inital colors
            AssertObject(box1.Color).IsEqual(Colors.White);
            AssertObject(box2.Color).IsEqual(Colors.White);
            AssertObject(box3.Color).IsEqual(Colors.White);

            // set mouse position to button one and simulate is pressed
            runner.SetMousePos(new Vector2(60, 20))
                .SimulateMouseButtonPressed(MouseButton.Left);

            // wait until next frame
            await runner.AwaitIdleFrame();
            // verify box one is changed to Aqua
            AssertObject(box1.Color).IsEqual(Colors.Aqua);
            AssertObject(box2.Color).IsEqual(Colors.White);
            AssertObject(box3.Color).IsEqual(Colors.White);

            // set mouse position to button two and simulate is pressed
            runner.SetMousePos(new Vector2(160, 20))
                .SimulateMouseButtonPressed(MouseButton.Left);
            // verify box two is changed to Aqua
            AssertObject(box1.Color).IsEqual(Colors.Aqua);
            AssertObject(box2.Color).IsEqual(Colors.Aqua);
            AssertObject(box3.Color).IsEqual(Colors.White);

            // set mouse position to button three and simulate is pressed
            runner.SetMousePos(new Vector2(260, 20))
                .SimulateMouseButtonPressed(MouseButton.Left);
            // verify box three is changed to red and after around 1s to Aqua
            AssertObject(box3.Color).IsEqual(Colors.Red);
            await runner.AwaitSignal("panel_color_change", box3, Colors.Aqua).WithTimeout(1100);
            AssertObject(box3.Color).IsEqual(Colors.Aqua);
        }

        [TestCase(Description = "Example to wait for a specific method result", Timeout = 5000)]
        public async Task AwaitMethod()
        {
            ISceneRunner runner = ISceneRunner.Load("res://test/core/resources/scenes/TestScene.tscn", true);

            // wait until 'color_cycle()' returns 'black'
            await runner.AwaitMethod<string>("color_cycle").IsEqual("black");

            // wait until 'color_cycle()' and expect be return `red` but should fail because it ends with `black`
            await AssertThrown(runner.AwaitMethod<string>("color_cycle").IsEqual("red"))
               .ContinueWith(result => result.Result?.HasMessage(
                    """
                    Expecting be equal:
                        "red"
                     but is
                        "black"
                    """
               ));
            // wait again for returns 'red' but with using a custom timeout of 500ms and expect is interrupted after 500ms
            await AssertThrown(runner.AwaitMethod<string>("color_cycle").IsEqual("red").WithTimeout(500))
               .ContinueWith(result => result.Result?.HasMessage("Assertion: Timed out after 500ms."));
        }

        [TestCase(Description = "Example to wait for a specific method result and used timefactor of 10", Timeout = 1000)]
        public async Task AwaitMethod_withTimeFactor()
        {
            ISceneRunner runner = ISceneRunner.Load("res://test/core/resources/scenes/TestScene.tscn", true);

            runner.SetTimeFactor(10);
            // wait until 'color_cycle()' returns 'black' (using small timeout we expect the method will now processes 10 times faster)
            await runner.AwaitMethod<string>("color_cycle").IsEqual("black").WithTimeout(300);

            // wait for returns 'red' but will never happen and expect is interrupted after 250ms
            await AssertThrown(runner.AwaitMethod<string>("color_cycle").IsEqual("red").WithTimeout(250))
               .ContinueWith(result => result.Result?.HasMessage("Assertion: Timed out after 250ms."));
        }

        [TestCase]
        public async Task AwaitSignal()
        {
            ISceneRunner runner = ISceneRunner.Load("res://test/core/resources/scenes/TestScene.tscn", true);
            var box1 = runner.GetProperty<ColorRect>("_box1")!;

            // Set max time factor to minimize waiting time checked `runner.wait_func`
            runner.Invoke("start_color_cycle");

            await runner.AwaitSignal("panel_color_change", box1, new Color(1, 0, 0)); // Red
            await runner.AwaitSignal("panel_color_change", box1, new Color(0, 0, 1)); // Blue
            await runner.AwaitSignal("panel_color_change", box1, new Color(0, 1, 0)); // Green
        }

        [TestCase]
        public async Task DisposeSceneRunner()
        {
            ISceneRunner runner = ISceneRunner.Load("res://test/core/resources/scenes/TestScene.tscn", true);
            SceneTree tree = (SceneTree)Godot.Engine.GetMainLoop();

            var currentScene = runner.Scene();
            var nodePath = currentScene.GetPath();
            // check scene is loaded and added to the root node
            AssertThat(Godot.GodotObject.IsInstanceValid(currentScene)).IsTrue();
            AssertThat(tree.Root.GetNodeOrNull(nodePath)).IsNotNull();

            await ISceneRunner.SyncProcessFrame;
            runner.Dispose();

            await ISceneRunner.SyncProcessFrame;
            // check scene is freed and removed from the root node
            AssertThat(Godot.GodotObject.IsInstanceValid(currentScene)).IsFalse();
            AssertThat(tree.Root.GetNodeOrNull(nodePath)).IsNull();
        }
    }
}

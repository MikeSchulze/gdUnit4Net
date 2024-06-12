namespace GdUnit4.Tests.Core;

using System.IO;
using System.Threading.Tasks;

using core.resources.scenes;

using Godot;

using static Assertions;

[TestSuite]
public sealed class SceneRunnerCSharpSceneTest
{
#nullable disable
    private ISceneRunner sceneRunner;
#nullable enable

    [Before]
    public void Setup() =>
        // use a dedicated FPS because we calculate frames by time
        Engine.PhysicsTicksPerSecond = 60;

    [After]
    public void TearDown()
        => Engine.PhysicsTicksPerSecond = 60;

    [BeforeTest]
    public void BeforeTest()
    {
        sceneRunner = ISceneRunner.Load("res://src/core/resources/scenes/TestSceneCSharp.tscn", true);
        AssertThat(sceneRunner).IsNotNull();
    }

    [TestCase]
    public void LoadSceneInvalidResource()
    {
        AssertThrown(() => ISceneRunner.Load("res://src/core/resources/scenes/NotExistingScene.tscn", true))
            .IsInstanceOf<FileNotFoundException>()
            .HasMessage("GdUnitSceneRunner: Can't load scene by given resource path: 'res://src/core/resources/scenes/NotExistingScene.tscn'. The resource does not exists.");
        AssertThrown(() => ISceneRunner.Load("res://src/core/resources/scenes/TestScene.gd", true))
            .IsInstanceOf<System.ArgumentException>()
            .HasMessage("GdUnitSceneRunner: The given resource: 'res://src/core/resources/scenes/TestScene.gd' is not a scene.");
    }

    [TestCase]
    public void LoadSceneByUid()
    {
        using var runner = ISceneRunner.Load("uid://cn8ucy2rheu0f", true);
        AssertThat(runner.Scene())
            .IsInstanceOf<Node2D>()
            .IsNotNull();
        AssertThrown(() => ISceneRunner.Load("uid://invalid_uid", true))
            .IsInstanceOf<FileNotFoundException>()
            .HasMessage("GdUnitSceneRunner: Can't load scene by given resource path: 'uid://invalid_uid'. The resource does not exists.");
    }

    [TestCase]
    public void LoadSceneTSCNFormat()
    {
        using var runner = ISceneRunner.Load("res://src/core/resources/scenes/SimpleScene.tscn", true);
        AssertThat(runner.Scene())
            .IsInstanceOf<Node2D>()
            .IsNotNull();
    }

    [TestCase]
    public void LoadSceneBinaryFormat()
    {
        using var runner = ISceneRunner.Load("res://src/core/resources/scenes/SimpleScene.scn", true);
        AssertThat(runner.Scene())
            .IsInstanceOf<Node2D>()
            .IsNotNull();
    }


    [TestCase]
    public void InitializeSceneBeforeAddingToSceneTree()
    {
        var currentScene = ((PackedScene)ResourceLoader.Load("res://src/core/resources/scenes/TestSceneWithInitialization.tscn")).Instantiate<TestSceneWithInitialization>();

        currentScene.Initialize();

        AssertThat(currentScene.MethodCalls.Count).IsEqual(1);
        AssertThat(currentScene.MethodCalls[0]).IsEqual("Initialize");

        using var runner = ISceneRunner.Load(currentScene, true);
        AssertThat(runner.Scene())
            .IsInstanceOf<TestSceneWithInitialization>()
            .IsSame(currentScene);

        var actualScene = ((TestSceneWithInitialization)runner.Scene());
        AssertThat(actualScene.MethodCalls.Count).IsEqual(2);
        AssertThat(actualScene.MethodCalls[0]).IsEqual("Initialize");
        AssertThat(actualScene.MethodCalls[1]).IsEqual("_Ready");
    }

    [TestCase]
    public void GetProperty()
    {
        AssertObject(sceneRunner.GetProperty("box0")).IsInstanceOf<ColorRect>().IsNotNull();
        AssertObject(sceneRunner.GetProperty("Box1")).IsInstanceOf<ColorRect>().IsNotNull();
        AssertObject(sceneRunner.GetProperty<ColorRect>("Box1")).IsInstanceOf<ColorRect>().IsNotNull();
        AssertObject(sceneRunner.GetProperty("initial_color")).IsEqual(Colors.Red);
        AssertObject(sceneRunner.GetProperty("nullable")).IsNull();
        AssertThrown(() => sceneRunner.GetProperty("_invalid"))
            .IsInstanceOf<System.MissingFieldException>()
            .HasMessage("The property '_invalid' not exist on loaded scene.");
    }

    [TestCase]
    public void SetProperty()
    {
        sceneRunner.SetProperty("initial_color", Colors.Red);
        AssertObject(sceneRunner.GetProperty("initial_color")).IsEqual(Colors.Red);
        AssertThrown(() => sceneRunner.SetProperty("_invalid", 42))
            .IsInstanceOf<System.MissingFieldException>()
            .HasMessage("The property '_invalid' not exist on loaded scene.");
    }

    [TestCase]
    public void InvokeSceneMethod()
    {
        AssertString(sceneRunner.Invoke("Add", 10, 12).ToString()).IsEqual("22");
        AssertThrown(() => sceneRunner.Invoke("Sub", 12, 10))
            .IsInstanceOf<System.MissingMethodException>()
            .HasMessage("The method 'Sub' not exist on this instance.");
    }

    [TestCase(Timeout = 1200)]
    public async Task AwaitForMilliseconds()
    {
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        await sceneRunner.AwaitMillis(1000);
        stopwatch.Stop();
        // verify we wait around 1000 ms (using 100ms offset because timing is not 100% accurate)
        AssertInt((int)stopwatch.ElapsedMilliseconds).IsBetween(900, 1100);
    }

    [TestCase(Timeout = 2000)]
    public async Task SimulateFrames()
    {
        var box1 = sceneRunner.GetProperty<ColorRect>("Box1")!;
        // initial is white
        AssertObject(box1.Color).IsEqual(Colors.White);

        // start color cycle by invoke the function 'StartColorCycle'
        sceneRunner.Invoke("StartColorCycle");

        // we wait for 10 frames
        await sceneRunner.SimulateFrames(10);
        // after 10 frame is still white
        AssertObject(box1.Color).IsEqual(Colors.White);

        // we wait 90 more frames
        await sceneRunner.SimulateFrames(90);
        // after 100 frames the box one should be changed the color
        AssertObject(box1.Color).IsNotEqual(Colors.White);
    }

    [TestCase(Timeout = 1000)]
    public async Task SimulateFramesWithDelay()
    {
        var box1 = sceneRunner.GetProperty<ColorRect>("Box1")!;
        // initial is white
        AssertObject(box1.Color).IsEqual(Colors.White);

        // start color cycle by invoke the function 'StartColorCycle'
        sceneRunner.Invoke("StartColorCycle");

        // we wait for 10 frames each with a 50ms delay
        await sceneRunner.SimulateFrames(10, 50);
        // after 10 frame and in sum 500ms is should be changed to red
        AssertObject(box1.Color).IsEqual(Colors.Red);
    }

    [TestCase(Description = "Example to test a scene with do a color cycle on box one each 500ms", Timeout = 4000)]
    public async Task RunSceneColorCycle()
    {
        sceneRunner.MaximizeView();

        var box1 = sceneRunner.GetProperty<ColorRect>("Box1")!;
        // verify initial color
        AssertObject(box1.Color).IsEqual(Colors.White);

        // start color cycle by invoke the function 'StartColorCycle'
        sceneRunner.Invoke("StartColorCycle");

        // await for each color cycle is emitted
        await sceneRunner.AwaitSignal(TestScene.SignalName.PanelColorChange, box1, Colors.Red);
        AssertObject(box1.Color).IsEqual(Colors.Red);
        await sceneRunner.AwaitSignal(TestScene.SignalName.PanelColorChange, box1, Colors.Blue);
        AssertObject(box1.Color).IsEqual(Colors.Blue);
        await sceneRunner.AwaitSignal(TestScene.SignalName.PanelColorChange, box1, Colors.Green);
        AssertObject(box1.Color).IsEqual(Colors.Green);

        // AwaitOnSignal must fail after an maximum timeout of 500ms because no signal 'panel_color_change' with given args color=Yellow is emitted
        await AssertThrown(sceneRunner.AwaitSignal(TestScene.SignalName.PanelColorChange, box1, Colors.Yellow).WithTimeout(700))
            .ContinueWith(result => result.Result?.IsInstanceOf<Executions.ExecutionTimeoutException>().HasMessage("Assertion: Timed out after 700ms."));
        // verify the box is still green
        AssertObject(box1.Color).IsEqual(Colors.Green);
    }

    [TestCase(Description = "Example to simulate the enter key is pressed to shoot a spell", Timeout = 2000)]
    public async Task RunSceneSimulateKeyPressed()
    {
        // initial no spell is fired
        AssertObject(sceneRunner.FindChild("Spell")).IsNull();

        // fire spell be pressing enter key
        sceneRunner.SimulateKeyPressed(Key.Enter);
        // wait until next frame
        await sceneRunner.AwaitIdleFrame();

        // verify a spell is created
        AssertObject(sceneRunner.FindChild("Spell")).IsNotNull();

        // wait until spell is explode after around 1s
        var spell = sceneRunner.FindChild("Spell");
        // wait spell is exploded
        await spell.AwaitSignal(Spell.SignalName.SpellExplode, spell).WithTimeout(1100);

        // verify spell is removed when is explode
        AssertObject(sceneRunner.FindChild("Spell")).IsNull();
    }

    [TestCase(Description = "Example to simulate mouse pressed on buttons", Timeout = 20000)]
    public async Task RunSceneSimulateMouseEvents()
    {
        sceneRunner.MaximizeView();
        await sceneRunner.AwaitIdleFrame();

        var box1 = sceneRunner.GetProperty<ColorRect>("Box1")!;
        var box2 = sceneRunner.GetProperty<ColorRect>("Box2")!;
        var box3 = sceneRunner.GetProperty<ColorRect>("Box3")!;

        // verify initial colors
        AssertObject(box1.Color).IsEqual(Colors.White);
        AssertObject(box2.Color).IsEqual(Colors.White);
        AssertObject(box3.Color).IsEqual(Colors.White);

        // set mouse position to button one and simulate is pressed
        sceneRunner.SetMousePos(new Vector2(60, 20))
                .SimulateMouseButtonPressed(MouseButton.Left);

        // wait until next frame
        await sceneRunner.AwaitIdleFrame();

        // verify box one is changed to Gray
        AssertObject(box1.Color).IsEqual(Colors.Gray);
        AssertObject(box2.Color).IsEqual(Colors.White);
        AssertObject(box3.Color).IsEqual(Colors.White);

        // set mouse position to button two and simulate is pressed
        sceneRunner.SetMousePos(new Vector2(160, 20))
            .SimulateMouseButtonPressed(MouseButton.Left);
        // verify box two is changed to Gray
        AssertObject(box1.Color).IsEqual(Colors.Gray);
        AssertObject(box2.Color).IsEqual(Colors.Gray);
        AssertObject(box3.Color).IsEqual(Colors.White);

        // set mouse position to button three and simulate is pressed
        sceneRunner.SetMousePos(new Vector2(260, 20))
            .SimulateMouseButtonPressed(MouseButton.Left);
        // verify box three is changed to red and after around 1s to Gray
        AssertObject(box3.Color).IsEqual(Colors.Red);
        await sceneRunner.AwaitSignal(TestScene.SignalName.PanelColorChange, box3, Colors.Gray).WithTimeout(1100);
        AssertObject(box3.Color).IsEqual(Colors.Gray);
    }

    [TestCase(Description = "Example to wait for a specific method result", Timeout = 5000)]
    public async Task AwaitMethod()
    {
        // wait until 'ColorCycle()' returns 'black'
        await sceneRunner.AwaitMethod<string>("ColorCycle").IsEqual("black");

        // wait until 'ColorCycle()' and expect be return `red` but should fail because it ends with `black`
        await AssertThrown(sceneRunner.AwaitMethod<string>("ColorCycle").IsEqual("red"))
           .ContinueWith(result => result.Result?.HasMessage(
                """
                Expecting be equal:
                    "red"
                 but is
                    "black"
                """
           ));
        // wait again for returns 'red' but with using a custom timeout of 500ms and expect is interrupted after 500ms
        await AssertThrown(sceneRunner.AwaitMethod<string>("ColorCycle").IsEqual("red").WithTimeout(500))
           .ContinueWith(result => result.Result?.HasMessage("Assertion: Timed out after 500ms."));
    }

    [TestCase(Description = "Example to wait for a specific method result and used time factor of 10", Timeout = 1000)]
    public async Task AwaitMethodWithTimeFactor()
    {
        sceneRunner.SetTimeFactor(10);
        // wait until 'ColorCycle()' returns 'black' (using small timeout we expect the method will now processes 10 times faster)
        await sceneRunner.AwaitMethod<string>("ColorCycle").IsEqual("black").WithTimeout(300);

        // wait for returns 'red' but will never happen and expect is interrupted after 150ms
        await AssertThrown(sceneRunner.AwaitMethod<string>("ColorCycle").IsEqual("red").WithTimeout(150))
           .ContinueWith(result => result.Result?.HasMessage("Assertion: Timed out after 150ms."));
    }

    [TestCase]
    public async Task AwaitSignal()
    {
        var box1 = sceneRunner.GetProperty<ColorRect>("Box1")!;

        // Set max time factor to minimize waiting time checked `SceneRunner.wait_func`
        sceneRunner.Invoke("StartColorCycle");

        await sceneRunner.AwaitSignal(TestScene.SignalName.PanelColorChange, box1, new Color(1, 0, 0)); // Red
        await sceneRunner.AwaitSignal(TestScene.SignalName.PanelColorChange, box1, new Color(0, 0, 1)); // Blue
        await sceneRunner.AwaitSignal(TestScene.SignalName.PanelColorChange, box1, new Color(0, 1, 0)); // Green
    }

    [TestCase]
    public async Task DisposeSceneRunner()
    {
        var sceneRunner = ISceneRunner.Load("res://src/core/resources/scenes/TestSceneCSharp.tscn", true);
        var tree = (SceneTree)Engine.GetMainLoop();

        var currentScene = sceneRunner.Scene();
        var nodePath = currentScene.GetPath();
        // check scene is loaded and added to the root node
        AssertThat(GodotObject.IsInstanceValid(currentScene)).IsTrue();
        AssertThat(tree.Root.GetNodeOrNull(nodePath)).IsNotNull();

        await ISceneRunner.SyncProcessFrame;
        sceneRunner.Dispose();

        await ISceneRunner.SyncProcessFrame;
        // check scene is freed and removed from the root node
        AssertThat(GodotObject.IsInstanceValid(currentScene)).IsFalse();
        AssertThat(tree.Root.GetNodeOrNull(nodePath)).IsNull();
    }
}

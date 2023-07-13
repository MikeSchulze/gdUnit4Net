using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;

namespace GdUnit4.Tests
{
    using Godot;
    using static Assertions;


    [TestSuite]
    class SceneRunnerInputEventIntegrationTest
    {

        //pragma warning disable CS8618
        private ISceneRunner SceneRunner;
        //pragma warning restore CS8618

        [Before]
        public void Setup()
        {
        }

        [BeforeTest]
        public void BeforeTest()
        {
            SceneRunner = ISceneRunner.Load("res://test/core/resources/scenes/TestSceneCSharp.tscn", true);
            AssertInitalMouseState();
            AssertInitalKeyState();
            // we need to maximize the view, a minimized view cannot handle mouse events see (https://github.com/godotengine/godot/issues/73461)
            SceneRunner.MaximizeView();
        }

        private void AssertInitalMouseState()
        {
            foreach (MouseButton button in Enum.GetValues(typeof(MouseButton)))
            {
                AssertThat(Input.IsMouseButtonPressed(button))
                    .OverrideFailureMessage($"Expect MouseButton {button} is not 'IsMouseButtonPressed'")
                    .IsFalse();
            }
            AssertThat((long)Input.GetMouseButtonMask()).IsEqual(0L);
        }

        private void AssertInitalKeyState()
        {
            foreach (Key key in Enum.GetValues(typeof(Key)))
            {
                AssertThat(Input.IsKeyPressed(key))
                    .OverrideFailureMessage($"Expect key {key} is not 'IsKeyPressed'")
                    .IsFalse();
                AssertThat(Input.IsPhysicalKeyPressed(key))
                    .OverrideFailureMessage($"Expect key {key} is not 'IsPhysicalKeyPressed'")
                    .IsFalse();
            }
        }

        private Vector2 ActualMousePos() => SceneRunner.Scene().GetViewport().GetMousePosition();

        // [TestCase]
        public void testSpy()
        {
            var scene = Godot.ResourceLoader.Load<PackedScene>("res://test/core/resources/scenes/TestSceneCSharp.tscn").Instantiate();


            var SceneMock = new Mock<TestScene>() { CallBase = true };
            var SceneSpy = SceneMock.Object;

            var SceneTree = (SceneTree)Godot.Engine.GetMainLoop();
            SceneTree.Root.AddChild(SceneSpy);


            SceneSpy.StartColorCycle();
            SceneMock.Verify(m => m.ColorCycle(), Times.Once);
            scene.Free();

        }

        [TestCase]
        public async Task ResetToInitalStateOnRelease()
        {
            // move mouse out of button range to avoid scene button interactons
            SceneRunner.SetMousePos(new Vector2(400, 400));
            // simulate mouse buttons and key press but we never released it
            SceneRunner.SimulateMouseButtonPress(MouseButton.Left);
            SceneRunner.SimulateMouseButtonPress(MouseButton.Right);
            SceneRunner.SimulateMouseButtonPress(MouseButton.Middle);
            SceneRunner.SimulateKeyPress(Key.Key0);
            SceneRunner.SimulateKeyPress(Key.X);
            await ISceneRunner.SyncProcessFrame;

            AssertThat(Input.IsMouseButtonPressed(MouseButton.Left)).IsTrue();
            AssertThat(Input.IsMouseButtonPressed(MouseButton.Right)).IsTrue();
            AssertThat(Input.IsMouseButtonPressed(MouseButton.Middle)).IsTrue();
            AssertThat(Input.IsKeyPressed(Key.Key0)).IsTrue();
            AssertThat(Input.IsPhysicalKeyPressed(Key.Key0)).IsTrue();
            AssertThat(Input.IsKeyPressed(Key.X)).IsTrue();
            AssertThat(Input.IsPhysicalKeyPressed(Key.X)).IsTrue();

            // unreference the scene SceneRunner to enforce reset to initial Input state
            SceneRunner.Dispose();
            await ISceneRunner.SyncProcessFrame;

            AssertThat(Input.IsMouseButtonPressed(MouseButton.Left)).IsFalse();
            AssertThat(Input.IsMouseButtonPressed(MouseButton.Right)).IsFalse();
            AssertThat(Input.IsMouseButtonPressed(MouseButton.Middle)).IsFalse();
            AssertThat(Input.IsKeyPressed(Key.Key0)).IsFalse();
            AssertThat(Input.IsPhysicalKeyPressed(Key.Key0)).IsFalse();
            AssertThat(Input.IsKeyPressed(Key.X)).IsFalse();
            AssertThat(Input.IsPhysicalKeyPressed(Key.X)).IsFalse();
        }

        [TestCase]
        public async Task SimulateKeyPress()
        {
            Key[] keys = { Key.A, Key.D, Key.X, Key.Key0 };

            foreach (Key key in keys)
            {
                SceneRunner.SimulateKeyPress(key);
                await ISceneRunner.SyncProcessFrame;

                var eventKey = new InputEventKey();
                eventKey.Keycode = key;
                eventKey.PhysicalKeycode = key;
                eventKey.Pressed = true;
                //Verify(_scene_spy, 1)._input(eventKey);
                AssertThat(Input.IsKeyPressed(key)).IsTrue();
            }

            AssertThat(Input.IsKeyPressed(Key.A)).IsTrue();
            AssertThat(Input.IsKeyPressed(Key.D)).IsTrue();
            AssertThat(Input.IsKeyPressed(Key.X)).IsTrue();
            AssertThat(Input.IsKeyPressed(Key.Key0)).IsTrue();

            AssertThat(Input.IsKeyPressed(Key.B)).IsFalse();
            AssertThat(Input.IsKeyPressed(Key.G)).IsFalse();
            AssertThat(Input.IsKeyPressed(Key.Z)).IsFalse();
            AssertThat(Input.IsKeyPressed(Key.Key1)).IsFalse();
        }

        [TestCase]
        public async Task SmulateKeyPressWithModifiers()
        {
            // press shift key + A
            SceneRunner
                .SimulateKeyPress(Key.Shift)
                .SimulateKeyPress(Key.A);
            await ISceneRunner.SyncProcessFrame;

            // results in two events, first is the shift key is press
            var eventKey = new InputEventKey();
            eventKey.Keycode = Key.Shift;
            eventKey.PhysicalKeycode = Key.Shift;
            eventKey.Pressed = true;
            eventKey.ShiftPressed = true;
            //verify(_scene_spy, 1)._input(mouseEvent)

            // second is the comnbination of current press shift and key A
            eventKey = new InputEventKey();
            eventKey.Keycode = Key.A;
            eventKey.PhysicalKeycode = Key.A;
            eventKey.Pressed = true;
            eventKey.ShiftPressed = true;
            //verify(_scene_spy, 1)._input(mouseEvent)
            AssertThat(Input.IsKeyPressed(Key.Shift)).IsTrue();
            AssertThat(Input.IsKeyPressed(Key.A)).IsTrue();
        }

        [TestCase]
        public async Task SimulateManyKeysPress()
        {
            //press and hold keys W and Z
            SceneRunner
                .SimulateKeyPress(Key.W)
                .SimulateKeyPress(Key.Z);
            await ISceneRunner.SyncProcessFrame;

            AssertThat(Input.IsKeyPressed(Key.W)).IsTrue();
            AssertThat(Input.IsPhysicalKeyPressed(Key.W)).IsTrue();
            AssertThat(Input.IsKeyPressed(Key.Z)).IsTrue();
            AssertThat(Input.IsPhysicalKeyPressed(Key.Z)).IsTrue();

            // now release key w
            SceneRunner.SimulateKeyRelease(Key.W);
            await ISceneRunner.SyncProcessFrame;

            AssertThat(Input.IsKeyPressed(Key.W)).IsFalse();
            AssertThat(Input.IsPhysicalKeyPressed(Key.W)).IsFalse();
            AssertThat(Input.IsKeyPressed(Key.Z)).IsTrue();
            AssertThat(Input.IsPhysicalKeyPressed(Key.Z)).IsTrue(); ;
        }

        [TestCase]
        public async Task SimulateSetMousePos()
        {
            // save current global mouse pos
            var gmp = SceneRunner.GetGlobalMousePosition();
            // set mouse to pos 100, 100
            SceneRunner.SetMousePos(new Vector2(100, 100));
            await ISceneRunner.SyncProcessFrame;

            AssertThat(ActualMousePos()).IsEqual(new Vector2(100, 100));

            var mouseEvent = new InputEventMouseMotion();
            mouseEvent.Position = new Vector2(100, 100);
            mouseEvent.GlobalPosition = gmp;
            //verify(_scene_spy, 1)._input(mouseEvent)

            // set mouse to pos 800, 400
            gmp = SceneRunner.GetGlobalMousePosition();
            SceneRunner.SetMousePos(new Vector2(800, 400));
            await ISceneRunner.SyncProcessFrame;

            AssertThat(ActualMousePos()).IsEqual(new Vector2(800, 400));

            mouseEvent = new InputEventMouseMotion();
            mouseEvent.Position = new Vector2(800, 400);
            mouseEvent.GlobalPosition = gmp;
            //verify(_scene_spy, 1)._input(mouseEvent)

            // and again back to 100,100
            gmp = SceneRunner.GetGlobalMousePosition();
            SceneRunner.SetMousePos(new Vector2(100, 100));
            await ISceneRunner.SyncProcessFrame;

            AssertThat(ActualMousePos()).IsEqual(new Vector2(100, 100));

            mouseEvent = new InputEventMouseMotion();
            mouseEvent.Position = new Vector2(100, 100);
            mouseEvent.GlobalPosition = gmp;
            //verify(_scene_spy, 1)._input(mouseEvent)
        }

        [TestCase]
        public async Task SimulateSetMousePosWithModifiers()
        {
            var isAlt = false;
            var isControl = false;
            var isShift = false;

            Key[] modifiers = { Key.Shift, Key.Ctrl, Key.Alt };
            MouseButton[] buttons = { MouseButton.Left, MouseButton.Middle, MouseButton.Right };

            foreach (Key modifier in modifiers)
            {
                isAlt = isAlt || Key.Alt == modifier;
                isControl = isControl || Key.Ctrl == modifier;
                isShift = isShift || Key.Shift == modifier;

                foreach (MouseButton mouse_button in buttons)
                {
                    // simulate press shift, set mouse pos and final press mouse button
                    var gmp = SceneRunner.GetGlobalMousePosition();

                    SceneRunner.SimulateKeyPress(modifier);
                    SceneRunner.SetMousePos(Vector2.Zero);
                    SceneRunner.SimulateMouseButtonPress(mouse_button);
                    await ISceneRunner.SyncProcessFrame;

                    var mouseEvent = new InputEventMouseButton();
                    mouseEvent.Position = Vector2.Zero;
                    mouseEvent.GlobalPosition = gmp;
                    mouseEvent.AltPressed = isAlt;
                    mouseEvent.CtrlPressed = isControl;
                    mouseEvent.ShiftPressed = isShift;
                    mouseEvent.Pressed = true;
                    mouseEvent.ButtonIndex = mouse_button;
                    mouseEvent.ButtonMask = GdUnit4.Core.SceneRunner.toMouseButtonMask(mouse_button);
                    //verify(_scene_spy, 1)._input(mouseEvent)
                    AssertThat(ActualMousePos()).IsEqual(Vector2.Zero);
                    AssertThat(Input.IsMouseButtonPressed(mouse_button)).IsTrue();
                    AssertThat(Input.GetMouseButtonMask()).IsEqual(mouseEvent.ButtonMask);

                    // finally release it
                    SceneRunner.SimulateMouseButtonRelease(mouse_button);
                    await ISceneRunner.SyncProcessFrame;
                    AssertThat(Input.IsMouseButtonPressed(mouse_button)).IsFalse();
                    AssertThat(Input.GetMouseButtonMask()).IsEqual((MouseButtonMask)0L);
                }
            }
        }

        [TestCase]
        public async Task SimulateMouseMove()
        {
            var gmp = SceneRunner.GetGlobalMousePosition();

            SceneRunner.SimulateMouseMove(new Vector2(400, 100));
            await ISceneRunner.SyncProcessFrame;

            AssertThat(ActualMousePos()).IsEqual(new Vector2(400, 100));
            var mouseEvent = new InputEventMouseMotion();
            mouseEvent.Position = new Vector2(400, 100);
            mouseEvent.GlobalPosition = gmp;
            mouseEvent.Relative = new Vector2(400, 100) - new Vector2(10, 10);
            //verify(_scene_spy, 1)._input(mouseEvent)

            // move mouse to next pos
            gmp = SceneRunner.GetGlobalMousePosition();
            SceneRunner.SimulateMouseMove(new Vector2(55, 42));
            await ISceneRunner.SyncProcessFrame;

            AssertThat(ActualMousePos()).IsEqual(new Vector2(55, 42));
            mouseEvent = new InputEventMouseMotion();
            mouseEvent.Position = new Vector2(55, 42);
            mouseEvent.GlobalPosition = gmp;
            mouseEvent.Relative = new Vector2(55, 42) - new Vector2(400, 100);
            //verify(_scene_spy, 1)._input(mouseEvent)
        }

        [TestCase(Timeout = 4000)]
        public async Task SimulateMouseMoveRelative()
        {
            SceneRunner.SimulateMouseMove(new Vector2(10, 10));
            await ISceneRunner.SyncProcessFrame;
            // initial pos
            AssertThat(ActualMousePos()).IsEqual(new Vector2(10, 10));

            await SceneRunner.SimulateMouseMoveRelative(new Vector2(900, 400), new Vector2(.2f, 1));
            // final pos
            AssertThat(ActualMousePos()).IsEqual(new Vector2(910, 410));
        }

        [TestCase]
        public async Task SimulateMouseButtonPressLeft()
        {
            // simulate mouse button press and hold
            var gmp = SceneRunner.GetGlobalMousePosition();
            SceneRunner.SimulateMouseButtonPress(MouseButton.Left);
            await ISceneRunner.SyncProcessFrame;

            var mouseEvent = new InputEventMouseButton();
            mouseEvent.Position = Vector2.Zero;
            mouseEvent.GlobalPosition = gmp;
            mouseEvent.Pressed = true;
            mouseEvent.DoubleClick = false;
            mouseEvent.ButtonIndex = MouseButton.Left;
            mouseEvent.ButtonMask = GdUnit4.Core.SceneRunner.toMouseButtonMask(MouseButton.Left);
            //verify(_scene_spy, 1)._input(event)
            AssertThat(Input.IsMouseButtonPressed(mouseEvent.ButtonIndex)).IsTrue();
            AssertThat(Input.GetMouseButtonMask()).IsEqual(mouseEvent.ButtonMask);
        }

        [TestCase]
        public async Task SimulateMouseButtonPressLeftDoubleclick()
        {
            // simulate mouse button press double_click
            var gmp = SceneRunner.GetGlobalMousePosition();
            SceneRunner.SimulateMouseButtonPress(MouseButton.Left, true);
            await ISceneRunner.SyncProcessFrame;

            var mouseEvent = new InputEventMouseButton();
            mouseEvent.Position = Vector2.Zero;
            mouseEvent.GlobalPosition = gmp;
            mouseEvent.Pressed = true;
            mouseEvent.DoubleClick = true;
            mouseEvent.ButtonIndex = MouseButton.Left;
            mouseEvent.ButtonMask = GdUnit4.Core.SceneRunner.toMouseButtonMask(MouseButton.Left);
            //verify(_scene_spy, 1)._input(event)
            AssertThat(Input.IsMouseButtonPressed(mouseEvent.ButtonIndex)).IsTrue();
            AssertThat(Input.GetMouseButtonMask()).IsEqual(mouseEvent.ButtonMask);
        }

        [TestCase]
        public async Task SimulateMouseButtonPressRight()
        {
            // simulate mouse button press and hold
            var gmp = SceneRunner.GetGlobalMousePosition();
            SceneRunner.SimulateMouseButtonPress(MouseButton.Right);
            await ISceneRunner.SyncProcessFrame;

            var mouseEvent = new InputEventMouseButton();
            mouseEvent.Position = Vector2.Zero;
            mouseEvent.GlobalPosition = gmp;
            mouseEvent.Pressed = true;
            mouseEvent.DoubleClick = false;
            mouseEvent.ButtonIndex = MouseButton.Right;
            mouseEvent.ButtonMask = GdUnit4.Core.SceneRunner.toMouseButtonMask(MouseButton.Right);
            //verify(_scene_spy, 1)._input(event)
            AssertThat(Input.IsMouseButtonPressed(mouseEvent.ButtonIndex)).IsTrue();
            AssertThat(Input.GetMouseButtonMask()).IsEqual(mouseEvent.ButtonMask);
        }

        [TestCase]
        public async Task SimulateMouseButtonPressRightDoubleclick()
        {
            // simulate mouse button press double_click
            var gmp = SceneRunner.GetGlobalMousePosition();
            SceneRunner.SimulateMouseButtonPress(MouseButton.Right, true);
            await ISceneRunner.SyncProcessFrame;

            var mouseEvent = new InputEventMouseButton();
            mouseEvent.Position = Vector2.Zero;
            mouseEvent.GlobalPosition = gmp;
            mouseEvent.Pressed = true;
            mouseEvent.DoubleClick = true;
            mouseEvent.ButtonIndex = MouseButton.Right;
            mouseEvent.ButtonMask = GdUnit4.Core.SceneRunner.toMouseButtonMask(MouseButton.Right);
            //verify(_scene_spy, 1)._input(event)
            AssertThat(Input.IsMouseButtonPressed(mouseEvent.ButtonIndex)).IsTrue();
            AssertThat(Input.GetMouseButtonMask()).IsEqual(mouseEvent.ButtonMask);
        }

    }
}

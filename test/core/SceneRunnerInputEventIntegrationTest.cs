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

#pragma warning disable CS8618
        private ISceneRunner SceneRunner;
#pragma warning restore CS8618

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


        [TestCase]
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
        public async Task TestSimulateKeyPress()
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
    }
}

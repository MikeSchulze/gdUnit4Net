namespace NUnit.Extension.GdUnit4;

using System;
using System.Threading;
using System.Threading.Tasks;

using Framework;

using Godot;

public class ExampleTest
{
    [GodotTest]
    public async Task GodotSceneTest()
    {
        var sceneTree = (SceneTree)Godot.Engine.GetMainLoop();
        var testScene = ((PackedScene)ResourceLoader.Load("res://TestScene.tscn")).Instantiate();
        GD.PrintS("Run GodotSceneTest", testScene);
        if (testScene != null)
            sceneTree.Root.AddChild(testScene);
        using var tokenSource = new CancellationTokenSource();
        await Task.Delay(TimeSpan.FromMilliseconds(1000), tokenSource.Token);

        Assert.That("Test", Is.EqualTo("Test"));
    }

    [GodotTest]
    public void GodotNode3D()
    {
        GD.PrintS("Run test GodotNode3D");
        var node = new Node3D();
        Assert.That(node, Is.Not.Null);
        node.Free();
    }

    [Test]
    public void NunitTest() => Assert.That("Test", Is.EqualTo("Test"));
}

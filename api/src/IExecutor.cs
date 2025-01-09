namespace GdUnit4;

using System;

using Godot;

public interface IExecutor : IDisposable
{
    /// <summary>
    ///     Checks whether the specified node can be executed by this executor implementation.
    /// </summary>
    /// <param name="node">The node to be check is executable</param>
    /// <returns></returns>
    public bool IsExecutable(Node node);

    // this method is called form gdScript and can't handle 'Task'
    // we used explicit 'async void' to avoid  'Attempted to convert an unmarshal-able managed type to Variant Task'
    public void Execute(CsNode testSuite);

    public IExecutor AddGdTestEventListener(GodotObject listener);
}

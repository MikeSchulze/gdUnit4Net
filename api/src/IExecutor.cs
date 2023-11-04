using System;

namespace GdUnit4
{

    public interface IExecutor : IDisposable
    {
        /// <summary>Checks whether the specified node can be executed by this executor implementation.</summary>
        public bool IsExecutable(Godot.Node node);

        // this method is called form gdScript and can't handle 'Task'
        // we used explicit 'async void' to avoid  'Attempted to convert an unmarshallable managed type to Variant Task'
        public void Execute(CsNode node);


        public IExecutor AddGdTestEventListener(Godot.GodotObject listener);



    }
}

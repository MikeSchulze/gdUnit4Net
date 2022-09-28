using System;

namespace GdUnit3
{

    public interface IExecutor : IDisposable
    {
        // this method is called form gdScript and can't handle 'Task'
        // we used explicit 'async void' to avoid  'Attempted to convert an unmarshallable managed type to Variant Task'
        public void Execute(CsNode node);


        public IExecutor AddGdTestEventListener(Godot.Object listener);

    }
}
namespace GdUnit3
{

    public interface IExecutor
    {

        [Godot.Signal] private delegate void ExecutionCompleted();

        // this method is called form gdScript and can't handle 'Task'
        // we used explicit 'async void' to avoid  'Attempted to convert an unmarshallable managed type to Variant Task'
        public void Execute(Godot.Node node);

    }
}
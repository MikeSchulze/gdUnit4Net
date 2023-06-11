// GdUnit generated TestSuite

namespace GdUnit4.Asserts
{
    using System;

    partial class ExampleNode : Godot.Node, System.IEquatable<ExampleNode>
    {
        int Value;
        string msg;

        public ExampleNode(string msg, int value)
        {
            this.msg = msg;
            this.Value = value;
        }

        public override bool Equals(object? obj)
        {
            return obj is ExampleNode example &&
                   Value == example.Value &&
                   msg == example.msg;
        }

        public bool Equals(ExampleNode? obj)
        {
            return obj is ExampleNode example &&
                   Value == example.Value &&
                   msg == example.msg;
        }



        public override int GetHashCode() => HashCode.Combine(Value, msg);
    }
}

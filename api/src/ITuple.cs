/// A tuple implementation to hold two or many values 
using System;
using System.Collections.Generic;

namespace GdUnit4.Asserts
{
    public interface ITuple : IEquatable<object?>
    {
        public IEnumerable<object?> Values
        { get; set; }
    }
}

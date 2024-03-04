/// A tuple implementation to hold two or many values
namespace GdUnit4.Asserts;

using System;
using System.Collections.Generic;

public interface ITuple : IEquatable<object?>
{
    public IEnumerable<object?> Values { get; set; }
}

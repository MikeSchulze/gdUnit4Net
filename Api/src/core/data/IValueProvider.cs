// ReSharper disable once CheckNamespace

namespace GdUnit4;

using System.Collections.Generic;

public interface IValueProvider
{
    public IEnumerable<object> GetValues();
}

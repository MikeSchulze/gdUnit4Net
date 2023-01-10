
using System.Collections.Generic;

namespace GdUnit4
{
    public interface IValueProvider
    {
        public IEnumerable<object> GetValues();
    }
}

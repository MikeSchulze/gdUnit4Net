// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4;

using System.Collections.Generic;

public interface IValueProvider
{
    public IEnumerable<object> GetValues();
}

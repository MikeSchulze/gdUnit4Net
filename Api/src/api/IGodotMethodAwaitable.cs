// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Api;

using System.Threading.Tasks;

using Godot;

public interface IGodotMethodAwaitable<[MustBeVariant] TVariant> : IGdUnitAwaitable
{
    Task<IGodotMethodAwaitable<TVariant>> IsEqual(TVariant expected);

    Task<IGodotMethodAwaitable<TVariant>> IsNull();

    Task<IGodotMethodAwaitable<TVariant>> IsNotNull();
}
